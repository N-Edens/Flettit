using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;

// Import af egne namespaces
using Data;
using Service;
using Model;

var builder = WebApplication.CreateBuilder(args);

var AllowSomeStuff = "_AllowSomeStuff"; // 

// Konfiguration af CORS for at tillade kommunikation med andre domæner
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSomeStuff, builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Konfiguration af databasekontekst (SQLite)
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextSQLite")));

// Registrering af PostService i Dependency Injection containeren
builder.Services.AddScoped<PostService>();

// Konfiguration af JSON serialization for at undgå cyklisk referencefejl
builder.Services.Configure<JsonOptions>(options =>
{
    // Ignorer cykliske referencer ved serialisering
    options.SerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataService = scope.ServiceProvider.GetRequiredService<PostService>(); // Hentning af PostService fra containeren
    dataService.SeedData(); // Kald af SeedData metoden for at initialisere data
}

app.UseHttpsRedirection(); // Middleware for at omdirigere HTTP til HTTPS
app.UseCors(AllowSomeStuff); // Middleware til at bruge CORS politik


// DataService fås via "Dependency Injection" (DI)
app.MapGet("/", (PostService service) =>
{
    return new { message = "Welcome to Flettit" };
});

app.MapGet("/api/posts", (PostService service) =>
{
    return service.GetPosts().Select(p => new {
        bookId = p.Id,
        title = p.Title,
        content = p.Content,
        username = p.User.Username,
        User = new
        {
            p.User.Id,
            p.User.Username
        }
    });
});

app.MapGet("/api/users", (PostService service) =>
{
    return service.GetUsers().Select(a => new { a.Id, a.Username });
});

app.MapGet("/api/users/{id}", (PostService service, int id) => {
    return service.GetUser(id);
});

app.MapPost("/api/posts", (PostService service, NewPostData data) =>
{
    string result = service.CreatePost(data.Title, data.Content, data.UserId);
    return new { message = result };
});

app.MapPost("/api/posts/{id}/comments", async (PostService service, int id, Comment comment) =>
{
    return await service.CreateComment(comment.Content, id, comment.UserId);
});

app.MapPut("/api/posts/{id}/upvote", async (PostService service, int id) =>
{
    return await service.UpvotePost(id);
});

app.MapPut("/api/posts/{id}/downvote", async (PostService service, int id) =>
{
    return await service.DownvotePost(id);
});


app.MapPut("/api/posts/{postId}/comments/{commentId}/upvote", async (PostService service, int postId, int commentId) =>
{
    return await service.UpvoteComment(commentId, postId);
});

app.MapPut("/api/posts/{postId}/comments/{commentId}/downvote", async (PostService service, int postId, int commentId) =>
{
    return await service.DownvoteComment(commentId, postId);
});


app.Run();
record NewPostData(string Title, string Content, int UserId);