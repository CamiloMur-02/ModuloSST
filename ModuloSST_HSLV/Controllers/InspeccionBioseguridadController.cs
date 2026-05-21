using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    /// <summary>
    /// Controlador Inspección de Bioseguridad
    /// Gestiona el registro, consulta y estadísticas de inspecciones de bioseguridad.
    /// </summary>
    /// <remarks>
    /// Funcionalidades:
    ///  - Registro de inspecciones
    ///  - Consulta de listado
    ///  - Visualización de detalle
    ///  - Dashboard de indicadores con filtros
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

        /// <summary>
        /// [2.4] Dashboard de indicadores de bioseguridad.
        /// Permite filtrar por año y trimestre.
        /// </summary>
        /// <remarks>
        /// Muestra:
        ///  - Estadísticas de cumplimiento por cada ítem evaluado
        ///  - Adherencia promedio por Proceso
        ///  - Adherencia promedio por Subproceso
        ///  - Indicadores rápidos: total de inspecciones
        /// </remarks>
        /// <param name="anio">Año a filtrar (por defecto el año actual).</param>
        /// <param name="trimestre">Trimestre a filtrar (1–4). Nulo = todo el año.</param>
        public ActionResult Dashboard(int? anio, int? trimestre)
        {
            int filtroAnio = anio ?? DateTime.Today.Year;

            if (trimestre.HasValue && (trimestre.Value < 1 || trimestre.Value > 4))
                trimestre = null;

            ViewBag.AnioActual = filtroAnio;
            ViewBag.TrimestreActual = trimestre;
            ViewBag.AniosDisponibles = ObtenerAniosDisponibles();

            // --- Consulta base filtrada ---
            var consulta = db.InspeccionesBioseguridad
                .Include("Proceso")
                .Include("Subproceso")
                .Where(i => i.FechaInspeccion.Year == filtroAnio);

            if (trimestre.HasValue)
            {
                int mesInicio = (trimestre.Value - 1) * 3 + 1;
                int mesFin = mesInicio + 2;
                consulta = consulta.Where(i =>
                    i.FechaInspeccion.Month >= mesInicio &&
                    i.FechaInspeccion.Month <= mesFin);
            }

            var lista = consulta.ToList();

            // ── Indicadores rápidos ───────────────────────────────────────
            ViewBag.TotalInspecciones = lista.Count;

            // ── Estadísticas por ítem (Cumple / No Cumple / Porcentaje) ───
            var estadisticasItems = new List<EstadisticaBioseguridad>();

            if (lista.Count > 0)
            {
                estadisticasItems.Add(CrearEstadistica("Higiene de Manos",
                    lista, x => x.HigieneManos));
                estadisticasItems.Add(CrearEstadistica("Protección Respiratoria",
                    lista, x => x.ProteccionRespiratoria));
                estadisticasItems.Add(CrearEstadistica("Uso de Guantes",
                    lista, x => x.UsoGuantes));
                estadisticasItems.Add(CrearEstadistica("Uso de Gorro",
                    lista, x => x.UsoGorro));
                estadisticasItems.Add(CrearEstadistica("Uso de Bata",
                    lista, x => x.UsoBata));
                estadisticasItems.Add(CrearEstadistica("Protección Visual",
                    lista, x => x.ProteccionVisual));
                estadisticasItems.Add(CrearEstadistica("Uso Delantal/Peto",
                    lista, x => x.UsoDelantalPeto));
                estadisticasItems.Add(CrearEstadistica("Calzado Adecuado",
                    lista, x => x.CalzadoAdecuado));
                estadisticasItems.Add(CrearEstadistica("Transporte de Muestras",
                    lista, x => x.TransporteMuestras));
                estadisticasItems.Add(CrearEstadistica("Manejo Cortopunzantes",
                    lista, x => x.ManejoCorto));
                estadisticasItems.Add(CrearEstadistica("Manejo de Residuos",
                    lista, x => x.ManejoResiduos));
            }

            ViewBag.EstadisticasItems = estadisticasItems;

            // ── Adherencia promedio por Proceso ───────────────────────────
            ViewBag.AdherenciaProceso = lista
                .GroupBy(i => i.Proceso != null ? i.Proceso.NombreProceso : "Sin Proceso")
                .Select(g => new EstadisticaBioseguridad
                {
                    Nombre = g.Key,
                    Cumple = (int)g.Average(x => x.TotalCumple),
                    NoCumple = (int)g.Average(x => x.TotalNoCumple)
                })
                .OrderByDescending(x => x.PorcentajeAdherencia)
                .ToList();

            // ── Adherencia promedio por Subproceso ────────────────────────
            ViewBag.AdherenciaSubproceso = lista
                .GroupBy(i => i.Subproceso != null ? i.Subproceso.NombreSubproceso : "Sin Subproceso")
                .Select(g => new EstadisticaBioseguridad
                {
                    Nombre = g.Key,
                    Cumple = (int)g.Average(x => x.TotalCumple),
                    NoCumple = (int)g.Average(x => x.TotalNoCumple)
                })
                .OrderByDescending(x => x.PorcentajeAdherencia)
                .ToList();

            return View();
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

            TempData["Exito"] = "Inspección de bioseguridad registrada correctamente.";

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
            var anios = db.InspeccionesBioseguridad
                .Select(i => i.FechaInspeccion.Year)
                .Distinct()
                .OrderByDescending(a => a)
                .ToList();

            int anioActual = DateTime.Today.Year;
            if (!anios.Contains(anioActual))
                anios.Insert(0, anioActual);

            return anios;
        }

        /// <summary>
        /// [4.3] Crea un ítem de estadística para un campo evaluado,
        /// contando cuántas inspecciones cumplen y cuántas no cumplen.
        /// </summary>
        /// <param name="nombre">Nombre del ítem evaluado.</param>
        /// <param name="lista">Lista de inspecciones filtradas.</param>
        /// <param name="selector">Función que selecciona el campo del ítem.</param>
        private static EstadisticaBioseguridad CrearEstadistica(
            string nombre,
            List<InspeccionBioseguridad> lista,
            Func<InspeccionBioseguridad, string> selector)
        {
            return new EstadisticaBioseguridad
            {
                Nombre = nombre,
                Cumple = lista.Count(x => selector(x) == "C"),
                NoCumple = lista.Count(x => selector(x) == "NC")
            };
        }

        #endregion

        #region [5] Clases de Soporte

        /// <summary>
        /// Ítem estadístico de bioseguridad con nombre, conteos
        /// y porcentaje de adherencia calculado automáticamente.
        /// </summary>
        public class EstadisticaBioseguridad
        {
            public string Nombre { get; set; }
            public int Cumple { get; set; }
            public int NoCumple { get; set; }

            /// <summary>
            /// Porcentaje de adherencia calculado sobre Cumple + NoCumple.
            /// Retorna 0 si no hay evaluaciones aplicables.
            /// </summary>
            public double PorcentajeAdherencia =>
                (Cumple + NoCumple) > 0
                    ? (double)Cumple / (Cumple + NoCumple) * 100
                    : 0;
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