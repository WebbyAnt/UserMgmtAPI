using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();


// Middleware to handle exceptions and enforce standardized error handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var errorResponse = new { message = "An unexpected error occurred." };
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
});

// Middleware to log requests and responses
app.Use(async (context, next) =>
{
    // Log request
    Console.WriteLine($"Incoming request: {context.Request.Method} {context.Request.Path}");

    // Call the next middleware in the pipeline
    await next();

    // Log response
    Console.WriteLine($"Outgoing response: {context.Response.StatusCode}");
});


var users = new List<User>
{
        new User { Id = 1, Name = "John Doe", Email = "john.doe@example.com", Password = "password" },
        new User { Id = 2, Name = "Jane Smith", Email = "jane.smith@example.com", Password = "password" }
};


// Define minimal API endpoints
app.MapGet("/users", () =>
{
    return Results.Ok(users);
});

app.MapPost("/users", (User user) =>
{
    if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
    {
        return Results.BadRequest(new { message = "Name, Email, and Password are required" });
    }

    user.Id = users.Max(u => u.Id) + 1; // Generate new ID

    // Add user to database (example)
    users.Add(user);

    return Results.Created($"/users/{user.Id}", user);
});

app.MapPut("/users/{id}", (User user, int id) =>
{
    if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
    {
        return Results.BadRequest(new { message = "Name, Email, and Password are required" });
    }

    var existingUser = users.FirstOrDefault(u => u.Id == id);
    if (existingUser == null)
    {
        return Results.NotFound();
    }

    existingUser.Name = user.Name;
    existingUser.Email = user.Email;
    existingUser.Password = user.Password;

    // Update user in database (example)

    return Results.Ok(existingUser);
});

app.MapDelete("/users/{id}", (int id) =>
{
    var existingUser = users.FirstOrDefault(u => u.Id == id);
    if (existingUser == null)
    {
        return Results.NotFound();
    }

    // Remove user from database (example)
    users.Remove(existingUser);

    return Results.NoContent();

});

app.Run();

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}