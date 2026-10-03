namespace PersonService.Contracts;

public class ValidationErrorResponse
{
    public string? Message { get; set; }

    public Dictionary<string, string>? Errors { get; set; }
}