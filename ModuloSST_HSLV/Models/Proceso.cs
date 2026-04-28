using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models
{
    [Table("Proceso")]
    public class Proceso
    {
        [Key]
        public int IdProceso { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Nombre del Proceso")]
        public string NombreProceso { get; set; }

        [Display(Name = "Activo")]
        public bool Estado { get; set; }

        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }
    }
}