using System.Data.Entity;
using ModuloSST_HSLV.Models;

namespace ModuloSST_HSLV.DAL
{
    /// <summary>
    /// Contexto principal de Entity Framework para el Módulo SST.
    /// La cadena de conexión 'SSTContext' se define en Web.config.
    /// Apunta a SQL Server 2022 en Docker (sst_hslv_sqlserver, puerto 1433).
    /// </summary>
    public class SSTContext : DbContext
    {
        public SSTContext() : base("name=SSTContext")
        {
            // Desactiva migraciones automáticas — la BD se gestiona manualmente
            Database.SetInitializer<SSTContext>(null);

            // Timeout de 60 segundos para consultas (por defecto es 30)
            Database.CommandTimeout = 60;
        }

        // ── Módulo Evaluaciones ───────────────────────────────
        public DbSet<Evaluacion> Evaluaciones { get; set; }

        // ── Módulo Inspección de Bioseguridad ────────────────
        public DbSet<InspeccionBioseguridad> InspeccionesBioseguridad { get; set; }

        // ── Módulo Matriz EPP ─────────────────────────────────
        public DbSet<CatalogoEpp> CatalogoEPP { get; set; }
        public DbSet<MatrizEpp> MatrizEPP { get; set; }
        public DbSet<ElementoEpp> ElementosEPP { get; set; }

        // ── Catálogos compartidos ─────────────────────────────
        public DbSet<Proceso> Procesos { get; set; }
        public DbSet<Subproceso> Subprocesos { get; set; }

        // ── Módulo Reportes SST ───────────────────────────────
        public DbSet<ReporteAccidenteTrabajo> ReportesAccidenteTrabajo { get; set; }
        public DbSet<ReporteEnfermedadLaboral> ReportesEnfermedadLaboral { get; set; }
        public DbSet<ReporteIncidente> ReportesIncidente { get; set; }

        // ── Módulo Pausas Activas ─────────────────────────────
        public DbSet<PausaActiva> PausasActivas { get; set; }
    }
}