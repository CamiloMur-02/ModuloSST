using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models.HojaVida
{
    [Table("Hojavida_datoslaborales")]
    public class HvDatosLaborales
    {
        [Key]
        public int oid { get; set; }
        public int? oidfuncionario { get; set; }
        public int? sindicato { get; set; }
        public DateTime? fechaingreso { get; set; }
        public int? oidproceso { get; set; }
    }
}