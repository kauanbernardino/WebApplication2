using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Model
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options)
            : base(options) { }

        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Disciplina> Disciplinas { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
