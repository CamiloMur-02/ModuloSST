using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Entidad: Reporte de Enfermedad Laboral
    /// 
    /// Representa el registro de enfermedades laborales, enfermedades generales
    /// y accidentes de trabajo desde el enfoque médico/diagnóstico.
    /// 
    /// Flujo del proceso:
    /// - Datos del colaborador
    /// - Tipo de enfermedad y diagnóstico
    /// - Incapacidad
    /// - Investigación (solo si es enfermedad laboral)
    /// - Plan de mejora (solo si es enfermedad laboral)
    /// - Observaciones finales
    /// 
    /// Reglas de negocio:
    /// - Solo cuando TipoEnfermedad = "Enfermedad Laboral" se habilitan
    ///   los campos de investigación, seguimiento y plan de mejora.
    /// - Los días de investigación los ingresa el usuario manualmente.
    /// - Los archivos se almacenan en el servidor y se referencian por ruta.
    /// </summary>
    [Table("ReporteEnfermedadLaboral")]
    public class ReporteEnfermedadLaboral
    {
        #region [1] Clave primaria

        [Key]
        public int IdReporteEnfermedadLaboral { get; set; }

        #endregion


        #region [2] Datos del colaborador

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
        [StringLength(20)]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        /// <summary>
        /// Nombre completo del colaborador.
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(200)]
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
        [StringLength(150)]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; }

        /// <summary>
        /// Empresa o contratista asociado.
        /// </summary>
        [Required(ErrorMessage = "El contratista es obligatorio.")]
        [StringLength(150)]
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


        #region [3] Tipo de enfermedad y diagnóstico

        /// <summary>
        /// Tipo de enfermedad registrada.
        /// Valores permitidos:
        /// - Enfermedad Laboral
        /// - Enfermedad General
        /// - Accidente de Trabajo
        /// </summary>
        [Required(ErrorMessage = "Debe seleccionar el tipo de enfermedad.")]
        [StringLength(50)]
        [Display(Name = "Tipo de Enfermedad")]
        public string TipoEnfermedad { get; set; }

        /// <summary>
        /// Código del diagnóstico médico.
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Código de Diagnóstico")]
        public string Codigo { get; set; }

        /// <summary>
        /// Descripción del diagnóstico.
        /// </summary>
        [Required(ErrorMessage = "El detalle del diagnóstico es obligatorio.")]
        [StringLength(300)]
        [Display(Name = "Detalle del Diagnóstico")]
        public string DetallesDiagnostico { get; set; }

        /// <summary>
        /// Fecha en la que se emitió el diagnóstico.
        /// </summary>
        [Required]
        [Display(Name = "Fecha de Diagnóstico")]
        [DataType(DataType.Date)]
        public DateTime FechaDiagnostico { get; set; }

        #endregion


        #region [4] Incapacidad

        /// <summary>
        /// Fecha de inicio de la incapacidad.
        /// </summary>
        [Display(Name = "Fecha Inicio Incapacidad")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicioIncapacidad { get; set; }

        /// <summary>
        /// Fecha de finalización de la incapacidad.
        /// </summary>
        [Display(Name = "Fecha Final Incapacidad")]
        [DataType(DataType.Date)]
        public DateTime? FechaFinalIncapacidad { get; set; }

        /// <summary>
        /// Número de días de incapacidad.
        /// </summary>
        [Display(Name = "Días de Incapacidad")]
        public int DiasIncapacidad { get; set; }

        #endregion


        #region [5] Investigación y seguimiento

        /// <summary>
        /// Fecha en que se realizó la investigación.
        /// Solo aplica para Enfermedad Laboral.
        /// </summary>
        [Display(Name = "Fecha de Investigación")]
        [DataType(DataType.Date)]
        public DateTime? FechaInvestigacion { get; set; }

        /// <summary>
        /// Días entre diagnóstico e investigación.
        /// Lo ingresa manualmente el usuario.
        /// </summary>
        [Display(Name = "Días de Investigación")]
        public int? DiasInvestigacion { get; set; }

        /// <summary>
        /// Causa identificada.
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Causa")]
        public string Causa { get; set; }

        /// <summary>
        /// Ruta del archivo de seguimiento.
        /// </summary>
        [StringLength(500)]
        public string RutaArchivoSeguimiento { get; set; }

        /// <summary>
        /// Nombre del archivo de seguimiento.
        /// </summary>
        [StringLength(255)]
        public string NombreArchivoSeguimiento { get; set; }

        /// <summary>
        /// Ruta del archivo FUREL.
        /// </summary>
        [StringLength(500)]
        public string RutaArchivoFurel { get; set; }

        /// <summary>
        /// Nombre del archivo FUREL.
        /// </summary>
        [StringLength(255)]
        public string NombreArchivoFurel { get; set; }

        #endregion


        #region [6] Plan de mejora

        /// <summary>
        /// Actividad a realizar como acción correctiva.
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Actividad a Realizar")]
        public string ActividadARealizar { get; set; }

        /// <summary>
        /// Responsable de la ejecución.
        /// </summary>
        [StringLength(150)]
        [Display(Name = "Responsable")]
        public string Responsable { get; set; }

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


        #region [7] Observaciones

        /// <summary>
        /// Observaciones del proceso de investigación.
        /// Solo aplica para Enfermedad Laboral.
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; }

        /// <summary>
        /// Observaciones finales del registro.
        /// Aplica para todos los tipos de enfermedad.
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Observaciones Finales")]
        public string ObservacionesFinales { get; set; }

        #endregion


        #region [8] Relaciones

        [Display(Name = "ID Proceso")]
        public int IdProceso { get; set; }

        [Display(Name = "ID Subproceso")]
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


        #region [9] Auditoría

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