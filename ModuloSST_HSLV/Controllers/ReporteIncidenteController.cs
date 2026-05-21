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
    /// Controlador Reporte Incidente
    /// Gestiona reportes de incidentes (eventos con potencial de daño sin accidente).
    /// </summary>
    /// <remarks>
    /// Incluye:
    ///  - Registro
    ///  - Edición
    ///  - Consulta
    ///  - Manejo de archivos PDF
    ///  - Dashboard de indicadores
    /// </remarks>
    public class ReporteIncidenteController : Controller
    {
        #region [1] Campos y Constantes

        /// <summary>Contexto de base de datos.</summary>
        private readonly SSTContext db = new SSTContext();

        /// <summary>Nombre de subcarpeta física.</summary>
        private const string SubCarpeta = "Incidentes";

        /// <summary>Ruta virtual de almacenamiento.</summary>
        private const string CarpetaVirtual = "~/Uploads/Incidentes/";

        #endregion

        #region [2] Métodos GET

        /// <summary>
        /// [2.1] Lista los reportes ordenados por fecha descendente.
        /// </summary>
        public ActionResult Index()
        {
            var reportes = db.ReportesIncidente
                .Include("Proceso")
                .Include("Subproceso")
                .OrderByDescending(r => r.FechaIncidente)
                .ToList();

            return View(reportes);
        }

        /// <summary>
        /// [2.2] Muestra el formulario de registro.
        /// </summary>
        public ActionResult Registrar()
        {
            CargarListas();

            return View(new ReporteIncidente
            {
                FechaIncidente = DateTime.Today
            });
        }

        /// <summary>
        /// [2.3] Carga el formulario de edición.
        /// </summary>
        public ActionResult Editar(int id)
        {
            var reporte = db.ReportesIncidente
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(r => r.IdReporteIncidente == id);

            if (reporte == null)
            {
                TempData["Error"] = "No se encontró el reporte con ID " + id + ".";
                return RedirectToAction("Index");
            }

            CargarListas();
            return View(reporte);
        }

        /// <summary>
        /// [2.4] Muestra el detalle del reporte.
        /// </summary>
        public ActionResult Detalle(int id)
        {
            var reporte = db.ReportesIncidente
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(r => r.IdReporteIncidente == id);

            if (reporte == null)
            {
                TempData["Error"] = "No se encontró el reporte con ID " + id + ".";
                return RedirectToAction("Index");
            }

            return View(reporte);
        }

        /// <summary>
        /// [2.5] Descarga archivos PDF.
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
                                    "' no se encontró en el servidor.";
                return RedirectToAction("Index");
            }

            return File(rutaFisica, "application/pdf", nombre ?? "archivo.pdf");
        }

        /// <summary>
        /// [2.6] Dashboard de indicadores con filtros por año y trimestre.
        /// </summary>
        /// <param name="anio">Año a filtrar (por defecto el año actual).</param>
        /// <param name="trimestre">Trimestre a filtrar (1–4). Nulo = todo el año.</param>
        public ActionResult Dashboard(int? anio, int? trimestre)
        {
            // --- Parámetros de filtro ---
            int anioSel = anio ?? DateTime.Now.Year;

            if (trimestre.HasValue && (trimestre.Value < 1 || trimestre.Value > 4))
                trimestre = null;

            ViewBag.AnioSeleccionado = anioSel;
            ViewBag.TrimestreActual = trimestre;
            ViewBag.AniosDisponibles = ObtenerAniosDisponibles();

            // --- Consulta base filtrada ---
            var consulta = db.ReportesIncidente
                .Include("Proceso")
                .Where(r => r.FechaIncidente.Year == anioSel);

            if (trimestre.HasValue)
            {
                int mesInicio = (trimestre.Value - 1) * 3 + 1;
                int mesFin = mesInicio + 2;
                consulta = consulta.Where(r =>
                    r.FechaIncidente.Month >= mesInicio &&
                    r.FechaIncidente.Month <= mesFin);
            }

            var lista = consulta.ToList();

            // --- Indicadores generales ---
            ViewBag.TotalIncidentes = lista.Count;
            ViewBag.PromedioInvestigacion = lista
                .Where(r => r.DiasInvestigacion.HasValue)
                .Select(r => r.DiasInvestigacion.Value)
                .DefaultIfEmpty(0)
                .Average();

            // --- Distribución mensual (para gráfica de barras) ---
            ViewBag.DistribucionMensual = Enumerable.Range(1, 12)
                .Select(m => new EstadisticaItemIncidente
                {
                    Nombre = System.Globalization.CultureInfo
                                   .CurrentCulture.DateTimeFormat
                                   .GetAbbreviatedMonthName(m),
                    Cantidad = lista.Count(r => r.FechaIncidente.Month == m)
                })
                .ToList();

            // --- Top procesos ---
            ViewBag.TopProcesos = lista
                .GroupBy(r => r.Proceso != null ? r.Proceso.NombreProceso : "Sin proceso")
                .Select(g => new EstadisticaItemIncidente
                {
                    Nombre = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // --- Estadísticas por tipo de peligro ---
            ViewBag.EstadisticasPeligros = lista
                .Where(r => !string.IsNullOrWhiteSpace(r.TipoPeligro))
                .GroupBy(r => r.TipoPeligro)
                .Select(g => new EstadisticaItemIncidente
                {
                    Nombre = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // --- Estadísticas por causa ---
            ViewBag.EstadisticasCausas = lista
                .GroupBy(r => !string.IsNullOrWhiteSpace(r.Causa) ? r.Causa : "No definida")
                .Select(g => new EstadisticaItemIncidente
                {
                    Nombre = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // --- Distribución por género ---
            ViewBag.DistribucionGenero = lista
                .Where(r => !string.IsNullOrWhiteSpace(r.Genero))
                .GroupBy(r => r.Genero)
                .Select(g => new EstadisticaItemIncidente
                {
                    Nombre = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            // --- Distribución por turno ---
            ViewBag.DistribucionTurnos = lista
                .Where(r => !string.IsNullOrWhiteSpace(r.Turno))
                .GroupBy(r => r.Turno)
                .Select(g => new EstadisticaItemIncidente
                {
                    Nombre = g.Key,
                    Cantidad = g.Count()
                })
                .ToList();

            return View();
        }

        #endregion

        #region [3] Métodos POST

        /// <summary>
        /// [3.1] Registra un nuevo incidente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(
            ReporteIncidente modelo,
            HttpPostedFileBase archivoIncidente,
            HttpPostedFileBase archivoInvestigacion,
            HttpPostedFileBase archivoPlan)
        {
            if (modelo.FechaInvestigacion.HasValue)
                modelo.DiasInvestigacion =
                    (int)(modelo.FechaInvestigacion.Value - modelo.FechaIncidente).TotalDays;

            if (!ModelState.IsValid)
            {
                CargarListas();
                return View(modelo);
            }

            string carpeta = Server.MapPath(CarpetaVirtual);

            try { EnsureDirectory(carpeta); }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CargarListas();
                return View(modelo);
            }

            string rI, nI, eI;
            if (!GuardarPdf(archivoIncidente, carpeta, SubCarpeta, out rI, out nI, out eI))
            {
                ModelState.AddModelError("", eI);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoIncidente = rI;
            modelo.NombreArchivoIncidente = nI;

            string rInv, nInv, eInv;
            if (!GuardarPdf(archivoInvestigacion, carpeta, SubCarpeta, out rInv, out nInv, out eInv))
            {
                ModelState.AddModelError("", eInv);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoInvestigacion = rInv;
            modelo.NombreArchivoInvestigacion = nInv;

            string rP, nP, eP;
            if (!GuardarPdf(archivoPlan, carpeta, SubCarpeta, out rP, out nP, out eP))
            {
                ModelState.AddModelError("", eP);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoPlan = rP;
            modelo.NombreArchivoPlan = nP;

            try
            {
                modelo.FechaCreacion = DateTime.Now;
                db.ReportesIncidente.Add(modelo);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
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
            ReporteIncidente modelo,
            HttpPostedFileBase archivoIncidente,
            HttpPostedFileBase archivoInvestigacion,
            HttpPostedFileBase archivoPlan)
        {
            if (modelo.FechaInvestigacion.HasValue)
                modelo.DiasInvestigacion =
                    (int)(modelo.FechaInvestigacion.Value - modelo.FechaIncidente).TotalDays;

            if (!ModelState.IsValid)
            {
                CargarListas();
                return View(modelo);
            }

            var original = db.ReportesIncidente
                .AsNoTracking()
                .FirstOrDefault(r => r.IdReporteIncidente == modelo.IdReporteIncidente);

            if (original == null)
            {
                TempData["Error"] = "No se encontró el reporte.";
                return RedirectToAction("Index");
            }

            modelo.FechaCreacion = original.FechaCreacion;
            modelo.FechaModificacion = DateTime.Now;

            string carpeta = Server.MapPath(CarpetaVirtual);

            try { EnsureDirectory(carpeta); }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CargarListas();
                return View(modelo);
            }

            string rI, nI;
            ActualizarPdf(archivoIncidente, carpeta, SubCarpeta,
                original.RutaArchivoIncidente, original.NombreArchivoIncidente,
                out rI, out nI);
            modelo.RutaArchivoIncidente = rI;
            modelo.NombreArchivoIncidente = nI;

            string rInv, nInv;
            ActualizarPdf(archivoInvestigacion, carpeta, SubCarpeta,
                original.RutaArchivoInvestigacion, original.NombreArchivoInvestigacion,
                out rInv, out nInv);
            modelo.RutaArchivoInvestigacion = rInv;
            modelo.NombreArchivoInvestigacion = nInv;

            string rP, nP;
            ActualizarPdf(archivoPlan, carpeta, SubCarpeta,
                original.RutaArchivoPlan, original.NombreArchivoPlan,
                out rP, out nP);
            modelo.RutaArchivoPlan = rP;
            modelo.NombreArchivoPlan = nP;

            try
            {
                db.Entry(modelo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CargarListas();
                return View(modelo);
            }

            TempData["Exito"] = "Reporte actualizado correctamente.";
            return RedirectToAction("Index");
        }

        #endregion

        #region [4] Métodos Privados

        /// <summary>[4.1] Carga listas para formularios.</summary>
        private void CargarListas()
        {
            ViewBag.Generos = new[] { "Masculino", "Femenino", "Otro" };
            ViewBag.TiposPeligro = new[]
            {
                "Mecánico","Biológico","Biomecánico","Locativo",
                "Químico","Físico","Psicosocial","Eléctrico",
                "Público","Tecnológico","Otro"
            };
            ViewBag.Turnos = new[] { "Mañana", "Tarde", "Noche", "Día", "Adm. (M/T)" };
            ViewBag.CausasSubcausas = CausasHelper.ObtenerCausasSubcausas();
        }

        /// <summary>[4.2] Crea carpeta si no existe.</summary>
        private static void EnsureDirectory(string ruta)
        {
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);
        }

        /// <summary>[4.3] Guarda archivo PDF.</summary>
        private static bool GuardarPdf(
            HttpPostedFileBase archivo, string carpeta, string sub,
            out string ruta, out string nombre, out string error)
        {
            ruta = nombre = error = null;

            if (archivo == null || archivo.ContentLength == 0)
                return true;

            if (Path.GetExtension(archivo.FileName).ToLower() != ".pdf")
            {
                error = "Solo se permiten archivos PDF.";
                return false;
            }

            string n = Guid.NewGuid() + "_" + Path.GetFileName(archivo.FileName);
            archivo.SaveAs(Path.Combine(carpeta, n));

            nombre = archivo.FileName;
            ruta = "/Uploads/" + sub + "/" + n;

            return true;
        }

        /// <summary>[4.4] Actualiza archivo PDF.</summary>
        private static void ActualizarPdf(
            HttpPostedFileBase archivo, string carpeta, string sub,
            string rutaAnterior, string nombreAnterior,
            out string nuevaRuta, out string nuevoNombre)
        {
            if (archivo != null &&
                archivo.ContentLength > 0 &&
                Path.GetExtension(archivo.FileName).ToLower() == ".pdf")
            {
                string n = Guid.NewGuid() + "_" + Path.GetFileName(archivo.FileName);
                archivo.SaveAs(Path.Combine(carpeta, n));

                nuevaRuta = "/Uploads/" + sub + "/" + n;
                nuevoNombre = archivo.FileName;
            }
            else
            {
                nuevaRuta = rutaAnterior;
                nuevoNombre = nombreAnterior;
            }
        }

        /// <summary>[4.5] Obtiene los años con registros para el selector del Dashboard.</summary>
        private List<int> ObtenerAniosDisponibles()
        {
            var aniosEnBD = db.ReportesIncidente
                .Select(r => r.FechaIncidente.Year)
                .Distinct()
                .OrderByDescending(a => a)
                .ToList();

            // Garantiza que el año actual siempre aparezca aunque no tenga registros
            int anioActual = DateTime.Now.Year;
            if (!aniosEnBD.Contains(anioActual))
                aniosEnBD.Insert(0, anioActual);

            return aniosEnBD;
        }

        #endregion

        #region [5] Clases de Soporte

        /// <summary>
        /// Representa un ítem estadístico con nombre y cantidad
        /// para las gráficas del Dashboard.
        /// </summary>
        public class EstadisticaItemIncidente
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
        }

        #endregion

        #region [6] Liberación de Recursos

        /// <summary>
        /// [6.1] Libera los recursos del contexto.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}