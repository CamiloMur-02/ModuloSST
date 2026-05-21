using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models.HojaVida
{
    [Table("Hojavida_cargo")]
    public class HvCargo
    {
        [Key]
        public int oid { get; set; }
        public string cargo { get; set; }
    }
}