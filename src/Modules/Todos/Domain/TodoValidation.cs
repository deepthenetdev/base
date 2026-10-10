namespace Base.Modules.Todos.Domain;

/// <summary>
/// Input rules shared by the endpoints (which return 400 with details) and the entity (which refuses bad state).
/// </summary>
public static class TodoValidation
{
    public const string TitleField = "title";

    /// <returns>Null when valid; otherwise errors keyed by field name, ready for a ValidationProblem response.</returns>
    public static Dictionary<string, string[]>? ValidateTitle(string? title)
    {
        var trimmed = title?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return Error("Title is required.");
        }

        if (trimmed.Length > TodoItem.MaxTitleLength)
        {
            return Error($"Title must be {TodoItem.MaxTitleLength} characters or fewer.");
        }

        return null;
    }

    private static Dictionary<string, string[]> Error(string message) =>
        new(StringComparer.Ordinal) { [TitleField] = [message] };
}
