using Base.Modules.Todos.Domain;

namespace Base.Api.Tests.Todos;

// Unit tests: no database, no web server. They run in milliseconds.
public sealed class TodoItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 15, 9, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Create_trims_the_title_and_starts_open()
    {
        var todo = TodoItem.Create("  Ship the base repo  ", Now);

        Assert.Equal("Ship the base repo", todo.Title);
        Assert.False(todo.IsDone);
        Assert.Null(todo.CompletedAt);
        Assert.Equal(Now, todo.CreatedAt);
        Assert.NotEqual(Guid.Empty, todo.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_an_empty_title(string title)
    {
        Assert.Throws<ArgumentException>(() => TodoItem.Create(title, Now));
    }

    [Fact]
    public void MarkDone_records_when_it_was_completed_and_is_idempotent()
    {
        var todo = TodoItem.Create("Write tests", Now);
        var completedAt = Now.AddHours(1);

        todo.MarkDone(completedAt);
        todo.MarkDone(completedAt.AddHours(1));

        Assert.True(todo.IsDone);
        Assert.Equal(completedAt, todo.CompletedAt);
    }

    [Fact]
    public void Reopen_clears_completion()
    {
        var todo = TodoItem.Create("Write tests", Now);
        todo.MarkDone(Now.AddHours(1));

        todo.Reopen();

        Assert.False(todo.IsDone);
        Assert.Null(todo.CompletedAt);
    }

    [Fact]
    public void Validation_rejects_titles_that_are_too_long()
    {
        var errors = TodoValidation.ValidateTitle(new string('a', TodoItem.MaxTitleLength + 1));

        Assert.NotNull(errors);
        Assert.Contains(TodoValidation.TitleField, errors.Keys);
    }

    [Fact]
    public void Validation_accepts_a_normal_title()
    {
        Assert.Null(TodoValidation.ValidateTitle("Buy milk"));
    }
}
