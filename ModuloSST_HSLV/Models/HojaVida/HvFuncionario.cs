using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuloSST_HSLV.Models.HojaVida
{
    [Table("Hojavida_funcionario")]
    public class HvFuncionario
    {
        [Key]
        public int oid { get; set; }
        public string iden { get; set; }
        public string primernombre { get; set; }
        public string segundonombre { get; set; }
        public string primerapellido { get; set; }
        public string segundoapellido { get; set; }
        public int? sexo { get; set; }
        public DateTime? fechanacimiento { get; set; }
        public int? oidcargo { get; set; }
    }
}