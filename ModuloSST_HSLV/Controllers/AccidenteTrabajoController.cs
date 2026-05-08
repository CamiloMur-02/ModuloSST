using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;
using ModuloSST_HSLV.Helpers;
using System.Collections.Generic;

namespace ModuloSST_HSLV.Controllers
{
    
    public class EstadisticaItem
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
    }

    public class ReporteAccidenteTrabajoController : Controller
    {
        private readonly SSTContext db = new SSTContext();

        // --- 1. VISTA PRINCIPAL (INDEX) ---
        public ActionResult Index()
        {
            var reportes = db.ReportesAccidenteTrabajo.Include(r => r.Proceso).ToList();
            return View(reportes);
        }

        // --- 2. DASHBOARD (GRÁFICOS Y ESTADÍSTICAS) ---     
        public ActionResult Dashboard(int? anio, int? mes, int? trimestre)
        {
            int filtroAnio = anio ?? DateTime.Today.Year;
            var query = db.ReportesAccidenteTrabajo.AsQueryable();
            query = query.Where(r => r.FechaAT.Year == filtroAnio);

            if (mes.HasValue && mes > 0)
            {
                query = query.Where(r => r.FechaAT.Month == mes.Value);
            }
            else if (trimestre.HasValue && trimestre > 0)
            {
                int mesInicio = (trimestre.Value - 1) * 3 + 1;
                int mesFin = mesInicio + 2;
                query = query.Where(r => r.FechaAT.Month >= mesInicio && r.FechaAT.Month <= mesFin);
            }

            var datosFiltrados = query.ToList();

            // Totales para tarjetas
            ViewBag.TotalAccidentes = datosFiltrados.Count();
            ViewBag.TotalIncapacidad = datosFiltrados.Sum(r => (int?)r.DiasIncapacidad) ?? 0;

            // Datos para Gráfico de Procesos
            ViewBag.TopProcesos = datosFiltrados
                .GroupBy(r => r.Proceso?.NombreProceso ?? "Sin Proceso")
                .Select(g => new EstadisticaItem { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad).Take(5).ToList();

            // Datos para Gráfico de Peligros
            ViewBag.EstadisticasPeligros = datosFiltrados
                .GroupBy(r => r.TipoPeligro ?? "No definido")
                .Select(g => new EstadisticaItem { Nombre = g.Key, Cantidad = g.Count() }).ToList();

            // Datos para Tabla de Causas
            ViewBag.EstadisticasCausas = datosFiltrados
                .Where(r => !string.IsNullOrEmpty(r.Causa))
                .GroupBy(r => r.Causa)
                .Select(g => new EstadisticaItem { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad).ToList();

            ViewBag.AnioActual = filtroAnio;
            ViewBag.TrimestreActual = trimestre;

            return View();
        }

        // --- 3. REGISTRAR ---
        public ActionResult Registrar()
        {
            CargarListas();
            return View(new ReporteAccidenteTrabajo { FechaAT = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(ReporteAccidenteTrabajo modelo)
        {
            if (ModelState.IsValid)
            {
                modelo.FechaCreacion = DateTime.Now;
                db.ReportesAccidenteTrabajo.Add(modelo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            CargarListas();
            return View(modelo);
        }

        // --- 4. EDITAR ---
        public ActionResult Editar(int id)
        {
            var reporte = db.ReportesAccidenteTrabajo.Find(id);
            if (reporte == null) return HttpNotFound();
            CargarListas();
            return View(reporte);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(ReporteAccidenteTrabajo modelo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(modelo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            CargarListas();
            return View(modelo);
        }

        // --- 5. DETALLE ---
        public ActionResult Detalle(int id)
        {
            var reporte = db.ReportesAccidenteTrabajo
                .Include(r => r.Proceso)
                .Include(r => r.Subproceso)
                .FirstOrDefault(r => r.IdReporteAccidenteTrabajo == id);

            if (reporte == null) return HttpNotFound();
            return View(reporte);
        }

        private void CargarListas()
        {
            ViewBag.Procesos = db.Procesos.OrderBy(p => p.NombreProceso).ToList() ?? new List<Proceso>();
            ViewBag.Subprocesos = db.Subprocesos.OrderBy(s => s.NombreSubproceso).ToList() ?? new List<Subproceso>();
            ViewBag.Generos = new string[] { "Masculino", "Femenino", "Otro" };
            ViewBag.TiposPeligro = new string[] { "Físico", "Químico", "Biológico", "Biomecánico", "Psicosocial", "Mecánico", "Locativo", "Público", "Otro" };
            ViewBag.Turnos = new string[] { "Mañana", "Tarde", "Noche", "Rotativo" };
            ViewBag.CausasSubcausas = ModuloSST_HSLV.Helpers.CausasHelper.ObtenerCausasSubcausas() ?? new Dictionary<string, string[]>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}