using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Model
{
    public class Disciplina
    {
        [Key]
        public int id { get; set; }
        [StringLength(35)]
        public String descricao { get; set; }
    }
}
