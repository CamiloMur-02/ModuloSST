using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models.HojaVida
{
    [Table("Hojavida_sindicato")]
    public class HvSindicato
    {
        [Key]
        public int oid { get; set; }
        public string nombre { get; set; }
    }
}