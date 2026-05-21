using System;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador Catálogo de EPP
    /// Gestiona el catálogo maestro de tipos de Elementos de Protección Personal (EPP).
    /// </summary>
    /// <remarks>
    /// Funcionalidades:
    ///  - Crear tipos de EPP
    ///  - Editar tipos de EPP
    ///  - Activar / Desactivar registros
    ///
    /// Regla de negocio:
    ///  Solo los EPP con Estado = true estarán disponibles
    ///  en el registro de nuevas matrices o entregas.
    /// </remarks>
    public class CatalogoEppController : Controller
    {
        #region [1] Campos y Dependencias

        /// <summary>Contexto de base de datos.</summary>
        private readonly SSTContext db = new SSTContext();

        #endregion

        #region [2] Métodos GET

        /// <summary>
        /// [2.1] Lista todos los tipos de EPP ordenados por nombre.
        /// </summary>
        public ActionResult Index()
        {
            return View(db.CatalogoEPP
                          .OrderBy(c => c.NombreEPP)
                          .ToList());
        }

        /// <summary>
        /// [2.2] Muestra el formulario de creación de EPP.
        /// </summary>
        public ActionResult Crear()
        {
            return View(new CatalogoEpp { Estado = true });
        }

        /// <summary>
        /// [2.3] Muestra el formulario de edición de un EPP.
        /// </summary>
        /// <param name="id">ID del EPP.</param>
        public ActionResult Editar(int id)
        {
            var item = db.CatalogoEPP.Find(id);

            if (item == null)
            {
                TempData["Error"] = "No se encontró el EPP con ID " + id + ".";
                return RedirectToAction("Index");
            }

            return View(item);
        }

        #endregion

        #region [3] Métodos POST

        /// <summary>
        /// [3.1] Crea un nuevo tipo de EPP.
        /// </summary>
        /// <param name="modelo">Modelo del EPP.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(CatalogoEpp modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            try
            {
                modelo.FechaCreacion = DateTime.Now;
                db.CatalogoEPP.Add(modelo);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Ocurrió un error al guardar el EPP en la base de datos. " +
                    "Verifique que el nombre no esté duplicado. " +
                    "Detalle técnico: " + ex.Message);

                return View(modelo);
            }

            TempData["Exito"] = "Tipo de EPP registrado correctamente.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// [3.2] Actualiza un tipo de EPP existente.
        /// </summary>
        /// <param name="modelo">Modelo actualizado del EPP.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(CatalogoEpp modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            try
            {
                modelo.FechaModificacion = DateTime.Now;
                db.Entry(modelo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Ocurrió un error al actualizar el EPP en la base de datos. " +
                    "Detalle técnico: " + ex.Message);

                return View(modelo);
            }

            TempData["Exito"] = "Tipo de EPP actualizado correctamente.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// [3.3] Alterna el estado activo/inactivo de un EPP.
        /// </summary>
        /// <param name="id">ID del EPP.</param>
        /// <remarks>
        /// Un EPP inactivo no estará disponible en nuevas entregas.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id)
        {
            var item = db.CatalogoEPP.Find(id);

            if (item == null)
            {
                TempData["Error"] = "No se encontró el EPP con ID " + id + ".";
                return RedirectToAction("Index");
            }

            try
            {
                item.Estado = !item.Estado;
                item.FechaModificacion = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo cambiar el estado del EPP. " +
                                    "Detalle técnico: " + ex.Message;

                return RedirectToAction("Index");
            }

            string nuevoEstado = item.Estado ? "Activo" : "Inactivo";

            TempData["Exito"] = "EPP '" + item.NombreEPP + "' marcado como " +
                                nuevoEstado + " correctamente.";

            return RedirectToAction("Index");
        }

        #endregion

        #region [4] Liberación de Recursos

        /// <summary>
        /// [4.1] Libera los recursos del contexto de base de datos.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}