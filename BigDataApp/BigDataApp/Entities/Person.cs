namespace BigDataApp;

public abstract class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public abstract string Category { get; set; }
    public HashSet<Movie>? Movies { get; set; }

    public override string ToString()
    {
        var result = $"Category = {Category}\nName = {Name}\n";
        Movies.ToList().ForEach(item => result += item.ToString());
        return result;
    }
}

public class Actor : Person
{
    public override string Category { get; set; } = "Actor";
}

public class Director : Person
{
    public override string Category { get; set; } = "Director";
}