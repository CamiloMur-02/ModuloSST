using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador Pausa Activa
    /// Gestiona el registro, consulta y estadísticas de pausas activas realizadas a colaboradores.
    /// </summary>
    /// <remarks>
    /// Funcionalidades:
    ///  - Registro de pausas activas
    ///  - Consulta de listado
    ///  - Visualización de detalle
    ///  - Dashboard de indicadores con filtros
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

        /// <summary>
        /// [2.4] Dashboard de indicadores de pausas activas.
        /// Permite filtrar por año, mes y trimestre.
        /// </summary>
        /// <remarks>
        /// Muestra:
        ///  - Total de pausas activas en el período
        ///  - Distribución mensual
        ///  - Agrupación por Proceso
        ///  - Agrupación por Subproceso
        ///  - Agrupación por Cargo
        /// </remarks>
        /// <param name="anio">Año a filtrar (por defecto el año actual).</param>
        /// <param name="mes">Mes a filtrar (1–12). Nulo = todos los meses.</param>
        /// <param name="trimestre">Trimestre a filtrar (1–4). Nulo = todo el año.</param>
        public ActionResult Dashboard(int? anio, int? mes, int? trimestre)
        {
            int filtroAnio = anio ?? DateTime.Today.Year;

            if (trimestre.HasValue && (trimestre.Value < 1 || trimestre.Value > 4))
                trimestre = null;

            if (mes.HasValue && (mes.Value < 1 || mes.Value > 12))
                mes = null;

            ViewBag.AnioActual = filtroAnio;
            ViewBag.MesActual = mes;
            ViewBag.TrimestreActual = trimestre;
            ViewBag.AniosDisponibles = ObtenerAniosDisponibles();

            // --- Consulta base filtrada por año ---
            var consulta = db.PausasActivas
                .Include("Proceso")
                .Include("Subproceso")
                .Where(p => p.Fecha.Year == filtroAnio);

            // --- Filtro por mes ---
            if (mes.HasValue)
                consulta = consulta.Where(p => p.Fecha.Month == mes.Value);

            // --- Filtro por trimestre (no aplica si ya hay filtro de mes) ---
            else if (trimestre.HasValue)
            {
                int mesInicio = (trimestre.Value - 1) * 3 + 1;
                int mesFin = mesInicio + 2;
                consulta = consulta.Where(p =>
                    p.Fecha.Month >= mesInicio &&
                    p.Fecha.Month <= mesFin);
            }

            var lista = consulta.ToList();

            // ── Indicadores rápidos ───────────────────────────────────────
            ViewBag.TotalPausas = lista.Count;

            // ── Distribución mensual (para gráfica de barras) ─────────────
            ViewBag.DistribucionMensual = Enumerable.Range(1, 12)
                .Select(m => new EstadisticaPausa
                {
                    Nombre = System.Globalization.CultureInfo
                                   .CurrentCulture.DateTimeFormat
                                   .GetAbbreviatedMonthName(m),
                    Cantidad = lista.Count(p => p.Fecha.Month == m)
                })
                .ToList();

            // ── Agrupación por Proceso ────────────────────────────────────
            ViewBag.EstadisticasProceso = lista
                .GroupBy(p => p.Proceso != null ? p.Proceso.NombreProceso : "Sin Proceso")
                .Select(g => new EstadisticaPausa { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // ── Agrupación por Subproceso ─────────────────────────────────
            ViewBag.EstadisticasSubproceso = lista
                .GroupBy(p => p.Subproceso != null ? p.Subproceso.NombreSubproceso : "Sin Subproceso")
                .Select(g => new EstadisticaPausa { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // ── Agrupación por Cargo ──────────────────────────────────────
            ViewBag.EstadisticasCargo = lista
                .GroupBy(p => !string.IsNullOrWhiteSpace(p.Cargo) ? p.Cargo : "No definido")
                .Select(g => new EstadisticaPausa { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            return View();
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

            TempData["Exito"] = "Pausa activa registrada correctamente.";

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
        }

        /// <summary>
        /// [4.2] Obtiene los años con registros para el selector del Dashboard.
        /// Garantiza que el año actual siempre aparezca aunque no tenga registros.
        /// </summary>
        private List<int> ObtenerAniosDisponibles()
        {
            var anios = db.PausasActivas
                .Select(p => p.Fecha.Year)
                .Distinct()
                .OrderByDescending(a => a)
                .ToList();

            int anioActual = DateTime.Today.Year;
            if (!anios.Contains(anioActual))
                anios.Insert(0, anioActual);

            return anios;
        }

        #endregion

        #region [5] Clases de Soporte

        /// <summary>
        /// Representa un ítem estadístico con nombre y cantidad
        /// para las gráficas del Dashboard.
        /// </summary>
        public class EstadisticaPausa
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