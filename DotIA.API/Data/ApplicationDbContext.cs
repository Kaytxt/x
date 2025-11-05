using Microsoft.EntityFrameworkCore;
using TabelasDoBanco;

namespace DotIA.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Solicitante> Solicitantes { get; set; }
        public DbSet<Tecnico> Tecnicos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Subcategoria> Subcategorias { get; set; }
        public DbSet<NivelAtendimento> NiveisAtendimento { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<HistoricoUtil> HistoricoUtil { get; set; }
        public DbSet<ChatHistorico> ChatsHistorico { get; set; }
        public DbSet<AvaliacaoResposta> AvaliacoesResposta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Departamento>().ToTable("departamentos");
            modelBuilder.Entity<Solicitante>().ToTable("solicitantes");
            modelBuilder.Entity<Tecnico>().ToTable("tecnicos");
            modelBuilder.Entity<Categoria>().ToTable("categorias");
            modelBuilder.Entity<Subcategoria>().ToTable("subcategorias");
            modelBuilder.Entity<NivelAtendimento>().ToTable("niveis_atendimento");
            modelBuilder.Entity<Ticket>().ToTable("tickets");
            modelBuilder.Entity<HistoricoUtil>().ToTable("historico_util");
            modelBuilder.Entity<ChatHistorico>().ToTable("chat_historico");
            modelBuilder.Entity<AvaliacaoResposta>().ToTable("avaliacao_resposta");
        }
    }
}