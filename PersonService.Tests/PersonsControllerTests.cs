using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonService.Contracts;
using PersonService.Controllers;
using PersonService.Data;
using PersonService.Models;

namespace PersonService.Tests;

public class PersonsControllerTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_Returns201WithLocationHeader()
    {
        using var db = CreateContext();
        var controller = new PersonsController(db);

        var result = await controller.Create(new PersonRequest
        {
            Name = "Alice",
            Age = 30,
            Address = "Moscow",
            Work = "T-Bank"
        });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        var routeId = Assert.IsType<int>(created.RouteValues!["id"]);
        Assert.True(routeId > 0);
        Assert.Equal(1, await db.Persons.CountAsync());
    }

    [Fact]
    public async Task GetAll_ReturnsAllPersons()
    {
        using var db = CreateContext();
        db.Persons.AddRange(
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 });
        await db.SaveChangesAsync();
        var controller = new PersonsController(db);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<PersonResponse>>(ok.Value);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsPerson()
    {
        using var db = CreateContext();
        var person = new Person { Name = "Alice", Age = 30, Address = "Moscow", Work = "T-Bank" };
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        var controller = new PersonsController(db);

        var result = await controller.GetById(person.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PersonResponse>(ok.Value);
        Assert.Equal(person.Id, response.Id);
        Assert.Equal("Alice", response.Name);
        Assert.Equal(30, response.Age);
        Assert.Equal("Moscow", response.Address);
        Assert.Equal("T-Bank", response.Work);
    }

    [Fact]
    public async Task GetById_WhenNotExists_Returns404()
    {
        using var db = CreateContext();
        var controller = new PersonsController(db);

        var result = await controller.GetById(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
        var error = Assert.IsType<ErrorResponse>(notFound.Value);
        Assert.NotNull(error.Message);
    }

    [Fact]
    public async Task Update_PartiallyUpdatesOnlyProvidedFields()
    {
        using var db = CreateContext();
        var person = new Person { Name = "Alice", Age = 30, Address = "Moscow", Work = "T-Bank" };
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        var controller = new PersonsController(db);

        var result = await controller.Update(
            person.Id,
            new PersonRequest { Name = "Alice2", Address = "SPb" });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PersonResponse>(ok.Value);
        Assert.Equal("Alice2", response.Name);
        Assert.Equal("SPb", response.Address);
        Assert.Equal(30, response.Age);      // не передан в PATCH → не изменён
        Assert.Equal("T-Bank", response.Work); // не передан в PATCH → не изменён
    }

    [Fact]
    public async Task Delete_Returns204AndRemovesPerson()
    {
        using var db = CreateContext();
        var person = new Person { Name = "Alice", Age = 30 };
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        var controller = new PersonsController(db);

        var result = await controller.Delete(person.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await db.Persons.CountAsync());
    }
}