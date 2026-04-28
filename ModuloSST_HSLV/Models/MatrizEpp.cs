using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Entidad: Matriz de Entrega de EPP
    /// 
    /// Representa el registro de entrega de Elementos de Protección Personal (EPP)
    /// a un colaborador del HSLV.
    /// 
    /// Reglas de negocio:
    /// - Una matriz puede contener múltiples elementos EPP (relación 1:N).
    /// - Cada elemento tiene su propio tiempo de alerta y fecha de entrega.
    /// - Los elementos activos son los que se consideran vigentes.
    /// - El cálculo del tiempo restante se realiza en el controlador (no en BD).
    /// </summary>
    [Table("MatrizEPP")]
    public class MatrizEpp
    {
        #region [1] Clave primaria

        [Key]
        public int IdMatrizEPP { get; set; }

        #endregion


        #region [2] Información general

        /// <summary>
        /// Fecha en la que se registra la entrega de EPP.
        /// </summary>
        [Required]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }

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
        /// Tiempo de prestación del servicio en meses.
        /// </summary>
        [Required(ErrorMessage = "El tiempo de prestación es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Tiempo de Prestación (meses)")]
        public string TiempoPrestacion { get; set; }

        #endregion


        #region [4] Observaciones

        /// <summary>
        /// Observaciones finales sobre la entrega de EPP.
        /// </summary>
        [Display(Name = "Observaciones Finales")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        public string ObservacionesFinales { get; set; }

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
        /// Proceso al que pertenece la matriz.
        /// </summary>
        [ForeignKey("IdProceso")]
        public virtual Proceso Proceso { get; set; }

        /// <summary>
        /// Subproceso al que pertenece la matriz.
        /// </summary>
        [ForeignKey("IdSubproceso")]
        public virtual Subproceso Subproceso { get; set; }

        /// <summary>
        /// Lista de elementos EPP entregados al colaborador.
        /// Relación uno a muchos (1:N).
        /// </summary>
        public virtual ICollection<ElementoEpp> Elementos { get; set; }

        #endregion
    }
}