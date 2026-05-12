using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ModuloSST_HSLV.DAL; 
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.Controllers
{
    public class ReporteIncidenteController : Controller
    {

        private readonly SSTContext db = new SSTContext();

        // --- [GET] Listado Principal ---
        public ActionResult Index()
        {
            var reportes = db.ReportesIncidente
                .Include(r => r.Proceso)
                .OrderByDescending(r => r.FechaIncidente)
                .ToList();
            return View(reportes);
        }

        // --- [GET] Mostrar formulario de registro ---
        public ActionResult Registrar()
        {
            CargarListas();

            // Creamos un modelo con la fecha de hoy por defecto
            var nuevoReporte = new ReporteIncidente
            {
                FechaIncidente = DateTime.Today,
                FechaCreacion = DateTime.Now
            };

            return View(nuevoReporte);
        }

        // --- [POST] Guardar el registro en la BD ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(ReporteIncidente reporte, HttpPostedFileBase archivoIncidente, HttpPostedFileBase archivoInvestigacion, HttpPostedFileBase archivoPlan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (archivoIncidente != null && archivoIncidente.ContentLength > 0)
                    {
                        reporte.RutaArchivoIncidente = GuardarArchivo(archivoIncidente, "Incidentes");
                        reporte.NombreArchivoIncidente = archivoIncidente.FileName;
                    }

                    if (archivoInvestigacion != null && archivoInvestigacion.ContentLength > 0)
                    {
                        reporte.RutaArchivoInvestigacion = GuardarArchivo(archivoInvestigacion, "Incidentes");
                        reporte.NombreArchivoInvestigacion = archivoInvestigacion.FileName;
                    }

                    if (archivoPlan != null && archivoPlan.ContentLength > 0)
                    {
                        reporte.RutaArchivoPlan = GuardarArchivo(archivoPlan, "Incidentes");
                        reporte.NombreArchivoPlan = archivoPlan.FileName;
                    }

                    reporte.FechaCreacion = DateTime.Now;
                    db.ReportesIncidente.Add(reporte);
                    db.SaveChanges();

                    TempData["Exito"] = "Incidente registrado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                }
            }

            CargarListas();
            return View(reporte);
        }

        // --- [GET] Detalle del Incidente ---
        public ActionResult Detalle(int id)
        {
            var reporte = db.ReportesIncidente
                .Include(r => r.Proceso)
                .Include(r => r.Subproceso)
                .FirstOrDefault(r => r.IdReporteIncidente == id);

            if (reporte == null)
            {
                TempData["Error"] = "No se encontró el reporte.";
                return RedirectToAction("Index");
            }

            return View(reporte);
        }

        // --- [GET] Formulario de Edición ---
        public ActionResult Editar(int id)
        {
            var reporte = db.ReportesIncidente.Find(id);
            if (reporte == null) return HttpNotFound();

            CargarListas();
            return View(reporte);
        }

        // --- [POST] Guardar Edición ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(ReporteIncidente reporte, HttpPostedFileBase archivoIncidente, HttpPostedFileBase archivoInvestigacion, HttpPostedFileBase archivoPlan)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (archivoIncidente != null && archivoIncidente.ContentLength > 0)
                    {
                        reporte.RutaArchivoIncidente = GuardarArchivo(archivoIncidente, "Incidentes");
                        reporte.NombreArchivoIncidente = archivoIncidente.FileName;
                    }

                    if (archivoInvestigacion != null && archivoInvestigacion.ContentLength > 0)
                    {
                        reporte.RutaArchivoInvestigacion = GuardarArchivo(archivoInvestigacion, "Incidentes");
                        reporte.NombreArchivoInvestigacion = archivoInvestigacion.FileName;
                    }

                    if (archivoPlan != null && archivoPlan.ContentLength > 0)
                    {
                        reporte.RutaArchivoPlan = GuardarArchivo(archivoPlan, "Incidentes");
                        reporte.NombreArchivoPlan = archivoPlan.FileName;
                    }

                    reporte.FechaModificacion = DateTime.Now;
                    db.Entry(reporte).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["Exito"] = "Reporte actualizado con éxito.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                }
            }

            CargarListas();
            return View(reporte);
        }

        // --- [GET] Descargar Archivos ---
        public ActionResult Descargar(string ruta, string nombre)
        {
            if (string.IsNullOrEmpty(ruta)) return HttpNotFound();

            string rutaFisica = Server.MapPath(ruta);

            if (!System.IO.File.Exists(rutaFisica)) return HttpNotFound();

            return File(rutaFisica, "application/pdf", nombre);
        }

        // --- [GET] Dashboard de Indicadores ---
        public ActionResult Dashboard(int? anio, int? trimestre)
        {
            int anioSel = anio ?? DateTime.Now.Year;
            ViewBag.AnioActual = anioSel;
            ViewBag.TrimestreActual = trimestre;

            var consulta = db.ReportesIncidente.AsQueryable();

            if (anio.HasValue)
                consulta = consulta.Where(r => r.FechaIncidente.Year == anio.Value);

            if (trimestre.HasValue)
            {
                int mesInicio = (trimestre.Value - 1) * 3 + 1;
                int mesFin = mesInicio + 2;
                consulta = consulta.Where(r => r.FechaIncidente.Month >= mesInicio && r.FechaIncidente.Month <= mesFin);
            }

            var listaFiltrada = consulta.ToList();

            ViewBag.TotalIncidentes = listaFiltrada.Count();

            ViewBag.TopProcesos = listaFiltrada
                .GroupBy(r => r.Proceso?.NombreProceso ?? "N/A")
                .Select(g => new EstadisticaItemIncidente { Nombre = g.Key, Cantidad = g.Count() })
                .ToList();

            ViewBag.EstadisticasPeligros = listaFiltrada
                .GroupBy(r => r.TipoPeligro)
                .Select(g => new EstadisticaItemIncidente { Nombre = g.Key, Cantidad = g.Count() })
                .ToList();

            ViewBag.EstadisticasCausas = listaFiltrada
                .GroupBy(r => r.Causa ?? "No definida")
                .Select(g => new EstadisticaItemIncidente { Nombre = g.Key, Cantidad = g.Count() })
                .ToList();

            return View();
        }


        public class EstadisticaItemIncidente
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
        }

        // --- Métodos Auxiliares ---
        private void CargarListas()
        {
            ViewBag.Procesos = db.Procesos.OrderBy(p => p.NombreProceso).ToList();
            ViewBag.Subprocesos = db.Subprocesos.OrderBy(s => s.NombreSubproceso).ToList();
            ViewBag.Generos = new[] { "Masculino", "Femenino", "Otro" };
            ViewBag.Turnos = new[] { "Mañana", "Tarde", "Noche", "Rotativo" };
            ViewBag.TiposPeligro = new[] { "Biológico", "Físico", "Químico", "Psicosocial", "Biomecánico", "Condiciones de Seguridad", "Fenómenos Naturales", "Otro" };

            // Diccionario de Causas y Subcausas para el Registro/Edición
            var causas = new Dictionary<string, string[]> {
        { "Manejo inadecuado de cortopunzantes", new string[] { "Falta de autocuidado", "Reencapuchar agujas", "Manejo inseguro durante procedimientos", "Segregación inadecuada" } },
        { "Posturas y movimientos inadecuados / ergonomía", new string[] { "Posturas inadecuadas", "Movimientos inadecuados", "Sobreesfuerzo", "Levantamiento incorrecto" } },
        { "Caídas y resbalones", new string[] { "Piso húmedo", "Falta de señalización", "Desniveles sin demarcar", "Exceso de confianza" } },
        { "Movilización inadecuada de pacientes", new string[] { "Técnica incorrecta", "Falta de apoyo", "No solicitar ayuda" } },
        { "Factores externos / terceros", new string[] { "Acciones inseguras de terceros", "Pacientes agresivos", "Bloqueos violentos" } }
    };

            ViewBag.CausasSubcausas = causas;
        }

        private string GuardarArchivo(HttpPostedFileBase file, string carpeta)
        {
            if (file == null || file.ContentLength == 0) return null;

            string subPath = "~/Uploads/" + carpeta + "/";
            string folderPath = Server.MapPath(subPath);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            file.SaveAs(Path.Combine(folderPath, fileName));

            return subPath + fileName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}