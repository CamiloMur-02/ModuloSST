using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models.HojaVida
{
    [Table("Hojavida_proceso")]
    public class HvProceso
    {
        [Key]
        public int oid { get; set; }
        public string proceso { get; set; }
    }
}