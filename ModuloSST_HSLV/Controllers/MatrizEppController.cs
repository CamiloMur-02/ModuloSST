using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Modelo auxiliar para estadísticas del Dashboard de EPP.
    /// </summary>
    public class EstadisticaEpp
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
    }

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
        /// [2.1] Dashboard con estadísticas de entregas de EPP.
        /// Permite filtrar por año y muestra distribución mensual,
        /// trimestral y top 10 de elementos más entregados.
        /// </summary>
        public ActionResult Dashboard(int? anio)
        {
            int filtroAnio = anio ?? DateTime.Today.Year;

            var entregas = db.ElementosEPP
                .Include("CatalogoEpp")
                .Where(e => e.Activo &&
                            e.FechaEntrega.HasValue &&
                            e.FechaEntrega.Value.Year == filtroAnio)
                .ToList();

            // ── Distribución mensual (array de 12 posiciones) ─────────────
            int[] datosMensuales = new int[12];
            foreach (var g in entregas.GroupBy(e => e.FechaEntrega.Value.Month))
                datosMensuales[g.Key - 1] = g.Count();

            ViewBag.DatosMensuales = datosMensuales;

            // ── Distribución trimestral ───────────────────────────────────
            ViewBag.DatosTrimestrales = new int[4]
            {
                datosMensuales[0] + datosMensuales[1] + datosMensuales[2],
                datosMensuales[3] + datosMensuales[4] + datosMensuales[5],
                datosMensuales[6] + datosMensuales[7] + datosMensuales[8],
                datosMensuales[9] + datosMensuales[10] + datosMensuales[11]
            };

            // ── Top 10 elementos más entregados ───────────────────────────
            ViewBag.TopElementos = entregas
                .Where(e => e.CatalogoEpp != null)
                .GroupBy(e => e.CatalogoEpp.NombreEPP)
                .Select(g => new EstadisticaEpp { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
                .ToList();

            // ── Tarjeta resumen ───────────────────────────────────────────
            ViewBag.TotalEntregas = entregas.Count;
            ViewBag.AnioActual = filtroAnio;

            return View();
        }

        /// <summary>
        /// [2.2] Lista todas las matrices con sus elementos y recalcula TiempoRestante en memoria.
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

            foreach (var registro in matriz)
                foreach (var elem in registro.Elementos.Where(e => e.Activo && e.FechaEntrega.HasValue))
                    elem.TiempoRestante = CalcularTiempoRestante(elem.FechaEntrega.Value, elem.TiempoAlerta);

            return View(matriz);
        }

        /// <summary>
        /// [2.3] Muestra el formulario de registro de una nueva matriz.
        /// </summary>
        public ActionResult Registrar()
        {
            CargarListas();
            return View(new MatrizEpp { FechaRegistro = DateTime.Today });
        }

        /// <summary>
        /// [2.4] Muestra el formulario de edición de una matriz existente.
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
                elem.TiempoRestante = CalcularTiempoRestante(elem.FechaEntrega.Value, elem.TiempoAlerta);

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

                modelo.Elementos.Add(new ElementoEpp
                {
                    IdCatalogoEPP = idEpp,
                    FechaEntrega = fechaEntrega,
                    TiempoAlerta = alerta,
                    TiempoRestante = CalcularTiempoRestante(fechaEntrega.Value, alerta),
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

            TempData["Exito"] = "Entrega de EPP registrada correctamente";

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

            try
            {
                db.ElementosEPP.Add(new ElementoEpp
                {
                    IdMatrizEPP = idMatriz,
                    IdCatalogoEPP = idCatalogo,
                    FechaEntrega = fechaEntrega,
                    TiempoAlerta = tiempoAlerta,
                    TiempoRestante = CalcularTiempoRestante(fechaEntrega.Value, tiempoAlerta),
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
            ViewBag.Generos = new[] { "Masculino", "Femenino", "Otro" };
            ViewBag.CatalogoEPP = db.CatalogoEPP
                .Where(c => c.Estado)
                .OrderBy(c => c.NombreEPP)
                .ToList();
        }

        /// <summary>
        /// [4.2] Calcula el tiempo restante en meses para un elemento EPP.
        /// Retorna 0 si el elemento ya está vencido (evita valores negativos).
        /// Usa 30.44 días promedio por mes para mayor precisión.
        /// </summary>
        /// <param name="fechaEntrega">Fecha en que se entregó el EPP.</param>
        /// <param name="tiempoAlerta">Vida útil estimada en meses.</param>
        private static decimal CalcularTiempoRestante(DateTime fechaEntrega, int tiempoAlerta)
        {
            DateTime fechaVencimiento = fechaEntrega.AddMonths(tiempoAlerta);

            if (DateTime.Today >= fechaVencimiento)
                return 0;

            TimeSpan diferencia = fechaVencimiento - DateTime.Today;
            return (decimal)(diferencia.TotalDays / 30.44);
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