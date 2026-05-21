using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    /// <summary>
    /// Subproceso, servicio o área del hospital.
    /// Pertenece a un Proceso padre y se usa como clasificación
    /// en todos los módulos del sistema SST.
    /// </summary>
    [Table("Subproceso")]
    public class Subproceso
    {
        [Key]
        public int IdSubproceso { get; set; }

        /// <summary>Proceso al que pertenece este subproceso.</summary>
        [Required]
        [Display(Name = "ID Proceso")]
        public int IdProceso { get; set; }

        [Required(ErrorMessage = "El nombre del subproceso es obligatorio.")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        [Display(Name = "Subproceso / Servicio / Área")]
        public string NombreSubproceso { get; set; }

        [Display(Name = "Activo")]
        public bool Estado { get; set; }

        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        /// <summary>Proceso padre al que pertenece este subproceso.</summary>
        [ForeignKey("IdProceso")]
        public virtual Proceso Proceso { get; set; }
    }
}