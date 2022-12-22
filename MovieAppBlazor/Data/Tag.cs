namespace MovieAppBlazor.Data;

public sealed class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public HashSet<Movie> Movies { get; set; }

    public override string ToString()
    {
        return Name;
    }
    
    public override int GetHashCode()
    {
        if (Name == null) 
            return 0;
        
        return Name.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var other = obj as Tag;
        return other != null && other.Name == this.Name;
    }
}