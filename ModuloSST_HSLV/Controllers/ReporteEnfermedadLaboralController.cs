using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador Reporte Enfermedad Laboral
    /// Gestiona reportes de enfermedad laboral, enfermedad general y accidente de trabajo (diagnóstico).
    /// </summary>
    /// <remarks>
    /// Regla clave:
    /// Solo cuando TipoEnfermedad = "Enfermedad Laboral" se activan:
    ///  - Investigación
    ///  - Plan de mejora
    ///  - Archivos PDF (seguimiento y FUREL)
    /// </remarks>
    public class ReporteEnfermedadLaboralController : Controller
    {
        #region [1] Campos y Constantes

        /// <summary>Contexto de base de datos.</summary>
        private readonly SSTContext db = new SSTContext();

        /// <summary>Nombre de subcarpeta física.</summary>
        private const string SubCarpeta = "EnfermedadLaboral";

        /// <summary>Ruta virtual donde se almacenan los archivos.</summary>
        private const string CarpetaVirtual = "~/Uploads/EnfermedadLaboral/";

        #endregion

        #region [2] Métodos GET

        /// <summary>
        /// [2.1] Lista todos los reportes ordenados por fecha de diagnóstico descendente.
        /// </summary>
        public ActionResult Index()
        {
            var reportes = db.ReportesEnfermedadLaboral
                .Include("Proceso")
                .Include("Subproceso")
                .OrderByDescending(r => r.FechaDiagnostico)
                .ToList();

            return View(reportes);
        }

        /// <summary>
        /// [2.2] Muestra el formulario de registro.
        /// </summary>
        public ActionResult Registrar()
        {
            CargarListas();

            return View(new ReporteEnfermedadLaboral
            {
                FechaDiagnostico = DateTime.Today,
                DiasIncapacidad = 0
            });
        }

        /// <summary>
        /// [2.3] Carga el formulario de edición.
        /// </summary>
        public ActionResult Editar(int id)
        {
            var reporte = db.ReportesEnfermedadLaboral
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(r => r.IdReporteEnfermedadLaboral == id);

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
            var reporte = db.ReportesEnfermedadLaboral
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(r => r.IdReporteEnfermedadLaboral == id);

            if (reporte == null)
            {
                TempData["Error"] = "No se encontró el reporte con ID " + id + ".";
                return RedirectToAction("Index");
            }

            return View(reporte);
        }

        /// <summary>
        /// [2.5] Descarga un archivo PDF.
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
        /// [3.1] Registra un nuevo reporte.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(
            ReporteEnfermedadLaboral modelo,
            HttpPostedFileBase archivoPlan,
            HttpPostedFileBase archivoSeguimiento,
            HttpPostedFileBase archivoFurel)
        {
            LimpiarCamposNoLaborales(modelo);

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

            string ruta, nombre, error;

            if (!GuardarPdf(archivoPlan, carpeta, SubCarpeta, out ruta, out nombre, out error))
            {
                ModelState.AddModelError("", error);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoPlan = ruta;
            modelo.NombreArchivoPlan = nombre;

            if (!GuardarPdf(archivoSeguimiento, carpeta, SubCarpeta, out ruta, out nombre, out error))
            {
                ModelState.AddModelError("", error);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoSeguimiento = ruta;
            modelo.NombreArchivoSeguimiento = nombre;

            if (!GuardarPdf(archivoFurel, carpeta, SubCarpeta, out ruta, out nombre, out error))
            {
                ModelState.AddModelError("", error);
                CargarListas();
                return View(modelo);
            }
            modelo.RutaArchivoFurel = ruta;
            modelo.NombreArchivoFurel = nombre;

            try
            {
                modelo.FechaCreacion = DateTime.Now;
                db.ReportesEnfermedadLaboral.Add(modelo);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Ocurrió un error al guardar el reporte en la base de datos. " +
                    "Verifique los datos ingresados. " +
                    "Detalle técnico: " + ex.Message);
                CargarListas();
                return View(modelo);
            }

            TempData["Exito"] = "Reporte de enfermedad registrado correctamente para " +
                                 modelo.NombreCompleto + ".";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// [3.2] Actualiza un reporte existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(
            ReporteEnfermedadLaboral modelo,
            HttpPostedFileBase archivoPlan,
            HttpPostedFileBase archivoSeguimiento,
            HttpPostedFileBase archivoFurel)
        {
            LimpiarCamposNoLaborales(modelo);

            if (!ModelState.IsValid)
            {
                CargarListas();
                return View(modelo);
            }

            var original = db.ReportesEnfermedadLaboral
                .AsNoTracking()
                .FirstOrDefault(r => r.IdReporteEnfermedadLaboral == modelo.IdReporteEnfermedadLaboral);

            if (original == null)
            {
                TempData["Error"] = "No se encontró el reporte que intenta editar.";
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

            string rutaP, nomP;
            ActualizarPdf(archivoPlan, carpeta, SubCarpeta,
                original.RutaArchivoPlan, original.NombreArchivoPlan,
                out rutaP, out nomP);

            modelo.RutaArchivoPlan = rutaP;
            modelo.NombreArchivoPlan = nomP;

            string rutaS, nomS;
            ActualizarPdf(archivoSeguimiento, carpeta, SubCarpeta,
                original.RutaArchivoSeguimiento, original.NombreArchivoSeguimiento,
                out rutaS, out nomS);

            modelo.RutaArchivoSeguimiento = rutaS;
            modelo.NombreArchivoSeguimiento = nomS;

            string rutaF, nomF;
            ActualizarPdf(archivoFurel, carpeta, SubCarpeta,
                original.RutaArchivoFurel, original.NombreArchivoFurel,
                out rutaF, out nomF);

            modelo.RutaArchivoFurel = rutaF;
            modelo.NombreArchivoFurel = nomF;

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

        /// <summary>[4.1] Limpia campos cuando no es enfermedad laboral.</summary>
        private static void LimpiarCamposNoLaborales(ReporteEnfermedadLaboral modelo)
        {
            if (modelo.TipoEnfermedad == "Enfermedad Laboral") return;

            modelo.FechaInvestigacion = null;
            modelo.DiasInvestigacion = null;
            modelo.Causa = null;
            modelo.ActividadARealizar = null;
            modelo.Responsable = null;
            modelo.FechaPlaneada = null;
            modelo.FechaEjecucion = null;
            modelo.Observaciones = null;
            modelo.RutaArchivoSeguimiento = null;
            modelo.NombreArchivoSeguimiento = null;
            modelo.RutaArchivoFurel = null;
            modelo.NombreArchivoFurel = null;
        }

        /// <summary>[4.2] Carga listas para formularios.</summary>
        private void CargarListas()
        {
            ViewBag.Procesos = db.Procesos.OrderBy(p => p.NombreProceso).ToList();
            ViewBag.Subprocesos = db.Subprocesos.OrderBy(s => s.NombreSubproceso).ToList();
            ViewBag.Generos = new[] { "Masculino", "Femenino", "Otro" };
            ViewBag.TiposEnfermedad = new[]
            {
                "Enfermedad Laboral",
                "Enfermedad General",
                "Accidente de Trabajo"
            };
        }

        /// <summary>[4.3] Crea carpeta si no existe.</summary>
        private static void EnsureDirectory(string ruta)
        {
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);
        }

        /// <summary>[4.4] Guarda archivo PDF.</summary>
        private static bool GuardarPdf(
            HttpPostedFileBase archivo, string carpeta, string sub,
            out string ruta, out string nombre, out string error)
        {
            ruta = nombre = error = null;

            if (archivo == null || archivo.ContentLength == 0)
                return true;

            if (Path.GetExtension(archivo.FileName).ToLower() != ".pdf")
            {
                error = "El archivo '" + archivo.FileName + "' no es un PDF válido.";
                return false;
            }

            string n = Guid.NewGuid() + "_" + Path.GetFileName(archivo.FileName);
            archivo.SaveAs(Path.Combine(carpeta, n));

            nombre = archivo.FileName;
            ruta = "/Uploads/" + sub + "/" + n;

            return true;
        }

        /// <summary>[4.5] Actualiza archivo PDF.</summary>
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

        #endregion

        #region [5] Liberación de Recursos

        /// <summary>
        /// [5.1] Libera los recursos del contexto.
        /// </summary>

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}