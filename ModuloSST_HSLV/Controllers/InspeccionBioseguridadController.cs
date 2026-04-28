using System;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador Inspección de Bioseguridad
    /// Gestiona el registro y consulta de inspecciones de bioseguridad.
    /// </summary>
    /// <remarks>
    /// Funcionalidades:
    ///  - Registro de inspecciones
    ///  - Consulta de listado
    ///  - Visualización de detalle
    ///
    /// Regla de negocio:
    ///  Los totales de Cumple (C), No Cumple (NC) y No Aplica (NA)
    ///  se calculan automáticamente al guardar.
    /// </remarks>
    public class InspeccionBioseguridadController : Controller
    {
        #region [1] Campos y Dependencias

        /// <summary>Contexto de base de datos.</summary>
        private readonly SSTContext db = new SSTContext();

        #endregion

        #region [2] Métodos GET

        /// <summary>
        /// [2.1] Lista todas las inspecciones ordenadas por fecha descendente.
        /// </summary>
        public ActionResult Index()
        {
            var inspecciones = db.InspeccionesBioseguridad
                .Include("Proceso")
                .Include("Subproceso")
                .OrderByDescending(i => i.FechaInspeccion)
                .ToList();

            return View(inspecciones);
        }

        /// <summary>
        /// [2.2] Muestra el formulario para registrar una inspección.
        /// </summary>
        public ActionResult Registrar()
        {
            CargarListas();

            return View(new InspeccionBioseguridad
            {
                FechaInspeccion = DateTime.Today
            });
        }

        /// <summary>
        /// [2.3] Muestra el detalle de una inspección.
        /// </summary>
        public ActionResult Detalle(int id)
        {
            var inspeccion = db.InspeccionesBioseguridad
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(i => i.IdInspeccionBioseguridad == id);

            if (inspeccion == null)
            {
                TempData["Error"] = "No se encontró la inspección con ID " + id + ".";
                return RedirectToAction("Index");
            }

            return View(inspeccion);
        }

        #endregion

        #region [3] Métodos POST

        /// <summary>
        /// [3.1] Registra una nueva inspección de bioseguridad.
        /// </summary>
        /// <param name="modelo">Modelo de la inspección.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(InspeccionBioseguridad modelo)
        {
            if (!ModelState.IsValid)
            {
                CargarListas();
                return View(modelo);
            }

            // [3.1.1] Calcular totales automáticamente (C / NC / NA)
            var items = new[]
            {
                modelo.HigieneManos,        modelo.ProteccionRespiratoria,
                modelo.UsoGuantes,          modelo.UsoGorro,
                modelo.UsoBata,             modelo.ProteccionVisual,
                modelo.UsoDelantalPeto,     modelo.CalzadoAdecuado,
                modelo.TransporteMuestras,  modelo.ManejoCorto,
                modelo.ManejoResiduos
            };

            modelo.TotalCumple = items.Count(v => v == "C");
            modelo.TotalNoCumple = items.Count(v => v == "NC");
            modelo.TotalNoAplica = items.Count(v => v == "NA");
            modelo.FechaCreacion = DateTime.Now;

            // [3.1.2] Guardar en base de datos
            try
            {
                db.InspeccionesBioseguridad.Add(modelo);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Ocurrió un error al guardar la inspección en la base de datos. " +
                    "Verifique que el proceso y subproceso seleccionados sean válidos. " +
                    "Detalle técnico: " + ex.Message);

                CargarListas();
                return View(modelo);
            }

            TempData["Exito"] = "Inspección de bioseguridad registrada correctamente para " +
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