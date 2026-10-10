namespace Base.Modules.Todos.Domain;

/// <summary>
/// The Todo entity. State changes go through methods so the rules live in one place;
/// the current time is passed in so the rules are easy to test.
/// </summary>
public sealed class TodoItem
{
    public const int MaxTitleLength = 200;

    // Used by EF Core when loading from the database.
    private TodoItem()
    {
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public bool IsDone { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static TodoItem Create(string title, DateTimeOffset now) => new()
    {
        // Version 7 GUIDs are time-ordered, which keeps database indexes compact.
        Id = Guid.CreateVersion7(now),
        Title = NormalizeTitle(title),
        CreatedAt = now,
    };

    public void Rename(string title) => Title = NormalizeTitle(title);

    public void MarkDone(DateTimeOffset now)
    {
        if (IsDone)
        {
            return;
        }

        IsDone = true;
        CompletedAt = now;
    }

    public void Reopen()
    {
        IsDone = false;
        CompletedAt = null;
    }

    private static string NormalizeTitle(string title)
    {
        var errors = TodoValidation.ValidateTitle(title);
        if (errors is not null)
        {
            throw new ArgumentException(errors[TodoValidation.TitleField][0], nameof(title));
        }

        return title.Trim();
    }
}
