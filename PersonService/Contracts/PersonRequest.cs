namespace PersonService.Contracts;

/// <summary>Запрос на создание (POST) или частичное обновление (PATCH) человека.</summary>
public class PersonRequest
{
    public string? Name { get; set; }

    public int? Age { get; set; }

    public string? Address { get; set; }

    public string? Work { get; set; }
}