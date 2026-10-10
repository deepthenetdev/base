using Base.Modules.Todos.Contracts;
using Base.Modules.Todos.Data;
using Base.Modules.Todos.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Base.Modules.Todos.Endpoints;

/// <summary>
/// REST endpoints for todos. Reads go through HybridCache (in-memory first, then Valkey, then PostgreSQL);
/// every write removes the cached list so readers never see stale data for long.
/// </summary>
public static class TodoEndpoints
{
    internal const string ListCacheKey = "todos:list:v1";
    private const int MaxListSize = 100;

    public static RouteGroupBuilder MapTodoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/todos").WithTags("Todos");

        group.MapGet("/", ListAsync).WithName("ListTodos").WithSummary("List the most recent todos");
        group.MapGet("/{id:guid}", GetAsync).WithName("GetTodo").WithSummary("Get one todo");
        group.MapPost("/", CreateAsync).WithName("CreateTodo").WithSummary("Create a todo");
        group.MapPatch("/{id:guid}", UpdateAsync).WithName("UpdateTodo").WithSummary("Rename a todo or change its status");
        group.MapDelete("/{id:guid}", DeleteAsync).WithName("DeleteTodo").WithSummary("Delete a todo");

        return group;
    }

    private static async Task<Ok<TodoResponse[]>> ListAsync(
        TodosDbContext db, HybridCache cache, CancellationToken cancellationToken)
    {
        var todos = await cache.GetOrCreateAsync(
            ListCacheKey,
            async token => await db.Todos
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Take(MaxListSize)
                .Select(t => new TodoResponse(t.Id, t.Title, t.IsDone, t.CreatedAt, t.CompletedAt))
                .ToArrayAsync(token),
            cancellationToken: cancellationToken);

        return TypedResults.Ok(todos);
    }

    private static async Task<Results<Ok<TodoResponse>, NotFound>> GetAsync(
        Guid id, TodosDbContext db, CancellationToken cancellationToken)
    {
        var todo = await db.Todos.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return todo is null ? TypedResults.NotFound() : TypedResults.Ok(TodoResponse.From(todo));
    }

    private static async Task<Results<Created<TodoResponse>, ValidationProblem>> CreateAsync(
        CreateTodoRequest request, TodosDbContext db, HybridCache cache, TimeProvider clock, CancellationToken cancellationToken)
    {
        var errors = TodoValidation.ValidateTitle(request.Title);
        if (errors is not null)
        {
            return TypedResults.ValidationProblem(errors);
        }

        // Validation passed, so Title is known to be non-empty here.
        var todo = TodoItem.Create(request.Title!, clock.GetUtcNow());
        db.Todos.Add(todo);
        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(ListCacheKey, cancellationToken);

        return TypedResults.Created($"/api/todos/{todo.Id}", TodoResponse.From(todo));
    }

    private static async Task<Results<Ok<TodoResponse>, NotFound, ValidationProblem>> UpdateAsync(
        Guid id, UpdateTodoRequest request, TodosDbContext db, HybridCache cache, TimeProvider clock, CancellationToken cancellationToken)
    {
        if (request.Title is not null)
        {
            var errors = TodoValidation.ValidateTitle(request.Title);
            if (errors is not null)
            {
                return TypedResults.ValidationProblem(errors);
            }
        }

        var todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (todo is null)
        {
            return TypedResults.NotFound();
        }

        if (request.Title is not null)
        {
            todo.Rename(request.Title);
        }

        if (request.IsDone == true)
        {
            todo.MarkDone(clock.GetUtcNow());
        }
        else if (request.IsDone == false)
        {
            todo.Reopen();
        }

        await db.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(ListCacheKey, cancellationToken);

        return TypedResults.Ok(TodoResponse.From(todo));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        Guid id, TodosDbContext db, HybridCache cache, CancellationToken cancellationToken)
    {
        var deleted = await db.Todos.Where(t => t.Id == id).ExecuteDeleteAsync(cancellationToken);
        if (deleted == 0)
        {
            return TypedResults.NotFound();
        }

        await cache.RemoveAsync(ListCacheKey, cancellationToken);
        return TypedResults.NoContent();
    }
}
