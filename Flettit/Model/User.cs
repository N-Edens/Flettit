using Model;

namespace Model;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public List<Post> Posts { get; set; } = new();
    public User(string username = "")
    {
        Username = username;
    }
    public User()
    {
        Id = 0;
        Username = "";
    }
}