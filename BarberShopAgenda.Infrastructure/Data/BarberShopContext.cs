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
        var sabadoHorarioFim = new TimeOnly(16, 0);

        modelBuilder.Entity<Barbeiro>().HasData(
            new Barbeiro
            {
                Id = 1, Nome = "Carlos Silva", Especialidade = "Cortes clássicos", Ativo = true, UsuarioId = 2,
                HorarioInicioManha = horarioInicioManha, HorarioFimManha = horarioFimManha,
                HorarioInicioTarde = horarioInicioTarde, HorarioFimTarde = horarioFimTarde, DiasTrabalho = 63,
                SabadoHorarioFim = sabadoHorarioFim
            },
            new Barbeiro
            {
                Id = 2, Nome = "João Pereira", Especialidade = "Barba e navalha", Ativo = true, UsuarioId = 3,
                HorarioInicioManha = horarioInicioManha, HorarioFimManha = horarioFimManha,
                HorarioInicioTarde = horarioInicioTarde, HorarioFimTarde = horarioFimTarde, DiasTrabalho = 63,
                SabadoHorarioFim = sabadoHorarioFim
            },
            new Barbeiro
            {
                Id = 3, Nome = "Marcos Souza", Especialidade = "Cortes modernos e degradê", Ativo = true, UsuarioId = 4,
                HorarioInicioManha = horarioInicioManha, HorarioFimManha = horarioFimManha,
                HorarioInicioTarde = horarioInicioTarde, HorarioFimTarde = horarioFimTarde, DiasTrabalho = 63,
                SabadoHorarioFim = sabadoHorarioFim
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
            new Servico { Id = 1, Nome = "Corte", Descricao = "Corte tradicional masculino", Preco = 40.00m, DuracaoMinutos = 30 },
            new Servico { Id = 2, Nome = "Barba", Descricao = "Aparar e desenhar barba", Preco = 40.00m, DuracaoMinutos = 20 },
            new Servico { Id = 3, Nome = "Corte + Barba", Descricao = "Combo corte de cabelo e barba", Preco = 70.00m, DuracaoMinutos = 50 },
            new Servico { Id = 4, Nome = "Sobrancelha", Descricao = "Design de sobrancelha na navalha", Preco = 10.00m, DuracaoMinutos = 15 },
            new Servico { Id = 5, Nome = "Pigmentação", Descricao = "Pigmentação para barba ou cabelo — valor a partir de R$ 25,00, pode variar conforme o serviço", Preco = 25.00m, DuracaoMinutos = 40 },
            new Servico { Id = 6, Nome = "Corte + Sobrancelha", Descricao = "Combo corte de cabelo e sobrancelha", Preco = 45.00m, DuracaoMinutos = 40 },
            new Servico { Id = 7, Nome = "Pezinho", Descricao = "Acabamento do corte (pezinho)", Preco = 20.00m, DuracaoMinutos = 15 },
            new Servico { Id = 8, Nome = "Barba + Pezinho", Descricao = "Combo barba e acabamento do corte", Preco = 50.00m, DuracaoMinutos = 30 },
            new Servico { Id = 9, Nome = "Luzes", Descricao = "Mechas/luzes no cabelo", Preco = 150.00m, DuracaoMinutos = 120 },
            new Servico { Id = 10, Nome = "Platinado", Descricao = "Descoloração completa do cabelo", Preco = 180.00m, DuracaoMinutos = 150 },
            new Servico { Id = 11, Nome = "Hidratação", Descricao = "Hidratação capilar", Preco = 20.00m, DuracaoMinutos = 30 },
            new Servico { Id = 12, Nome = "Cera Nasal", Descricao = "Remoção de pelos nasais com cera", Preco = 20.00m, DuracaoMinutos = 10 },
            new Servico { Id = 13, Nome = "Relaxamento", Descricao = "Relaxamento capilar", Preco = 25.00m, DuracaoMinutos = 30 }
        );
    }
}
