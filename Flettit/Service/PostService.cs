using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using Data;
using Model;

namespace Service;

public class PostService
{
    private DataContext db { get; }

    public PostService(DataContext db)
    {
        this.db = db;
    }
    /// <summary>
    /// Seeder noget nyt data i databasen hvis det er nødvendigt.
    /// </summary>
    public void SeedData()
    {

        User user = db.Users.FirstOrDefault()!;
        if (user == null)
        {
            user = new User { Username = "Kristian" };
            db.Users.Add(user);
        }

        var currentTime = DateTime.Now;

        Post post = db.Posts.FirstOrDefault()!;
        if (post == null)
        {
            db.Posts.Add(new Post { Title = "Neymar overrated", Content = "Neymar har ikke spillet i flere år", User = user, CreationDate = currentTime });
        }

        Comment comment = db.Comments.FirstOrDefault()!;
        if (comment == null)
        {
            db.Comments.Add(new Comment { Content = "haha bad opnion nerd", User = user, CreationDate = currentTime });
        }

        db.SaveChanges();
    }

    public List<Post> GetPosts()
    {
        return db.Posts.Include(p => p.User).ToList();
    }

    public Post GetPost(int id)
    {
        return db.Posts.Include(p => p.User).FirstOrDefault(p => p.Id == id);
    }

    public List<User> GetUsers()
    {
        return db.Users.ToList();
    }

    public User GetUser(int id)
    {
        return db.Users.Include(u => u.Posts).FirstOrDefault(u => u.Id == id);
    }
    public async Task<Post> UpvotePost(int id)
    {
        var post = await db.Posts.FindAsync(id);
        if (post != null)
        {
            post.Upvotes++;
            await db.SaveChangesAsync();
        }
        return post;
    }

    public async Task<Post> DownvotePost(int id)
    {
        var post = await db.Posts.FindAsync(id);
        if (post != null)
        {
            post.Downvotes++;
            await db.SaveChangesAsync();
        }
        return post;
    }

    public async Task<Comment> UpvoteComment(int id)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment != null)
        {
            comment.Upvotes++;
            await db.SaveChangesAsync();
        }
        return comment;
    }

    public async Task<Comment> DownvoteComment(int id)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment != null)
        {
            comment.Downvotes++;
            await db.SaveChangesAsync();
        }
        return comment;
    }
    public string CreatePost(string title, string content, int userId)
    {
        // Check if the user exists
        User user = db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            // User doesn't exist, handle this case
            return "User not found";
        }

        // User exists, create the post
        db.Posts.Add(new Post { Title = title, Content = content, User = user });
        db.SaveChanges();
        return "Post created";
    }

    public async Task<Comment> CreateComment(string content, int postId, int userId)
    {
        var post = await db.Posts.FindAsync(postId);
        var comment = new Comment { Content = content, UserId = userId };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        return comment;
    }

}