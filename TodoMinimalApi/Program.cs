using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // Creates a WebApplicationBuilder
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList")); // Adds a TodoContext to the DI container
builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // Adds a filter that enables the developer exception page

var app = builder.Build(); // Creates a WebApplication

var todoItems = app.MapGroup("/todoitems");

todoItems.MapGet("/", GetAllTodos); // Adds a route to the app

todoItems.MapGet("/complete", GetCompleteTodos); // Adds a route to the app

todoItems.MapGet("/{id}", GetTodo); // Adds a route to the app

todoItems.MapPost("/", CreateTodo); // Adds a route to the app

todoItems.MapPut("/{id}", UpdateTodo);

todoItems.MapDelete("/{id}", DeleteTodo);

// GET / - returns hello world
app.MapGet("/", () => "Hello World!");

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.ToListAsync());
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
}

static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id) is Todo todo
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
}

static async Task<IResult> UpdateTodo(int id, Todo todo, TodoDb db)
{
    var existingTodo = await db.Todos.FindAsync(id);

    if (existingTodo is null) return Results.NotFound();

    existingTodo.Name = todo.Name;
    existingTodo.IsComplete = todo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
}

static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NoContent();
}