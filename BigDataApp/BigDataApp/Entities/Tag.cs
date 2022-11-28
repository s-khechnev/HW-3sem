namespace BigDataApp;

public class Tag
{
    public int Id { get; set; }
    public string TagName { get; set; }
    public HashSet<Movie> Movies { get; set; }

    public override string ToString()
    {
        return TagName;
    }
}