using BarberShopAgenda.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarberShopAgenda.Infrastructure.Data;

public class BarberShopContext : DbContext
{
    public BarberShopContext(DbContextOptions<BarberShopContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Barbeiro> Barbeiros => Set<Barbeiro>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasIndex(c => c.Email);

            entity.HasOne(c => c.Usuario)
                .WithOne(u => u.Cliente)
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Barbeiro>(entity =>
        {
            entity.ToTable("Barbeiros");

            entity.HasOne(b => b.Usuario)
                .WithOne(u => u.Barbeiro)
                .HasForeignKey<Barbeiro>(b => b.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Servico>(entity =>
        {
            entity.ToTable("Servicos");
        });

        modelBuilder.Entity<Agendamento>(entity =>
        {
            entity.ToTable("Agendamentos");
            entity.HasIndex(a => a.DataHora);
            entity.HasIndex(a => a.BarbeiroId);

            entity.HasOne(a => a.Cliente)
                .WithMany(c => c.Agendamentos)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Barbeiro)
                .WithMany(b => b.Agendamentos)
                .HasForeignKey(a => a.BarbeiroId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Servico)
                .WithMany(s => s.Agendamentos)
                .HasForeignKey(a => a.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        var horarioInicioManha = new TimeOnly(9, 0);
        var horarioFimManha = new TimeOnly(12, 0);
        var horarioInicioTarde = new TimeOnly(13, 0);
        var horarioFimTarde = new TimeOnly(19, 0);

        modelBuilder.Entity<Barbeiro>().HasData(
            new Barbeiro
            {
                Id = 1, Nome = "Carlos Silva", Especialidade = "Cortes clássicos", Ativo = true, UsuarioId = 2,
                HorarioInicioManha = horarioInicioManha, HorarioFimManha = horarioFimManha,
                HorarioInicioTarde = horarioInicioTarde, HorarioFimTarde = horarioFimTarde, DiasTrabalho = 63
            },
            new Barbeiro
            {
                Id = 2, Nome = "João Pereira", Especialidade = "Barba e navalha", Ativo = true, UsuarioId = 3,
                HorarioInicioManha = horarioInicioManha, HorarioFimManha = horarioFimManha,
                HorarioInicioTarde = horarioInicioTarde, HorarioFimTarde = horarioFimTarde, DiasTrabalho = 63
            },
            new Barbeiro
            {
                Id = 3, Nome = "Marcos Souza", Especialidade = "Cortes modernos e degradê", Ativo = true, UsuarioId = 4,
                HorarioInicioManha = horarioInicioManha, HorarioFimManha = horarioFimManha,
                HorarioInicioTarde = horarioInicioTarde, HorarioFimTarde = horarioFimTarde, DiasTrabalho = 63
            }
        );

        var dataCadastroSeed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1, Nome = "Administrador", Email = "admin@barbershop.com", Papel = PapelUsuario.Admin, Ativo = true,
                DataCadastro = dataCadastroSeed,
                SenhaHash = "AQAAAAIAAYagAAAAEALS4Lb5vWiYPmmQFgUKKs5kmYjZALMI7i4meu9fPlAxq15d8thqwG9Ns75FRbzA4g=="
            },
            new Usuario
            {
                Id = 2, Nome = "Carlos Silva", Email = "carlos.silva@barbershop.com", Papel = PapelUsuario.Barbeiro, Ativo = true,
                DataCadastro = dataCadastroSeed,
                SenhaHash = "AQAAAAIAAYagAAAAEB/8Wz4FFhsxpPNegqz3iqcE1lO4G46znRK6GQ7gGZY0J2WdKaeuHZlGUgu4GhLMTQ=="
            },
            new Usuario
            {
                Id = 3, Nome = "João Pereira", Email = "joao.pereira@barbershop.com", Papel = PapelUsuario.Barbeiro, Ativo = true,
                DataCadastro = dataCadastroSeed,
                SenhaHash = "AQAAAAIAAYagAAAAEBy7KwJ9F4W7DZzBXQle0B0NrCquufmoJIbVSNJ30VQBaMVTzhYjtGkHZ05KR1r19w=="
            },
            new Usuario
            {
                Id = 4, Nome = "Marcos Souza", Email = "marcos.souza@barbershop.com", Papel = PapelUsuario.Barbeiro, Ativo = true,
                DataCadastro = dataCadastroSeed,
                SenhaHash = "AQAAAAIAAYagAAAAEFeTriNcRDe10YZZpLPfi+s+UfCm1EwRMEbHlX355or+Y+IESPd6txvMMI7ajmtkqQ=="
            }
        );

        modelBuilder.Entity<Servico>().HasData(
            new Servico { Id = 1, Nome = "Corte de Cabelo", Descricao = "Corte tradicional masculino", Preco = 40.00m, DuracaoMinutos = 30 },
            new Servico { Id = 2, Nome = "Barba", Descricao = "Aparar e desenhar barba", Preco = 30.00m, DuracaoMinutos = 20 },
            new Servico { Id = 3, Nome = "Corte + Barba", Descricao = "Combo corte de cabelo e barba", Preco = 60.00m, DuracaoMinutos = 50 },
            new Servico { Id = 4, Nome = "Sobrancelha", Descricao = "Design de sobrancelha na navalha", Preco = 15.00m, DuracaoMinutos = 15 },
            new Servico { Id = 5, Nome = "Pigmentação de Barba", Descricao = "Pigmentação para uniformizar a barba", Preco = 45.00m, DuracaoMinutos = 40 }
        );
    }
}
