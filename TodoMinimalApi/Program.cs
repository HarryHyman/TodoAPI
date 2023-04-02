using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // Creates a WebApplicationBuilder
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList")); // Adds a TodoContext to the DI container
builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Adds a filter that enables the developer exception page

var app = builder.Build(); // Creates a WebApplication

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", async (TodoDb db) => 
    await db.Todos.ToListAsync()); // Adds a route to the app

todoItems.MapGet("/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync()); // Adds a route to the app

todoItems.MapGet("/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id) 
        is Todo todo 
            ? Results.Ok(todo) 
            : Results.NotFound()); // Adds a route to the app

todoItems.MapPost("/", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    
    return Results.Created($"/todoitems/{todo.Id}", todo);
}); // Adds a route to the app

todoItems.MapPut("/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

todoItems.MapDelete("/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NoContent();
});

// GET / - returns hello world
app.MapGet("/", () => "Hello World!");

app.Run();
