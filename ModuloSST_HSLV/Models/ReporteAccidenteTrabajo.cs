using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Entidad: Reporte de Accidente de Trabajo
    /// 
    /// Representa el registro completo de un accidente de trabajo ocurrido en el Hospital Susana López de Valencia.
    /// 
    /// Flujo del proceso:
    /// - Datos del accidentado
    /// - Información del accidente
    /// - Investigación y seguimiento
    /// - Plan de mejora
    /// - Observaciones finales
    /// 
    /// Reglas de negocio:
    /// - Los campos obligatorios deben completarse antes de guardar.
    /// - Los días de investigación se calculan en el controlador.
    /// - Los archivos se almacenan en el servidor y se referencian por ruta.
    /// </summary>
    [Table("ReporteAccidenteTrabajo")]
    public class ReporteAccidenteTrabajo
    {
        #region [1] Clave primaria

        [Key]
        public int IdReporteAccidenteTrabajo { get; set; }

        #endregion


        #region [2] Datos del accidentado

        /// <summary>
        /// FK reservada para integración futura con el módulo de Hojas de Vida.
        /// Nullable hasta que se realice la integración.
        /// </summary>
        [Display(Name = "ID Empleado")]
        public int? IdEmpleado { get; set; }

        /// <summary>
        /// Número de identificación del colaborador.
        /// </summary>
        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        /// <summary>
        /// Nombre completo del colaborador.
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        [Display(Name = "Apellidos y Nombre")]
        public string NombreCompleto { get; set; }

        /// <summary>
        /// Género del colaborador.
        /// </summary>
        [Required(ErrorMessage = "El género es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Género")]
        public string Genero { get; set; }

        /// <summary>
        /// Edad del colaborador.
        /// </summary>
        [Display(Name = "Edad")]
        public int Edad { get; set; }

        /// <summary>
        /// Cargo desempeñado.
        /// </summary>
        [Required(ErrorMessage = "El cargo es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; }

        /// <summary>
        /// Empresa o contratista asociado.
        /// </summary>
        [Required(ErrorMessage = "El contratista es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Contratista")]
        public string Contratista { get; set; }

        /// <summary>
        /// Tiempo de prestación del servicio.
        /// </summary>
        [Required(ErrorMessage = "El tiempo de prestación es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Tiempo de Prestación (meses)")]
        public string TiempoPrestacion { get; set; }

        #endregion


        #region [3] Información del accidente

        /// <summary>
        /// Número consecutivo del reporte.
        /// </summary>
        [Required(ErrorMessage = "El número de reporte es obligatorio.")]
        [Display(Name = "Número de Reporte")]
        public int NumeroReporte { get; set; }

        /// <summary>
        /// Fecha en que ocurrió el accidente.
        /// </summary>
        [Required(ErrorMessage = "La fecha del accidente es obligatoria.")]
        [Display(Name = "Fecha del Accidente")]
        [DataType(DataType.Date)]
        public DateTime FechaAT { get; set; }

        /// <summary>
        /// Hora del accidente.
        /// </summary>
        [Display(Name = "Hora del Accidente")]
        public TimeSpan? HoraAT { get; set; }

        /// <summary>
        /// Tipo de peligro asociado al accidente.
        /// </summary>
        [Required(ErrorMessage = "El tipo de peligro es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Tipo de Peligro")]
        public string TipoPeligro { get; set; }

        /// <summary>
        /// Código del diagnóstico médico.
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Código de Diagnóstico")]
        public string CodigoDiagnostico { get; set; }

        /// <summary>
        /// Descripción del diagnóstico.
        /// </summary>
        [Required(ErrorMessage = "El detalle del diagnóstico es obligatorio.")]
        [StringLength(300, ErrorMessage = "Máximo 300 caracteres.")]
        [Display(Name = "Detalle del Diagnóstico")]
        public string DetalleDiagnostico { get; set; }

        /// <summary>
        /// Días de incapacidad generados.
        /// </summary>
        [Display(Name = "Días de Incapacidad")]
        public int DiasIncapacidad { get; set; }

        /// <summary>
        /// Fecha en que se realizó el reporte.
        /// </summary>
        [Display(Name = "Fecha del Reporte")]
        [DataType(DataType.Date)]
        public DateTime? FechaReporte { get; set; }

        /// <summary>
        /// Observaciones generales.
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Observaciones (Generales)")]
        public string Observaciones { get; set; }

        /// <summary>
        /// Observaciones específicas del accidente.
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Observaciones del Accidente")]
        public string ObservacionesAccidente { get; set; }

        /// <summary>
        /// Ruta del archivo adjunto del accidente.
        /// </summary>
        [StringLength(500)]
        public string RutaArchivoAccidente { get; set; }

        /// <summary>
        /// Nombre del archivo adjunto del accidente.
        /// </summary>
        [StringLength(255)]
        public string NombreArchivoAccidente { get; set; }

        #endregion


        #region [4] Investigación y seguimiento

        /// <summary>
        /// Fecha en que se realizó la investigación.
        /// </summary>
        [Display(Name = "Fecha de Investigación")]
        [DataType(DataType.Date)]
        public DateTime? FechaInvestigacion { get; set; }

        /// <summary>
        /// Días entre accidente e investigación (calculado en controlador).
        /// </summary>
        [Display(Name = "Días de Investigación")]
        public int? DiasInvestigacion { get; set; }

        /// <summary>
        /// Turno en el que ocurrió el evento.
        /// </summary>
        [StringLength(20)]
        [Display(Name = "Turno")]
        public string Turno { get; set; }

        /// <summary>
        /// Causa del accidente.
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Causa")]
        public string Causa { get; set; }

        /// <summary>
        /// Subcausa del accidente.
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Subcausa")]
        public string Subcausa { get; set; }

        /// <summary>
        /// Ruta del archivo de investigación.
        /// </summary>
        [StringLength(500)]
        public string RutaArchivoInvestigacion { get; set; }

        /// <summary>
        /// Nombre del archivo de investigación.
        /// </summary>
        [StringLength(255)]
        public string NombreArchivoInvestigacion { get; set; }

        #endregion


        #region [5] Plan de mejora

        /// <summary>
        /// Responsable de ejecutar el plan.
        /// </summary>
        [StringLength(150)]
        [Display(Name = "Responsable")]
        public string Responsable { get; set; }

        /// <summary>
        /// Acción o intervención a realizar.
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Intervención a Realizar")]
        public string IntervencionRealizar { get; set; }

        /// <summary>
        /// Fecha planeada de ejecución.
        /// </summary>
        [Display(Name = "Fecha Planeada")]
        [DataType(DataType.Date)]
        public DateTime? FechaPlaneada { get; set; }

        /// <summary>
        /// Fecha real de ejecución.
        /// </summary>
        [Display(Name = "Fecha de Ejecución")]
        [DataType(DataType.Date)]
        public DateTime? FechaEjecucion { get; set; }

        /// <summary>
        /// Ruta del archivo del plan de mejora.
        /// </summary>
        [StringLength(500)]
        public string RutaArchivoPlan { get; set; }

        /// <summary>
        /// Nombre del archivo del plan de mejora.
        /// </summary>
        [StringLength(255)]
        public string NombreArchivoPlan { get; set; }

        #endregion


        #region [6] Observaciones finales

        /// <summary>
        /// Observaciones finales del proceso.
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Observaciones Finales")]
        public string ObservacionesFinales { get; set; }

        #endregion


        #region [7] Relaciones

        [Required(ErrorMessage = "El proceso es obligatorio.")]
        [Display(Name = "Proceso")]
        public int IdProceso { get; set; }

        [Required(ErrorMessage = "El subproceso es obligatorio.")]
        [Display(Name = "Subproceso")]
        public int IdSubproceso { get; set; }

        /// <summary>
        /// Relación con el proceso organizacional.
        /// </summary>
        [ForeignKey("IdProceso")]
        public virtual Proceso Proceso { get; set; }

        /// <summary>
        /// Relación con el subproceso organizacional.
        /// </summary>
        [ForeignKey("IdSubproceso")]
        public virtual Subproceso Subproceso { get; set; }

        #endregion


        #region [8] Auditoría

        /// <summary>
        /// Fecha de creación del registro.
        /// </summary>
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última modificación.
        /// </summary>
        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        #endregion
    }
}