using System.Data.Entity;
using ModuloSST_HSLV.Models;
using ModuloSST_HSLV.Models.HojaVida;

namespace ModuloSST_HSLV.DAL
{
    /// <summary>
    /// Contexto principal de Entity Framework para el Módulo SST.
    /// La cadena de conexión 'SSTContext' se define en Web.config.
    /// Apunta a la base de datos SST_SUSANA.
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

    /// <summary>
    /// Contexto secundario de Entity Framework para la BD HOJAVIDA_EST.
    /// Usado exclusivamente para la consulta de datos del empleado
    /// en el autocompletado de los 6 módulos SST.
    /// La cadena de conexión 'HojaVidaContext' se define en Web.config.
    /// Solo lectura — no se realizan escrituras sobre esta BD.
    /// </summary>
    public class HojaVidaContext : DbContext
    {
        public HojaVidaContext() : base("name=HojaVidaContext")
        {
            // Desactiva migraciones automáticas — BD externa de solo lectura
            Database.SetInitializer<HojaVidaContext>(null);
            // Timeout de 60 segundos para consultas
            Database.CommandTimeout = 60;
        }

        // ── Datos del funcionario ─────────────────────────────
        public DbSet<HvFuncionario> Funcionarios { get; set; }
        public DbSet<HvCargo> Cargos { get; set; }

        // ── Datos laborales ───────────────────────────────────
        public DbSet<HvDatosLaborales> DatosLaborales { get; set; }
        public DbSet<HvSindicato> Sindicatos { get; set; }

        // ── Proceso y Subproceso ──────────────────────────────
        public DbSet<HvProceso> Procesos { get; set; }
        public DbSet<HvSubproceso> Subprocesos { get; set; }
    }
}