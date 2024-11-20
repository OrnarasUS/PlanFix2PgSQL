using Microsoft.EntityFrameworkCore;

namespace PlanFix2PgSQL;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Task> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        const string connStringTemplate = "Username={0};Password={1};Host={2};Port={3};Database={4};";
        var connString = string.Format(connStringTemplate, Program.Arguments.UserName, Program.Arguments.Password,
            Program.Arguments.Host, Program.Arguments.Port, Program.Arguments.DataBase);
        optionsBuilder.UseNpgsql(connString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tasks_pkey");

            entity.ToTable("tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accepted).HasColumnName("accepted");
            entity.Property(e => e.Billsended).HasColumnName("billsended");
            entity.Property(e => e.Created).HasColumnName("created");
            entity.Property(e => e.Ended).HasColumnName("ended");
            entity.Property(e => e.Executors).HasColumnName("executors");
            entity.Property(e => e.Mark).HasColumnName("mark");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Owner).HasColumnName("owner");
            entity.Property(e => e.Partner).HasColumnName("partner");
            entity.Property(e => e.Planended).HasColumnName("planended");
            entity.Property(e => e.Planendedonly).HasColumnName("planendedonly");
            entity.Property(e => e.Retailended).HasColumnName("retailended");
            entity.Property(e => e.Service).HasColumnName("service");
            entity.Property(e => e.Started).HasColumnName("started");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Template).HasColumnName("template");
        });
    }
}
