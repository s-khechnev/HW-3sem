namespace MovieAppBlazor.Data;

public sealed class Title
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public Movie? Movie { get; set; }
    public int MovieId { get; set; }
}