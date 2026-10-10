using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Base.Modules.Todos.Contracts;

namespace Base.Api.Tests.Todos;

// Integration tests: the whole API with a real database and cache.
// Start dependencies first with: docker compose up -d postgres valkey
[Trait("Category", "Integration")]
public sealed class TodoApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Ready_health_check_passes_when_dependencies_are_up()
    {
        var response = await _client.GetAsync("/health/ready", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Todo_can_be_created_read_completed_and_deleted()
    {
        var title = $"Integration test {Guid.NewGuid():N}";

        // Create
        var created = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest(title), Ct);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var todo = await created.Content.ReadFromJsonAsync<TodoResponse>(Ct);
        Assert.NotNull(todo);
        Assert.Equal(title, todo.Title);

        // Read one, and see it in the (cached) list
        var fetched = await _client.GetFromJsonAsync<TodoResponse>($"/api/todos/{todo.Id}", Ct);
        Assert.Equal(todo.Id, fetched?.Id);
        var list = await _client.GetFromJsonAsync<TodoResponse[]>("/api/todos", Ct);
        Assert.Contains(list ?? [], t => t.Id == todo.Id);

        // Complete: the cached list must reflect the change
        var patched = await _client.PatchAsJsonAsync($"/api/todos/{todo.Id}", new UpdateTodoRequest(null, true), Ct);
        Assert.Equal(HttpStatusCode.OK, patched.StatusCode);
        var listAfterPatch = await _client.GetFromJsonAsync<TodoResponse[]>("/api/todos", Ct);
        Assert.Contains(listAfterPatch ?? [], t => t.Id == todo.Id && t.IsDone);

        // Delete
        var deleted = await _client.DeleteAsync($"/api/todos/{todo.Id}", Ct);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        var missing = await _client.GetAsync($"/api/todos/{todo.Id}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Creating_a_todo_without_a_title_returns_validation_problem()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest("   "), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("title", out _));
    }

    [Fact]
    public async Task Unknown_todo_returns_not_found()
    {
        var response = await _client.GetAsync($"/api/todos/{Guid.NewGuid()}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
