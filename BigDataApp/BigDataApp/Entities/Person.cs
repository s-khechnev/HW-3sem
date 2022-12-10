using System.ComponentModel.DataAnnotations.Schema;
using BigDataApp.Entities;

namespace BigDataApp;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public virtual HashSet<Movie>? Movies { get; set; }

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
