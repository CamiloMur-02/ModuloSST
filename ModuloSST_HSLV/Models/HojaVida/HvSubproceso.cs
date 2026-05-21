using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models.HojaVida
{
    [Table("pla_subproceso")]
    public class HvSubproceso
    {
        [Key]
        public int oid { get; set; }
        public string gdsubpronom { get; set; }
        public int? oidproceso { get; set; }
    }
}