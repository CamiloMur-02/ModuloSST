using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador Matriz de EPP
    /// Gestiona la matriz de entrega de Elementos de Protección Personal (EPP).
    /// </summary>
    /// <remarks>
    /// Diseño:
    ///  - Cada colaborador tiene una MatrizEpp con múltiples ElementoEpp.
    ///  - El tiempo de alerta es por elemento.
    ///  - TiempoRestante se recalcula en memoria (dato derivado).
    ///  - "Excluir" marca Activo=false (no elimina físicamente).
    /// </remarks>
    public class MatrizEppController : Controller
    {
        #region [1] Campos y Dependencias

        /// <summary>Contexto de base de datos.</summary>
        private readonly SSTContext db = new SSTContext();

        #endregion

        #region [2] Métodos GET

        /// <summary>
        /// [2.1] Lista todas las matrices con sus elementos y recalcula TiempoRestante en memoria.
        /// </summary>
        public ActionResult Index()
        {
            var matriz = db.MatrizEPP
                .Include("Elementos")
                .Include("Elementos.CatalogoEpp")
                .Include("Proceso")
                .Include("Subproceso")
                .OrderByDescending(m => m.FechaRegistro)
                .ToList();

            // Recalcular TiempoRestante en memoria
            foreach (var registro in matriz)
                foreach (var elem in registro.Elementos.Where(e => e.Activo && e.FechaEntrega.HasValue))
                {
                    double dias = (DateTime.Today - elem.FechaEntrega.Value).TotalDays;
                    elem.TiempoRestante = (decimal)(elem.TiempoAlerta - (dias / 30));
                }

            return View(matriz);
        }

        /// <summary>
        /// [2.2] Muestra el formulario de registro de una nueva matriz.
        /// </summary>
        public ActionResult Registrar()
        {
            CargarListas();
            return View(new MatrizEpp { FechaRegistro = DateTime.Today });
        }

        /// <summary>
        /// [2.3] Muestra el formulario de edición de una matriz existente.
        /// </summary>
        public ActionResult Editar(int id)
        {
            var registro = db.MatrizEPP
                .Include("Elementos")
                .Include("Elementos.CatalogoEpp")
                .Include("Proceso")
                .Include("Subproceso")
                .FirstOrDefault(m => m.IdMatrizEPP == id);

            if (registro == null)
            {
                TempData["Error"] = "No se encontró la matriz EPP con ID " + id + ".";
                return RedirectToAction("Index");
            }

            foreach (var elem in registro.Elementos.Where(e => e.Activo && e.FechaEntrega.HasValue))
            {
                double dias = (DateTime.Today - elem.FechaEntrega.Value).TotalDays;
                elem.TiempoRestante = (decimal)(elem.TiempoAlerta - (dias / 30));
            }

            CargarListas();
            return View(registro);
        }

        #endregion

        #region [3] Métodos POST

        /// <summary>
        /// [3.1] Registra una nueva matriz de entrega de EPP.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(
            MatrizEpp modelo,
            int[] idsEppSeleccionados,
            FormCollection form)
        {
            if (idsEppSeleccionados == null || idsEppSeleccionados.Length == 0)
            {
                ModelState.AddModelError("",
                    "Debe seleccionar al menos un elemento EPP para registrar la entrega.");
                CargarListas();
                return View(modelo);
            }

            if (!ModelState.IsValid)
            {
                CargarListas();
                return View(modelo);
            }

            modelo.Elementos = new List<ElementoEpp>();

            foreach (int idEpp in idsEppSeleccionados)
            {
                string fechaStr = form["fecha_" + idEpp];
                string alertaStr = form["alerta_" + idEpp];

                DateTime? fechaEntrega = null;
                if (!string.IsNullOrWhiteSpace(fechaStr) &&
                    DateTime.TryParse(fechaStr, out DateTime fechaParsed))
                    fechaEntrega = fechaParsed;

                if (!fechaEntrega.HasValue)
                {
                    ModelState.AddModelError("",
                        "Debe ingresar la fecha de entrega para todos los EPP seleccionados.");
                    CargarListas();
                    return View(modelo);
                }

                int alerta = 12;
                if (!string.IsNullOrWhiteSpace(alertaStr))
                    int.TryParse(alertaStr, out alerta);

                double dias = (DateTime.Today - fechaEntrega.Value).TotalDays;
                decimal tiempoRestante = (decimal)(alerta - (dias / 30));

                modelo.Elementos.Add(new ElementoEpp
                {
                    IdCatalogoEPP = idEpp,
                    FechaEntrega = fechaEntrega,
                    TiempoAlerta = alerta,
                    TiempoRestante = tiempoRestante,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                });
            }

            try
            {
                modelo.FechaCreacion = DateTime.Now;
                db.MatrizEPP.Add(modelo);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",
                    "Ocurrió un error al guardar la matriz en la base de datos. " +
                    "Detalle técnico: " + ex.Message);

                CargarListas();
                return View(modelo);
            }

            TempData["Exito"] = "Entrega de EPP registrada correctamente para " +
                                 modelo.NombreCompleto + ".";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// [3.2] Marca un elemento como inactivo (no lo elimina físicamente).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExcluirElemento(int idElemento, int idMatriz)
        {
            var elem = db.ElementosEPP.Find(idElemento);

            if (elem == null)
            {
                TempData["Error"] = "El elemento EPP con ID " + idElemento +
                                    " no fue encontrado.";
                return RedirectToAction("Editar", new { id = idMatriz });
            }

            try
            {
                elem.Activo = false;
                elem.FechaModificacion = DateTime.Now;

                db.Entry(elem).Property(e => e.Activo).IsModified = true;
                db.Entry(elem).Property(e => e.FechaModificacion).IsModified = true;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo excluir el elemento. " +
                                    "Detalle técnico: " + ex.Message;
                return RedirectToAction("Editar", new { id = idMatriz });
            }

            TempData["Exito"] = "Elemento de EPP excluido correctamente.";
            return RedirectToAction("Editar", new { id = idMatriz });
        }

        /// <summary>
        /// [3.3] Agrega un nuevo elemento EPP a una matriz existente.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarElemento(
            int idMatriz,
            int idCatalogo,
            DateTime? fechaEntrega,
            int tiempoAlerta = 12)
        {
            if (!fechaEntrega.HasValue)
            {
                TempData["Error"] = "La fecha de entrega es obligatoria para agregar un elemento EPP.";
                return RedirectToAction("Editar", new { id = idMatriz });
            }

            double dias = (DateTime.Today - fechaEntrega.Value).TotalDays;
            decimal tiempoRestante = (decimal)(tiempoAlerta - (dias / 30));

            try
            {
                db.ElementosEPP.Add(new ElementoEpp
                {
                    IdMatrizEPP = idMatriz,
                    IdCatalogoEPP = idCatalogo,
                    FechaEntrega = fechaEntrega,
                    TiempoAlerta = tiempoAlerta,
                    TiempoRestante = tiempoRestante,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                });
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo agregar el elemento EPP. " +
                                    "Verifique que el tipo de EPP seleccionado sea válido. " +
                                    "Detalle técnico: " + ex.Message;
                return RedirectToAction("Editar", new { id = idMatriz });
            }

            TempData["Exito"] = "Elemento de EPP agregado correctamente.";
            return RedirectToAction("Editar", new { id = idMatriz });
        }

        /// <summary>
        /// [3.4] Guarda las observaciones finales de una matriz.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarObservaciones(int idMatriz, string observacionesFinales)
        {
            var registro = db.MatrizEPP
                .FirstOrDefault(m => m.IdMatrizEPP == idMatriz);

            if (registro == null)
            {
                TempData["Error"] = "No se encontró la entrega de EPP con ID " + idMatriz + ".";
                return RedirectToAction("Index");
            }

            try
            {
                registro.ObservacionesFinales = observacionesFinales;
                registro.FechaModificacion = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudieron guardar las observaciones. " +
                                    "Detalle técnico: " + ex.Message;
                return RedirectToAction("Editar", new { id = idMatriz });
            }

            TempData["Exito"] = "Observaciones guardadas correctamente.";
            return RedirectToAction("Editar", new { id = idMatriz });
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
            ViewBag.CatalogoEPP = db.CatalogoEPP
                .Where(c => c.Estado)
                .OrderBy(c => c.NombreEPP)
                .ToList();
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