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
    /// Controlador de Evaluaciones
    /// Gestiona la carga, almacenamiento y consulta de evaluaciones en formato PDF.
    /// </summary>
    /// <remarks>
    /// Tipos válidos definidos a nivel de aplicación:
    ///  - Inspeccion de areas y/o puesto de trabajo
    ///  - Evaluacion de Riesgo Psicosocial
    ///  - Analisis de puesto de trabajo
    ///  - Registro de inspeccion de bioseguridad
    ///
    /// Ubicación de archivos:
    ///  ~/Uploads/Evaluaciones/
    /// </remarks>
    public class EvaluacionController : Controller
    {
        #region [1] Campos y Constantes

        /// <summary>Contexto de base de datos.</summary>
        private readonly SSTContext db = new SSTContext();

        /// <summary>Ruta virtual donde se almacenan los archivos.</summary>
        private const string CARPETA_VIRTUAL = "~/Uploads/Evaluaciones/";

        /// <summary>Tipos de evaluación permitidos.</summary>
        private static readonly string[] TIPOS_EVALUACION_VALIDOS = new[]
        {
            "Inspeccion de areas y/o puesto de trabajo",
            "Evaluacion de Riesgo Psicosocial",
            "Analisis de puesto de trabajo",
            "Registro de inspeccion de bioseguridad"
        };

        #endregion

        #region [2] Métodos GET

        /// <summary>
        /// [2.1] Lista todas las evaluaciones ordenadas por fecha de carga.
        /// </summary>
        public ActionResult Index()
        {
            var evaluaciones = db.Evaluaciones
                                 .OrderByDescending(e => e.FechaCarga)
                                 .ToList();

            return View(evaluaciones);
        }

        /// <summary>
        /// [2.2] Muestra el formulario de creación de evaluaciones.
        /// </summary>
        public ActionResult Crear()
        {
            CargarTiposEvaluacion();
            return View();
        }

        #endregion

        #region [3] Métodos POST

        /// <summary>
        /// [3.1] Procesa la carga de una nueva evaluación en PDF.
        /// </summary>
        /// <param name="tipoEvaluacion">Tipo de evaluación seleccionado.</param>
        /// <param name="archivo">Archivo PDF adjunto.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(string tipoEvaluacion, HttpPostedFileBase archivo)
        {
            CargarTiposEvaluacion();

            // [3.1.1] Validaciones
            string error = ValidarEntrada(tipoEvaluacion, archivo);
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Error = error;
                return View();
            }

            // [3.1.2] Guardar archivo
            string nombreUnico;
            string errorArchivo = GuardarArchivo(archivo, out nombreUnico);

            if (!string.IsNullOrEmpty(errorArchivo))
            {
                ViewBag.Error = errorArchivo;
                return View();
            }

            // [3.1.3] Guardar en base de datos
            string errorBD = GuardarEnBaseDatos(tipoEvaluacion, archivo.FileName, nombreUnico);

            if (!string.IsNullOrEmpty(errorBD))
            {
                ViewBag.Error = errorBD;
                return View();
            }

            TempData["Exito"] = $"Evaluación '{tipoEvaluacion}' cargada correctamente.";
            return RedirectToAction("Index");
        }

        #endregion

        #region [4] Visualización de Archivos

        /// <summary>
        /// [4.1] Permite visualizar un archivo PDF en el navegador.
        /// </summary>
        /// <param name="id">ID de la evaluación.</param>
        public ActionResult VerArchivo(int id)
        {
            var evaluacion = db.Evaluaciones.Find(id);

            if (evaluacion == null)
            {
                TempData["Error"] = $"No se encontró la evaluación con ID {id}.";
                return RedirectToAction("Index");
            }

            string rutaFisica = Server.MapPath("~" + evaluacion.RutaArchivo);

            if (!System.IO.File.Exists(rutaFisica))
            {
                TempData["Error"] = $"El archivo '{evaluacion.NombreArchivo}' no existe en el servidor.";
                return RedirectToAction("Index");
            }

            return File(rutaFisica, "application/pdf", evaluacion.NombreArchivo);
        }

        #endregion

        #region [5] Métodos Privados (Lógica Interna)

        /// <summary>
        /// [5.1] Carga los tipos de evaluación en el ViewBag.
        /// </summary>
        private void CargarTiposEvaluacion()
        {
            ViewBag.TiposEvaluacion = TIPOS_EVALUACION_VALIDOS;
        }

        /// <summary>
        /// [5.2] Valida los datos de entrada del formulario.
        /// </summary>
        private string ValidarEntrada(string tipoEvaluacion, HttpPostedFileBase archivo)
        {
            if (string.IsNullOrWhiteSpace(tipoEvaluacion))
                return "Debe seleccionar el tipo de evaluación.";

            if (!TIPOS_EVALUACION_VALIDOS.Contains(tipoEvaluacion))
                return "El tipo de evaluación seleccionado no es válido.";

            if (archivo == null || archivo.ContentLength == 0)
                return "Debe adjuntar un archivo PDF.";

            if (Path.GetExtension(archivo.FileName).ToLower() != ".pdf")
                return $"El archivo '{archivo.FileName}' no es un PDF válido.";

            return null;
        }

        /// <summary>
        /// [5.3] Guarda el archivo en el servidor.
        /// </summary>
        private string GuardarArchivo(HttpPostedFileBase archivo, out string nombreUnico)
        {
            nombreUnico = null;

            string carpeta = Server.MapPath(CARPETA_VIRTUAL);

            try
            {
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                nombreUnico = $"{Guid.NewGuid()}_{Path.GetFileName(archivo.FileName)}";
                archivo.SaveAs(Path.Combine(carpeta, nombreUnico));
            }
            catch (Exception ex)
            {
                return "Error al guardar el archivo: " + ex.Message;
            }

            return null;
        }

        /// <summary>
        /// [5.4] Guarda el registro en la base de datos.
        /// </summary>
        private string GuardarEnBaseDatos(string tipoEvaluacion, string nombreOriginal, string nombreUnico)
        {
            try
            {
                db.Evaluaciones.Add(new Evaluacion
                {
                    TipoEvaluacion = tipoEvaluacion,
                    NombreArchivo = nombreOriginal,
                    RutaArchivo = "/Uploads/Evaluaciones/" + nombreUnico,
                    FechaCarga = DateTime.Now,
                    FechaCreacion = DateTime.Now
                });

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return "Error al guardar en base de datos: " + ex.Message;
            }

            return null;
        }

        #endregion

        #region [6] Liberación de Recursos

        /// <summary>
        /// [6.1] Libera los recursos del contexto de base de datos.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}