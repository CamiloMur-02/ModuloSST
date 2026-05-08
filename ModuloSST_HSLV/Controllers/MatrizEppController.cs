using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    
    public class EstadisticaEpp
    {
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
    }

    public class MatrizEppController : Controller
    {
        private readonly SSTContext db = new SSTContext();

        #region [1] Dashboard
        public ActionResult Dashboard(int? anio)
        {
            int anioSel = anio ?? DateTime.Now.Year;
            ViewBag.AnioActual = anioSel;

            var entregas = db.ElementosEPP
                .Include(e => e.CatalogoEpp)
                .Where(e => e.Activo && e.FechaEntrega.HasValue && e.FechaEntrega.Value.Year == anioSel)
                .ToList();

            int[] cantidadesMes = new int[12];
            var resumenMensual = entregas.GroupBy(e => e.FechaEntrega.Value.Month)
                                         .Select(g => new { Mes = g.Key, Cant = g.Count() });
            foreach (var item in resumenMensual) cantidadesMes[item.Mes - 1] = item.Cant;
            ViewBag.DatosMensuales = cantidadesMes;

            int[] trimestres = new int[4];
            trimestres[0] = cantidadesMes[0] + cantidadesMes[1] + cantidadesMes[2];
            trimestres[1] = cantidadesMes[3] + cantidadesMes[4] + cantidadesMes[5];
            trimestres[2] = cantidadesMes[6] + cantidadesMes[7] + cantidadesMes[8];
            trimestres[3] = cantidadesMes[9] + cantidadesMes[10] + cantidadesMes[11];
            ViewBag.DatosTrimestrales = trimestres;

            ViewBag.TopElementos = entregas
                .GroupBy(e => e.CatalogoEpp.NombreEPP)
                .Select(g => new EstadisticaEpp { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad).Take(10).ToList();

            return View();
        }
        #endregion

        #region [2] Vistas Principales

        public ActionResult Index()
        {
            var matriz = db.MatrizEPP
                .Include(m => m.Elementos.Select(e => e.CatalogoEpp))
                .Include(m => m.Proceso)
                .Include(m => m.Subproceso)
                .OrderByDescending(m => m.FechaRegistro).ToList();

            foreach (var registro in matriz)
            {
                foreach (var elem in registro.Elementos.Where(e => e.Activo && e.FechaEntrega.HasValue))
                {
                    // Calculamos la fecha de vencimiento real
                    DateTime fechaVencimiento = elem.FechaEntrega.Value.AddMonths(elem.TiempoAlerta);

                    // Si hoy es ANTES de la fecha de vencimiento, NO está vencido
                    if (DateTime.Today < fechaVencimiento)
                    {
                        TimeSpan diferencia = fechaVencimiento - DateTime.Today;
                        // Calculamos meses restantes (ejemplo: 0.5, 1.2, etc.)
                        elem.TiempoRestante = (decimal)(diferencia.TotalDays / 30.44);
                    }
                    else
                    {
                        // Si hoy es igual o mayor, está vencido
                        elem.TiempoRestante = 0;
                    }
                }
            }
            return View(matriz);
        }

        public ActionResult Registrar()
        {
            CargarListas();
            return View(new MatrizEpp { FechaRegistro = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(MatrizEpp modelo, int[] idsEppSeleccionados, FormCollection form)
        {
            if (ModelState.IsValid && idsEppSeleccionados != null)
            {
                modelo.Elementos = new List<ElementoEpp>();
                foreach (int idEpp in idsEppSeleccionados)
                {
                    if (DateTime.TryParse(form["fecha_" + idEpp], out DateTime f))
                    {
                        modelo.Elementos.Add(new ElementoEpp
                        {
                            IdCatalogoEPP = idEpp,
                            FechaEntrega = f,
                            TiempoAlerta = int.Parse(form["alerta_" + idEpp] ?? "12"),
                            Activo = true,
                            FechaCreacion = DateTime.Now
                        });
                    }
                }
                modelo.FechaCreacion = DateTime.Now;
                db.MatrizEPP.Add(modelo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            CargarListas();
            return View(modelo);
        }

        public ActionResult Editar(int id)
        {
            var registro = db.MatrizEPP
                .Include(m => m.Elementos.Select(e => e.CatalogoEpp))
                .Include(m => m.Proceso)
                .Include(m => m.Subproceso)
                .FirstOrDefault(m => m.IdMatrizEPP == id);

            if (registro == null) return RedirectToAction("Index");
            CargarListas();
            return View(registro);
        }
        #endregion

        #region [3] Acciones de Edición (Botones de la Vista)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExcluirElemento(int idElemento, int idMatriz)
        {
            var elem = db.ElementosEPP.Find(idElemento);
            if (elem != null)
            {
                elem.Activo = false;
                elem.FechaModificacion = DateTime.Now;
                db.Entry(elem).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Editar", new { id = idMatriz });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarElemento(int idMatriz, int idCatalogo, DateTime? fechaEntrega, int tiempoAlerta = 12)
        {
            if (fechaEntrega.HasValue)
            {
                var nuevo = new ElementoEpp
                {
                    IdMatrizEPP = idMatriz,
                    IdCatalogoEPP = idCatalogo,
                    FechaEntrega = fechaEntrega.Value,
                    TiempoAlerta = tiempoAlerta,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };
                db.ElementosEPP.Add(nuevo);
                db.SaveChanges();
            }
            return RedirectToAction("Editar", new { id = idMatriz });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarObservaciones(int idMatriz, string ObservacionesFinales)
        {
            var matriz = db.MatrizEPP.Find(idMatriz);
            if (matriz != null)
            {
                matriz.ObservacionesFinales = ObservacionesFinales;
                matriz.FechaModificacion = DateTime.Now;
                db.Entry(matriz).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Editar", new { id = idMatriz });
        }

        #endregion

        private void CargarListas()
        {
            ViewBag.Procesos = db.Procesos.OrderBy(p => p.NombreProceso).ToList();
            ViewBag.Subprocesos = db.Subprocesos.OrderBy(s => s.NombreSubproceso).ToList();
            ViewBag.CatalogoEPP = db.CatalogoEPP.Where(c => c.Estado).OrderBy(c => c.NombreEPP).ToList();
            ViewBag.Generos = new string[] { "Masculino", "Femenino", "Otro" };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}