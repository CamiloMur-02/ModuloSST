using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Entidad: Inspección de Bioseguridad
    /// 
    /// Representa una evaluación de cumplimiento de protocolos de bioseguridad
    /// aplicada a colaboradores del HSLV.
    /// 
    /// Reglas de negocio:
    /// - Cada ítem se evalúa como: "C" (Cumple), "NC" (No Cumple) o "NA" (No Aplica).
    /// - Los totales (Cumple / No Cumple / No Aplica) se calculan en el controlador.
    /// - Debe estar asociada a un Proceso y Subproceso.
    /// </summary>
    [Table("InspeccionBioseguridad")]
    public class InspeccionBioseguridad
    {
        #region [1] Clave primaria

        [Key]
        public int IdInspeccionBioseguridad { get; set; }

        #endregion


        #region [2] Relaciones (FK)

        /// <summary>
        /// Identificador del proceso asociado a la inspección.
        /// </summary>
        [Display(Name = "ID Proceso")]
        public int IdProceso { get; set; }

        /// <summary>
        /// Identificador del subproceso asociado a la inspección.
        /// </summary>
        [Display(Name = "ID Subproceso")]
        public int IdSubproceso { get; set; }

        /// <summary>
        /// Identificador del empleado (integración futura con hoja de vida).
        /// </summary>
        [Display(Name = "ID Empleado")]
        public int? IdEmpleado { get; set; }

        #endregion


        #region [3] Datos del colaborador

        /// <summary>
        /// Número de identificación del colaborador.
        /// </summary>
        [Required(ErrorMessage = "La cédula es obligatoria.")]
        [StringLength(20)]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        /// <summary>
        /// Nombre completo del colaborador inspeccionado.
        /// </summary>
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(200)]
        [Display(Name = "Apellidos y Nombres")]
        public string NombreCompleto { get; set; }

        /// <summary>
        /// Cargo desempeñado por el colaborador.
        /// </summary>
        [Required(ErrorMessage = "El cargo es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; }

        /// <summary>
        /// Empresa o contratista al que pertenece.
        /// </summary>
        [Required(ErrorMessage = "El contratista es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Contratista")]
        public string Contratista { get; set; }

        /// <summary>
        /// Edad del colaborador.
        /// </summary>
        [Display(Name = "Edad")]
        public int Edad { get; set; }

        /// <summary>
        /// Género del colaborador.
        /// </summary>
        [Required(ErrorMessage = "El género es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Género")]
        public string Genero { get; set; }

        /// <summary>
        /// Tiempo de prestación del servicio en meses.
        /// </summary>
        //[Required(ErrorMessage = "El tiempo de prestación es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Tiempo de Prestación (meses)")]
        public string TiempoPrestacion { get; set; }

        #endregion


        #region [4] Información de la inspección

        /// <summary>
        /// Fecha en la que se realizó la inspección.
        /// </summary>
        [Required(ErrorMessage = "La fecha de inspección es obligatoria.")]
        [Display(Name = "Fecha de Inspección")]
        [DataType(DataType.Date)]
        public DateTime FechaInspeccion { get; set; }

        #endregion


        #region [5] Ítems de evaluación

        /// <summary>Evaluación de higiene de manos.</summary>
        [Required(ErrorMessage = "Higiene de Manos es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Higiene de Manos")]
        public string HigieneManos { get; set; }

        /// <summary>Evaluación de protección respiratoria.</summary>
        [Required(ErrorMessage = "Protección Respiratoria es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Protección Respiratoria")]
        public string ProteccionRespiratoria { get; set; }

        /// <summary>Evaluación del uso de guantes.</summary>
        [Required(ErrorMessage = "Uso de Guantes es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Uso de Guantes")]
        public string UsoGuantes { get; set; }

        /// <summary>Evaluación del uso de gorro.</summary>
        [Required(ErrorMessage = "Uso de Gorro es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Uso de Gorro")]
        public string UsoGorro { get; set; }

        /// <summary>Evaluación del uso de bata.</summary>
        [Required(ErrorMessage = "Uso de Bata es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Uso de Bata")]
        public string UsoBata { get; set; }

        /// <summary>Evaluación de protección visual.</summary>
        [Required(ErrorMessage = "Protección Visual es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Protección Visual")]
        public string ProteccionVisual { get; set; }

        /// <summary>Evaluación del uso de delantal o peto.</summary>
        [Required(ErrorMessage = "Uso Delantal/Peto es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Uso Delantal/Peto")]
        public string UsoDelantalPeto { get; set; }

        /// <summary>Evaluación del calzado adecuado.</summary>
        [Required(ErrorMessage = "Calzado Adecuado es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Calzado Adecuado")]
        public string CalzadoAdecuado { get; set; }

        /// <summary>Evaluación del transporte de muestras.</summary>
        [Required(ErrorMessage = "Transporte de Muestras es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Transporte de Muestras")]
        public string TransporteMuestras { get; set; }

        /// <summary>Evaluación del manejo de corto punzantes.</summary>
        [Required(ErrorMessage = "Manejo Corto Punzantes es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Manejo Corto Punzantes")]
        public string ManejoCorto { get; set; }

        /// <summary>Evaluación del manejo de residuos.</summary>
        [Required(ErrorMessage = "Manejo de Residuos es obligatorio.")]
        [StringLength(2)]
        [Display(Name = "Manejo de Residuos")]
        public string ManejoResiduos { get; set; }

        #endregion


        #region [6] Totales calculados

        /// <summary>
        /// Total de ítems que cumplen (calculado en el controlador).
        /// </summary>
        [Display(Name = "Total Cumple")]
        public int TotalCumple { get; set; }

        /// <summary>
        /// Total de ítems que no cumplen (calculado en el controlador).
        /// </summary>
        [Display(Name = "Total No Cumple")]
        public int TotalNoCumple { get; set; }

        /// <summary>
        /// Total de ítems no aplicables (calculado en el controlador).
        /// </summary>
        [Display(Name = "Total No Aplica")]
        public int TotalNoAplica { get; set; }

        #endregion


        #region [7] Observaciones

        /// <summary>
        /// Observaciones finales de la inspección.
        /// </summary>
        [Display(Name = "Observaciones Finales")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        public string ObservacionesFinales { get; set; }

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


        #region [9] Navegación

        /// <summary>
        /// Proceso al que pertenece la inspección.
        /// </summary>
        [ForeignKey("IdProceso")]
        public virtual Proceso Proceso { get; set; }

        /// <summary>
        /// Subproceso al que pertenece la inspección.
        /// </summary>
        [ForeignKey("IdSubproceso")]
        public virtual Subproceso Subproceso { get; set; }

        #endregion
    }
}