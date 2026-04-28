using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Entidad: Elemento de EPP
    /// 
    /// Representa un elemento individual de EPP entregado a un colaborador
    /// dentro de una Matriz de EPP.
    /// 
    /// Reglas de negocio:
    /// - Cada elemento tiene su propia fecha de entrega y tiempo de alerta.
    /// - TiempoRestante es un dato derivado.
    /// - El campo Activo permite exclusión lógica (no se elimina físicamente).
    /// </summary>
    [Table("ElementoEPP")]
    public class ElementoEpp
    {
        #region [1] Clave Primaria

        /// <summary>
        /// Identificador único del elemento de EPP.
        /// </summary>
        [Key]
        public int IdElementoEPP { get; set; }

        #endregion

        #region [2] Campos Principales

        /// <summary>
        /// Identificador de la matriz a la que pertenece el elemento.
        /// </summary>
        [Required]
        [Display(Name = "ID Matriz EPP")]
        public int IdMatrizEPP { get; set; }

        /// <summary>
        /// Identificador del tipo de EPP en el catálogo.
        /// </summary>
        [Required]
        [Display(Name = "ID Catálogo EPP")]
        public int IdCatalogoEPP { get; set; }

        /// <summary>
        /// Fecha en que se realizó la entrega del EPP.
        /// Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "La fecha de entrega es obligatoria.")]
        [Display(Name = "Fecha de Entrega")]
        [DataType(DataType.Date)]
        public DateTime? FechaEntrega { get; set; }

        /// <summary>
        /// Tiempo restante en meses antes del reemplazo del EPP.
        /// Fórmula: TiempoAlerta - (días transcurridos / 30).
        /// Se recalcula en el controlador.
        /// </summary>
        [Display(Name = "Tiempo Restante (meses)")]
        public decimal TiempoRestante { get; set; }

        /// <summary>
        /// Tiempo estimado de vida útil del EPP en meses.
        /// Valor típico: 12.
        /// </summary>
        [Display(Name = "Tiempo de Alerta (meses)")]
        public int TiempoAlerta { get; set; }

        /// <summary>
        /// Indica si el elemento está activo.
        /// false = excluido lógicamente.
        /// </summary>
        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        #endregion

        #region [3] Auditoría

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

        #region [4] Relaciones (FK)

        /// <summary>
        /// Relación con la matriz de EPP.
        /// </summary>
        [ForeignKey("IdMatrizEPP")]
        public virtual MatrizEpp MatrizEpp { get; set; }

        /// <summary>
        /// Relación con el catálogo de EPP.
        /// </summary>
        [ForeignKey("IdCatalogoEPP")]
        public virtual CatalogoEpp CatalogoEpp { get; set; }

        #endregion
    }
}