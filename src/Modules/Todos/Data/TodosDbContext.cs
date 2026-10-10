using Base.Modules.Todos.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.Modules.Todos.Data;

/// <summary>
/// The Todos module's own DbContext. Its tables live in the "todos" schema, so each module's data
/// stays separate inside one PostgreSQL database and can be moved to its own database later.
/// </summary>
public sealed class TodosDbContext(DbContextOptions<TodosDbContext> options) : DbContext(options)
{
    public const string Schema = "todos";

    public DbSet<TodoItem> Todos => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<TodoItem>(todo =>
        {
            todo.ToTable("todo_items");
            todo.HasKey(t => t.Id);
            todo.Property(t => t.Id).ValueGeneratedNever();
            todo.Property(t => t.Title).HasMaxLength(TodoItem.MaxTitleLength).IsRequired();
            todo.HasIndex(t => t.CreatedAt);
        });
    }
}
