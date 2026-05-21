using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Helpers;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Modelo auxiliar para estadísticas del Dashboard.
    /// </summary>
    public class EstadisticaItem
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
    }

    /// <summary>
    /// Controlador Reporte de Accidente de Trabajo
    /// Gestiona el registro, edición, consulta, descarga y estadísticas
    /// de reportes de accidentes laborales.
    /// </summary>
    /// <remarks>
    /// Funcionalidades:
    ///  - Registro de reportes
    ///  - Edición de reportes
    ///  - Consulta de detalle
    ///  - Descarga de archivos PDF adjuntos
    ///  - Dashboard con estadísticas y filtros
    ///
    /// Ubicación de archivos:
    ///  ~/Uploads/AccidentesTrabajo/
    /// </remarks>
    public class ReporteAccidenteTrabajoController : Controller
    {
        #region [1] Campos y Constantes

        /// <summary>Contexto de base de datos.</summary>
        private readonly SSTContext db = new SSTContext();

        /// <summary>Subcarpeta de almacenamiento.</summary>
        private const string SubCarpeta = "Accidentes";

        /// <summary>Ruta virtual de almacenamiento.</summary>
        private const string CarpetaVirtual = "~/Uploads/Accidentes/";

        #endregion

        #region [2] Métodos GET

        /// <summary>
        /// [2.1] Lista todos los reportes ordenados por fecha de accidente descendente.
        /// </summary>
        public ActionResult Index()
        {
            var reportes = db.ReportesAccidenteTrabajo
                .Include("Proceso")
                .Include("Subproceso")
                .OrderByDescending(r => r.FechaAT)
                .ToList();

            return View(reportes);
        }

        /// <summary>
        /// [2.2] Dashboard con estadísticas y gráficos de accidentes de trabajo.
        /// Permite filtrar por año, mes o trimestre.
        /// </summary>
        public ActionResult Dashboard(int? anio, int? mes, int? trimestre)
        {
            int filtroAnio = anio ?? DateTime.Today.Year;

            var query = db.ReportesAccidenteTrabajo
                .Include("Proceso")
                .AsQueryable();

            query = query.Where(r => r.FechaAT.Year == filtroAnio);

            if (mes.HasValue && mes.Value > 0)
            {
                query = query.Where(r => r.FechaAT.Month == mes.Value);
            }
            else if (trimestre.HasValue && trimestre.Value > 0)
            {
                int mesInicio = (trimestre.Value - 1) * 3 + 1;
                int mesFin = mesInicio + 2;
                query = query.Where(r => r.FechaAT.Month >= mesInicio &&
                                         r.FechaAT.Month <= mesFin);
            }

            var datos = query.ToList();

            // ── Tarjetas resumen ──────────────────────────────────────────
            ViewBag.TotalAccidentes = datos.Count;
            ViewBag.TotalIncapacidad = datos.Sum(r => (int?)r.DiasIncapacidad) ?? 0;

            // ── Top 5 procesos con más accidentes ─────────────────────────
            ViewBag.TopProcesos = datos
                .GroupBy(r => r.Proceso != null ? r.Proceso.NombreProceso : "Sin Proceso")
                .Select(g => new EstadisticaItem { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            // ── Distribución por tipo de peligro ──────────────────────────
            ViewBag.EstadisticasPeligros = datos
                .GroupBy(r => !string.IsNullOrWhiteSpace(r.TipoPeligro)
                               ? r.TipoPeligro : "No definido")
                .Select(g => new EstadisticaItem { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // ── Tabla de causas más frecuentes ────────────────────────────
            ViewBag.EstadisticasCausas = datos
                .Where(r => !string.IsNullOrWhiteSpace(r.Causa))
                .GroupBy(r => r.Causa)
                .Select(g => new EstadisticaItem { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // ── Distribución por género ───────────────────────────────────
            ViewBag.EstadisticasGenero = datos
                .GroupBy(r => !string.IsNullOrWhiteSpace(r.Genero)
                               ? r.Genero : "No definido")
                .Select(g => new EstadisticaItem { Nombre = g.Key, Cantidad = g.Count() })
                .ToList();

            // ── Accidentes por mes (para gráfico de línea/barra) ──────────
            ViewBag.AccidentesPorMes = datos
                .GroupBy(r => r.FechaAT.Month)
                .Select(g => new EstadisticaItem
                {
                    Nombre = System.Globalization.CultureInfo
                                   .CurrentCulture.DateTimeFormat
                                   .GetMonthName(g.Key),
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Nombre)
                .ToList();

            // ── Filtros activos para la vista ─────────────────────────────
            ViewBag.AnioActual = filtroAnio;
            ViewBag.MesActual = mes;
            ViewBag.TrimestreActual = trimestre;

            return View();
        }

        /// <summary>
        /// [2.3] Muestra el formulario para registrar un nuevo reporte.
        /// </summary>
        public ActionResult Registrar()
        {
            CargarListas();

            return View(new ReporteAccidenteTrabajo
            {
                FechaAT = DateTime.Today,
                DiasIncapacidad = 0
            });
        }

        /// <summary>
        /// [2.4] Muestra el formulario de edición de un reporte.
        /// </summary>
        public ActionResult Editar(int id)
        {
            var reporte = db.ReportesAccidenteTrabajo
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(r => r.IdReporteAccidenteTrabajo == id);

            if (reporte == null)
            {
                TempData["Error"] = "No se encontró el reporte con ID " + id + ".";
                return RedirectToAction("Index");
            }

            CargarListas();
            return View(reporte);
        }

        /// <summary>
        /// [2.5] Muestra el detalle completo de un reporte.
        /// </summary>
        public ActionResult Detalle(int id)
        {
            var reporte = db.ReportesAccidenteTrabajo
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(r => r.IdReporteAccidenteTrabajo == id);

            if (reporte == null)
            {
                TempData["Error"] = "No se encontró el reporte con ID " + id + ".";
                return RedirectToAction("Index");
            }

            return View(reporte);
        }

        /// <summary>
        /// [2.6] Descarga un archivo PDF asociado al reporte.
        /// </summary>
        public ActionResult Descargar(string ruta, string nombre)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                TempData["Error"] = "La ruta del archivo no es válida.";
                return RedirectToAction("Index");
            }

            string rutaFisica = Server.MapPath("~" + ruta);

            if (!System.IO.File.Exists(rutaFisica))
            {
                TempData["Error"] = "El archivo '" + (nombre ?? ruta) +
                                    "' no se encontró en el servidor. " +
                                    "Es posible que haya sido movido o eliminado.";
                return RedirectToAction("Index");
            }

            return File(rutaFisica, "application/pdf", nombre ?? "archivo.pdf");
        }

        #endregion

        #region [3] Métodos POST

        /// <summary>
        /// [3.1] Registra un nuevo reporte de accidente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(
            ReporteAccidenteTrabajo modelo,
            HttpPostedFileBase archivoAccidente,
            HttpPostedFileBase archivoInvestigacion,
            HttpPostedFileBase archivoPlan)
        {
            if (modelo.FechaInvestigacion.HasValue)
                modelo.DiasInvestigacion =
                    (int)(modelo.FechaInvestigacion.Value - modelo.FechaAT).TotalDays;

            if (!ModelState.IsValid)
            {
                CargarListas();
                return View(modelo);
            }

            string carpeta = Server.MapPath(CarpetaVirtual);

            try { EnsureDirectory(carpeta); }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "No se pudo crear la carpeta de archivos adjuntos. " +
                    "Detalle técnico: " + ex.Message);

                CargarListas();
                return View(modelo);
            }

            string rutaAcc, nomAcc, errAcc;
            if (!GuardarArchivoPdf(archivoAccidente, carpeta, SubCarpeta,
                    out rutaAcc, out nomAcc, out errAcc))
            {
                ModelState.AddModelError("", errAcc);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoAccidente = rutaAcc;
            modelo.NombreArchivoAccidente = nomAcc;

            string rutaInv, nomInv, errInv;
            if (!GuardarArchivoPdf(archivoInvestigacion, carpeta, SubCarpeta,
                    out rutaInv, out nomInv, out errInv))
            {
                ModelState.AddModelError("", errInv);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoInvestigacion = rutaInv;
            modelo.NombreArchivoInvestigacion = nomInv;

            string rutaPlan, nomPlan, errPlan;
            if (!GuardarArchivoPdf(archivoPlan, carpeta, SubCarpeta,
                    out rutaPlan, out nomPlan, out errPlan))
            {
                ModelState.AddModelError("", errPlan);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoPlan = rutaPlan;
            modelo.NombreArchivoPlan = nomPlan;

            try
            {
                modelo.FechaCreacion = DateTime.Now;
                db.ReportesAccidenteTrabajo.Add(modelo);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Ocurrió un error al guardar el reporte en la base de datos. " +
                    "Verifique que todos los campos sean válidos. " +
                    "Detalle técnico: " + ex.Message);

                CargarListas();
                return View(modelo);
            }

            TempData["Exito"] = "Reporte registrado correctamente.";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// [3.2] Actualiza un reporte existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(
            ReporteAccidenteTrabajo modelo,
            HttpPostedFileBase archivoAccidente,
            HttpPostedFileBase archivoInvestigacion,
            HttpPostedFileBase archivoPlan)
        {
            if (modelo.FechaInvestigacion.HasValue)
                modelo.DiasInvestigacion =
                    (int)(modelo.FechaInvestigacion.Value - modelo.FechaAT).TotalDays;

            if (!ModelState.IsValid)
            {
                CargarListas();
                return View(modelo);
            }

            var original = db.ReportesAccidenteTrabajo
                .AsNoTracking()
                .FirstOrDefault(r => r.IdReporteAccidenteTrabajo == modelo.IdReporteAccidenteTrabajo);

            if (original == null)
            {
                TempData["Error"] = "No se encontró el reporte que intenta editar. " +
                                    "Es posible que haya sido eliminado.";
                return RedirectToAction("Index");
            }

            modelo.FechaCreacion = original.FechaCreacion;
            modelo.FechaModificacion = DateTime.Now;

            string carpeta = Server.MapPath(CarpetaVirtual);

            try { EnsureDirectory(carpeta); }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "No se pudo acceder a la carpeta de archivos. " +
                    "Detalle técnico: " + ex.Message);

                CargarListas();
                return View(modelo);
            }

            string rutaAcc, nomAcc;
            ActualizarArchivo(archivoAccidente, carpeta, SubCarpeta,
                original.RutaArchivoAccidente, original.NombreArchivoAccidente,
                out rutaAcc, out nomAcc);
            modelo.RutaArchivoAccidente = rutaAcc;
            modelo.NombreArchivoAccidente = nomAcc;

            string rutaInv, nomInv;
            ActualizarArchivo(archivoInvestigacion, carpeta, SubCarpeta,
                original.RutaArchivoInvestigacion, original.NombreArchivoInvestigacion,
                out rutaInv, out nomInv);
            modelo.RutaArchivoInvestigacion = rutaInv;
            modelo.NombreArchivoInvestigacion = nomInv;

            string rutaPlan, nomPlan;
            ActualizarArchivo(archivoPlan, carpeta, SubCarpeta,
                original.RutaArchivoPlan, original.NombreArchivoPlan,
                out rutaPlan, out nomPlan);
            modelo.RutaArchivoPlan = rutaPlan;
            modelo.NombreArchivoPlan = nomPlan;

            try
            {
                db.Entry(modelo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Ocurrió un error al actualizar el reporte en la base de datos. " +
                    "Detalle técnico: " + ex.Message);

                CargarListas();
                return View(modelo);
            }

            TempData["Exito"] = "Reporte actualizado correctamente.";
            return RedirectToAction("Index");
        }

        #endregion

        #region [4] Métodos Privados

        /// <summary>
        /// [4.1] Carga listas necesarias en ViewBag.
        /// </summary>
        private void CargarListas()
        {
            ViewBag.Generos = new[] { "Masculino", "Femenino", "Otro" };
            ViewBag.TiposPeligro = new[]
            {
                "Mecánico", "Biológico", "Biomecánico", "Locativo",
                "Químico", "Físico", "Psicosocial",
                "Eléctrico", "Público", "Tecnológico", "Otro"
            };
            ViewBag.Turnos = new[] { "Mañana", "Tarde", "Noche", "Día", "Adm. (M/T)" };
            ViewBag.CausasSubcausas = CausasHelper.ObtenerCausasSubcausas();
        }

        /// <summary>[4.2] Crea directorio si no existe.</summary>
        private static void EnsureDirectory(string ruta)
        {
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);
        }

        /// <summary>[4.3] Guarda archivo PDF.</summary>
        private static bool GuardarArchivoPdf(
            HttpPostedFileBase archivo,
            string carpeta,
            string subCarpeta,
            out string ruta,
            out string nombre,
            out string error)
        {
            ruta = nombre = error = null;

            if (archivo == null || archivo.ContentLength == 0)
                return true;

            string extension = Path.GetExtension(archivo.FileName).ToLower();
            if (extension != ".pdf")
            {
                error = "El archivo '" + archivo.FileName + "' no es un PDF válido. " +
                        "Solo se permiten archivos con extensión .pdf.";
                return false;
            }

            string nombreUnico = Guid.NewGuid() + "_" + Path.GetFileName(archivo.FileName);
            archivo.SaveAs(Path.Combine(carpeta, nombreUnico));
            nombre = archivo.FileName;
            ruta = "/Uploads/" + subCarpeta + "/" + nombreUnico;
            return true;
        }

        /// <summary>[4.4] Actualiza archivo PDF.</summary>
        private static void ActualizarArchivo(
            HttpPostedFileBase archivo,
            string carpeta,
            string subCarpeta,
            string rutaAnterior,
            string nombreAnterior,
            out string nuevaRuta,
            out string nuevoNombre)
        {
            if (archivo != null &&
                archivo.ContentLength > 0 &&
                Path.GetExtension(archivo.FileName).ToLower() == ".pdf")
            {
                string nombreUnico = Guid.NewGuid() + "_" + Path.GetFileName(archivo.FileName);
                archivo.SaveAs(Path.Combine(carpeta, nombreUnico));
                nuevoNombre = archivo.FileName;
                nuevaRuta = "/Uploads/" + subCarpeta + "/" + nombreUnico;
            }
            else
            {
                nuevaRuta = rutaAnterior;
                nuevoNombre = nombreAnterior;
            }
        }

        #endregion

        #region [5] Liberación de Recursos

        /// <summary>
        /// [5.1] Libera recursos del contexto.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}