namespace BigDataApp;

public abstract class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public abstract string Category { get; set; }
    public HashSet<Movie>? Movies { get; set; }

    public override string ToString()
    {
        var result = $"Category = {Category}\nName = {Name}\n";
        Movies.ToList().ForEach(item => result += item.ToString());
        return result;
    }
    
    public override int GetHashCode()
    {
        if (Name == null) 
            return 0;
        
        return Name.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var other = obj as Person;
        return other != null && other.Name == this.Name;
    }
}

public class Actor : Person
{
    public override string Category { get; set; } = "actor";
}

public class Director : Person
{
    public override string Category { get; set; } = "director";
}