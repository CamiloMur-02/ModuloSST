using System;
using System.Collections.Generic;
using System.Data.Entity;        
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    public class InspeccionBioseguridadController : Controller
    {
        private readonly SSTContext db = new SSTContext();

        #region [1] Vistas Principales (Index & Dashboard)

        // GET: InspeccionBioseguridad/Index
        public ActionResult Index()
        {
            var inspecciones = db.InspeccionesBioseguridad
                .Include(i => i.Proceso)
                .Include(i => i.Subproceso)
                .OrderByDescending(i => i.FechaInspeccion)
                .ToList();

            return View(inspecciones);
        }

        // GET: InspeccionBioseguridad/Dashboard
        public ActionResult Dashboard(int? anio, int? trimestre)
        {
            int anioSel = anio ?? DateTime.Now.Year;
            ViewBag.AnioActual = anioSel;
            ViewBag.TrimestreActual = trimestre;

            var consulta = db.InspeccionesBioseguridad.AsQueryable();

            if (anio.HasValue)
                consulta = consulta.Where(i => i.FechaInspeccion.Year == anio.Value);

            if (trimestre.HasValue)
            {
                int mesInicio = (trimestre.Value - 1) * 3 + 1;
                int mesFin = mesInicio + 2;
                consulta = consulta.Where(i => i.FechaInspeccion.Month >= mesInicio && i.FechaInspeccion.Month <= mesFin);
            }

            var lista = consulta.ToList();
            int totalInspecciones = lista.Count();
            ViewBag.TotalInspecciones = totalInspecciones;

            // 1. Estadísticas por Ítem (Conteo Exacto y Porcentajes)
            var estadisticasItems = new List<EstadisticaBioseguridad>();

            if (totalInspecciones > 0)
            {
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Higiene de Manos", Cumple = lista.Count(x => x.HigieneManos == "C"), NoCumple = lista.Count(x => x.HigieneManos == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Protección Respiratoria", Cumple = lista.Count(x => x.ProteccionRespiratoria == "C"), NoCumple = lista.Count(x => x.ProteccionRespiratoria == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Uso de Guantes", Cumple = lista.Count(x => x.UsoGuantes == "C"), NoCumple = lista.Count(x => x.UsoGuantes == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Uso de Gorro", Cumple = lista.Count(x => x.UsoGorro == "C"), NoCumple = lista.Count(x => x.UsoGorro == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Uso de Bata", Cumple = lista.Count(x => x.UsoBata == "C"), NoCumple = lista.Count(x => x.UsoBata == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Protección Visual", Cumple = lista.Count(x => x.ProteccionVisual == "C"), NoCumple = lista.Count(x => x.ProteccionVisual == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Uso Delantal/Peto", Cumple = lista.Count(x => x.UsoDelantalPeto == "C"), NoCumple = lista.Count(x => x.UsoDelantalPeto == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Calzado Adecuado", Cumple = lista.Count(x => x.CalzadoAdecuado == "C"), NoCumple = lista.Count(x => x.CalzadoAdecuado == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Transporte de Muestras", Cumple = lista.Count(x => x.TransporteMuestras == "C"), NoCumple = lista.Count(x => x.TransporteMuestras == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Manejo Cortopunzantes", Cumple = lista.Count(x => x.ManejoCorto == "C"), NoCumple = lista.Count(x => x.ManejoCorto == "NC") });
                estadisticasItems.Add(new EstadisticaBioseguridad { Nombre = "Manejo de Residuos", Cumple = lista.Count(x => x.ManejoResiduos == "C"), NoCumple = lista.Count(x => x.ManejoResiduos == "NC") });
            }

            ViewBag.EstadisticasItems = estadisticasItems;

            // 2. Adherencia por Proceso
            ViewBag.AdherenciaProceso = lista
                .GroupBy(i => i.Proceso?.NombreProceso ?? "N/A")
                .Select(g => new {
                    Nombre = g.Key,
                    Porcentaje = g.Average(x => (x.TotalCumple + x.TotalNoCumple) > 0 ? (double)x.TotalCumple / (x.TotalCumple + x.TotalNoCumple) * 100 : 0)
                }).ToList();

            // 3. Adherencia por Subproceso (Ej: Cirugía Adultos)
            ViewBag.AdherenciaSubproceso = lista
                .GroupBy(i => i.Subproceso?.NombreSubproceso ?? "N/A")
                .Select(g => new {
                    Nombre = g.Key,
                    Porcentaje = g.Average(x => (x.TotalCumple + x.TotalNoCumple) > 0 ? (double)x.TotalCumple / (x.TotalCumple + x.TotalNoCumple) * 100 : 0)
                }).ToList();

            return View();
        }

        #endregion

        #region [2] Métodos de Registro

        public ActionResult Registrar()
        {
            CargarListas();
            return View(new InspeccionBioseguridad { FechaInspeccion = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(InspeccionBioseguridad modelo)
        {
            if (ModelState.IsValid)
            {
                // Cálculo automático de totales antes de guardar
                var items = new[] {
                    modelo.HigieneManos, modelo.ProteccionRespiratoria, modelo.UsoGuantes,
                    modelo.UsoGorro, modelo.UsoBata, modelo.ProteccionVisual,
                    modelo.UsoDelantalPeto, modelo.CalzadoAdecuado, modelo.TransporteMuestras,
                    modelo.ManejoCorto, modelo.ManejoResiduos
                };

                modelo.TotalCumple = items.Count(v => v == "C");
                modelo.TotalNoCumple = items.Count(v => v == "NC");
                modelo.TotalNoAplica = items.Count(v => v == "NA");
                modelo.FechaCreacion = DateTime.Now;

                try
                {
                    db.InspeccionesBioseguridad.Add(modelo);
                    db.SaveChanges();
                    TempData["Exito"] = "Inspección registrada correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                }
            }

            CargarListas();
            return View(modelo);
        }

        public ActionResult Detalle(int id)
        {
            var inspeccion = db.InspeccionesBioseguridad
                .Include(i => i.Proceso)
                .Include(i => i.Subproceso)
                .FirstOrDefault(i => i.IdInspeccionBioseguridad == id);

            if (inspeccion == null) return RedirectToAction("Index");
            return View(inspeccion);
        }

        #endregion

        #region [3] Auxiliares

        private void CargarListas()
        {
            ViewBag.Procesos = db.Procesos.OrderBy(p => p.NombreProceso).ToList();
            ViewBag.Subprocesos = db.Subprocesos.OrderBy(s => s.NombreSubproceso).ToList();
            ViewBag.Generos = new[] { "Masculino", "Femenino", "Otro" };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        #endregion



        // CLASE DE APOYO PARA EL DASHBOARD
        public class EstadisticaBioseguridad
        {
            public string Nombre { get; set; }
            public int Cumple { get; set; }
            public int NoCumple { get; set; }
            public double PorcentajeAdherencia => (Cumple + NoCumple) > 0 ? (double)Cumple / (Cumple + NoCumple) * 100 : 0;
        }
    }
}