using Base.Modules.Todos.Domain;

namespace Base.Modules.Todos.Contracts;

// Request and response shapes for the HTTP API. They are separate from the entity on purpose:
// the database model can change without breaking clients, and clients can't set fields they shouldn't.

public sealed record TodoResponse(Guid Id, string Title, bool IsDone, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt)
{
    public static TodoResponse From(TodoItem todo)
    {
        ArgumentNullException.ThrowIfNull(todo);
        return new(todo.Id, todo.Title, todo.IsDone, todo.CreatedAt, todo.CompletedAt);
    }
}

public sealed record CreateTodoRequest(string? Title);

/// <summary>Partial update: fields left out (null) are not changed.</summary>
public sealed record UpdateTodoRequest(string? Title, bool? IsDone);
