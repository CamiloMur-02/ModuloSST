using System;
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
    /// Controlador Reporte de Accidente de Trabajo
    /// Gestiona el registro, edición, consulta y descarga de reportes de accidentes laborales.
    /// </summary>
    /// <remarks>
    /// Funcionalidades:
    ///  - Registro de reportes
    ///  - Edición de reportes
    ///  - Consulta de detalle
    ///  - Descarga de archivos PDF adjuntos
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
        private const string SubCarpeta = "AccidentesTrabajo";

        /// <summary>Ruta virtual de almacenamiento.</summary>
        private const string CarpetaVirtual = "~/Uploads/AccidentesTrabajo/";

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
        /// [2.2] Muestra el formulario para registrar un nuevo reporte.
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
        /// [2.3] Muestra el formulario de edición de un reporte.
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
        /// [2.4] Muestra el detalle completo de un reporte.
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
        /// [2.5] Descarga un archivo PDF asociado al reporte.
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

            TempData["Exito"] = "Reporte de accidente N° " + modelo.NumeroReporte +
                                 " registrado correctamente.";

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

            TempData["Exito"] = "Reporte N° " + modelo.NumeroReporte + " actualizado correctamente.";
            return RedirectToAction("Index");
        }

        #endregion

        #region [4] Métodos Privados

        /// <summary>
        /// [4.1] Carga listas necesarias en ViewBag.
        /// </summary>
        private void CargarListas()
        {
            ViewBag.Procesos = db.Procesos.OrderBy(p => p.NombreProceso).ToList();
            ViewBag.Subprocesos = db.Subprocesos.OrderBy(s => s.NombreSubproceso).ToList();
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