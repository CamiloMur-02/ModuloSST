using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    public class PausaActivaController : Controller
    {
        private readonly SSTContext db = new SSTContext();

        #region [1] Vistas Principales (Index & Dashboard)

        // GET: PausaActiva/Index
        public ActionResult Index()
        {
            var pausas = db.PausasActivas
                .Include(p => p.Proceso)
                .Include(p => p.Subproceso)
                .OrderByDescending(p => p.Fecha)
                .ToList();

            return View(pausas);
        }

        // GET: PausaActiva/Dashboard
        public ActionResult Dashboard(int? anio, int? mes, int? trimestre)
        {
            // Configuración de filtros iniciales
            int anioSel = anio ?? DateTime.Now.Year;
            ViewBag.AnioActual = anioSel;
            ViewBag.MesActual = mes;
            ViewBag.TrimestreActual = trimestre;

            var consulta = db.PausasActivas.AsQueryable();

            // Aplicar Filtro de Año
            consulta = consulta.Where(p => p.Fecha.Year == anioSel);

            // Aplicar Filtro de Mes
            if (mes.HasValue)
                consulta = consulta.Where(p => p.Fecha.Month == mes.Value);

            // Aplicar Filtro de Trimestre
            if (trimestre.HasValue)
            {
                int mesInicio = (trimestre.Value - 1) * 3 + 1;
                int mesFin = mesInicio + 2;
                consulta = consulta.Where(p => p.Fecha.Month >= mesInicio && p.Fecha.Month <= mesFin);
            }

            var listaFiltrada = consulta.ToList();

            // --- Cálculos para el Dashboard ---
            ViewBag.TotalPausas = listaFiltrada.Count();

            // 1. Agrupación por Proceso
            ViewBag.EstadisticasProceso = listaFiltrada
                .GroupBy(p => p.Proceso?.NombreProceso ?? "N/A")
                .Select(g => new EstadisticaPausa { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // 2. Agrupación por Subproceso
            ViewBag.EstadisticasSubproceso = listaFiltrada
                .GroupBy(p => p.Subproceso?.NombreSubproceso ?? "N/A")
                .Select(g => new EstadisticaPausa { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // 3. Agrupación por Cargo
            ViewBag.EstadisticasCargo = listaFiltrada
                .GroupBy(p => p.Cargo ?? "No definido")
                .Select(g => new EstadisticaPausa { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            return View();
        }

        #endregion

        #region [2] Registro y Detalle

        // GET: PausaActiva/Registrar
        public ActionResult Registrar()
        {
            CargarListas();
            return View(new PausaActiva { Fecha = DateTime.Today });
        }

        // POST: PausaActiva/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(PausaActiva modelo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    modelo.FechaCreacion = DateTime.Now;
                    db.PausasActivas.Add(modelo);
                    db.SaveChanges();

                    TempData["Exito"] = "Pausa activa registrada correctamente para " + modelo.NombreCompleto + ".";
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

        // GET: PausaActiva/Detalle/5
        public ActionResult Detalle(int id)
        {
            var pausa = db.PausasActivas
                .Include(p => p.Proceso)
                .Include(p => p.Subproceso)
                .FirstOrDefault(p => p.IdPausaActiva == id);

            if (pausa == null)
            {
                TempData["Error"] = "No se encontró el registro.";
                return RedirectToAction("Index");
            }

            return View(pausa);
        }

        #endregion

        #region [3] Métodos Auxiliares

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

        // Clase de apoyo para estructurar datos del Dashboard
        public class EstadisticaPausa
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
        }
    }
}