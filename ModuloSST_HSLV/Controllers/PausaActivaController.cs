using System;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador Pausa Activa
    /// Gestiona el registro y consulta de pausas activas realizadas a colaboradores.
    /// </summary>
    /// <remarks>
    /// Funcionalidades:
    ///  - Registro de pausas activas
    ///  - Consulta de listado
    ///  - Visualización de detalle
    /// </remarks>
    public class PausaActivaController : Controller
    {
        #region [1] Campos y Dependencias

        /// <summary>Contexto de base de datos.</summary>
        private readonly SSTContext db = new SSTContext();

        #endregion

        #region [2] Métodos GET

        /// <summary>
        /// [2.1] Lista todas las pausas activas ordenadas por fecha descendente.
        /// </summary>
        public ActionResult Index()
        {
            var pausas = db.PausasActivas
                .Include("Proceso")
                .Include("Subproceso")
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(pausas);
        }

        /// <summary>
        /// [2.2] Muestra el formulario para registrar una pausa activa.
        /// </summary>
        public ActionResult Registrar()
        {
            CargarListas();

            return View(new PausaActiva
            {
                Fecha = DateTime.Today
            });
        }

        /// <summary>
        /// [2.3] Muestra el detalle de una pausa activa.
        /// </summary>
        public ActionResult Detalle(int id)
        {
            var pausa = db.PausasActivas
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(p => p.IdPausaActiva == id);

            if (pausa == null)
            {
                TempData["Error"] = "No se encontró la pausa activa con ID " + id + ".";
                return RedirectToAction("Index");
            }

            return View(pausa);
        }

        #endregion

        #region [3] Métodos POST

        /// <summary>
        /// [3.1] Registra una nueva pausa activa.
        /// </summary>
        /// <param name="modelo">Modelo de la pausa activa.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(PausaActiva modelo)
        {
            if (!ModelState.IsValid)
            {
                CargarListas();
                return View(modelo);
            }

            try
            {
                modelo.FechaCreacion = DateTime.Now;
                db.PausasActivas.Add(modelo);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Ocurrió un error al guardar la pausa activa en la base de datos. " +
                    "Verifique que el proceso y subproceso sean válidos. " +
                    "Detalle técnico: " + ex.Message);

                CargarListas();
                return View(modelo);
            }

            TempData["Exito"] = "Pausa activa registrada correctamente para " +
                                 modelo.NombreCompleto + ".";

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