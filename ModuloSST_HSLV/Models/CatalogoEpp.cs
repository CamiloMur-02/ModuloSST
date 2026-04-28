using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Entidad: Catálogo de EPP
    /// 
    /// Representa el catálogo maestro de Elementos de Protección Personal (EPP)
    /// utilizados en el HSLV.
    /// 
    /// Reglas de negocio:
    /// - Solo los EPP con Estado = true están disponibles para selección
    ///   en la Matriz de Entrega de EPP.
    /// - El nombre debe ser único a nivel funcional (validado en aplicación/BD).
    /// </summary>
    [Table("CatalogoEPP")]
    public class CatalogoEpp
    {
        #region [1] Clave Primaria

        /// <summary>
        /// Identificador único del tipo de EPP.
        /// </summary>
        [Key]
        public int IdCatalogoEPP { get; set; }

        #endregion

        #region [2] Campos Principales

        /// <summary>
        /// Nombre del EPP (ej: Casco, Guantes, Gafas de seguridad).
        /// Campo obligatorio.
        /// </summary>
        [Required(ErrorMessage = "El nombre del EPP es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Nombre EPP")]
        public string NombreEPP { get; set; }

        /// <summary>
        /// Indica si el EPP está activo o inactivo.
        /// - true  = Disponible para uso
        /// - false = No disponible (no se elimina físicamente)
        /// </summary>
        [Display(Name = "Activo")]
        public bool Estado { get; set; }

        #endregion

        #region [3] Campos de Auditoría

        /// <summary>
        /// Fecha en que se creó el registro.
        /// </summary>
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de la última modificación del registro.
        /// Puede ser nula si nunca ha sido editado.
        /// </summary>
        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        #endregion

        #region [4] Relaciones

        // (Este modelo no define relaciones de navegación directas,
        // pero es referenciado por ElementoEpp en la MatrizEPP)

        #endregion
    }
}