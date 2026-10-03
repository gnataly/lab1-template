using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonService.Contracts;
using PersonService.Data;
using PersonService.Models;

namespace PersonService.Controllers;

[ApiController]
[Route("api/v1/persons")]
public class PersonsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PersonsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonResponse>>> GetAll()
    {
        var persons = await _db.Persons.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        return Ok(persons.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PersonResponse>> GetById(int id)
    {
        var person = await _db.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (person is null)
        {
            return NotFound(new ErrorResponse { Message = $"Person with id {id} not found" });
        }

        return Ok(ToResponse(person));
    }

    [HttpPost]
    public async Task<ActionResult<PersonResponse>> Create([FromBody] PersonRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ValidationErrorResponse
            {
                Message = "Validation failed",
                Errors = new Dictionary<string, string> { ["name"] = "Name is required" }
            });
        }

        var person = new Person
        {
            Name = request.Name,
            Age = request.Age,
            Address = request.Address,
            Work = request.Work
        };

        _db.Persons.Add(person);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = person.Id }, value: null);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<PersonResponse>> Update(int id, [FromBody] PersonRequest request)
    {
        var person = await _db.Persons.FirstOrDefaultAsync(p => p.Id == id);
        if (person is null)
        {
            return NotFound(new ErrorResponse { Message = $"Person with id {id} not found" });
        }

        if (request.Name is not null)
            person.Name = request.Name;
        if (request.Age is not null)
            person.Age = request.Age;
        if (request.Address is not null)
            person.Address = request.Address;
        if (request.Work is not null)
            person.Work = request.Work;

        await _db.SaveChangesAsync();

        return Ok(ToResponse(person));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var person = await _db.Persons.FirstOrDefaultAsync(p => p.Id == id);
        if (person is null)
        {
            return NotFound(new ErrorResponse { Message = $"Person with id {id} not found" });
        }

        _db.Persons.Remove(person);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static PersonResponse ToResponse(Person person) => new()
    {
        Id = person.Id,
        Name = person.Name,
        Age = person.Age,
        Address = person.Address,
        Work = person.Work
    };
}