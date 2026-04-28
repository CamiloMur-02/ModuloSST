using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Entidad: Pausa Activa
    /// 
    /// Representa el registro de una pausa activa realizada a un colaborador
    /// dentro del sistema SST del HSLV.
    /// 
    /// Reglas de negocio:
    /// - Cada registro corresponde a una pausa activa realizada en una fecha específica.
    /// - Debe estar asociada a un proceso y subproceso.
    /// - Se utiliza para seguimiento de actividades de bienestar laboral.
    /// </summary>
    [Table("PausaActiva")]
    public class PausaActiva
    {
        #region [1] Clave primaria

        [Key]
        public int IdPausaActiva { get; set; }

        #endregion


        #region [2] Información general

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
        /// Tiempo de prestación del servicio en meses.
        /// </summary>
        [Required(ErrorMessage = "El tiempo de prestación es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Tiempo de Prestación (meses)")]
        public string TiempoPrestacion { get; set; }

        #endregion


        #region [4] Información de la pausa activa

        /// <summary>
        /// Fecha en la que se realizó la pausa activa.
        /// </summary>
        [Required(ErrorMessage = "La fecha de pausa activa es obligatoria.")]
        [Display(Name = "Fecha de Pausa Activa")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        #endregion


        #region [5] Relaciones (FK)

        /// <summary>
        /// Identificador del proceso asociado.
        /// </summary>
        [Display(Name = "ID Proceso")]
        public int IdProceso { get; set; }

        /// <summary>
        /// Identificador del subproceso asociado.
        /// </summary>
        [Display(Name = "ID Subproceso")]
        public int IdSubproceso { get; set; }

        #endregion


        #region [6] Auditoría

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


        #region [7] Navegación

        /// <summary>
        /// Proceso al que pertenece la pausa activa.
        /// </summary>
        [ForeignKey("IdProceso")]
        public virtual Proceso Proceso { get; set; }

        /// <summary>
        /// Subproceso al que pertenece la pausa activa.
        /// </summary>
        [ForeignKey("IdSubproceso")]
        public virtual Subproceso Subproceso { get; set; }

        #endregion
    }
}