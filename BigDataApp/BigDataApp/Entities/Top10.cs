﻿namespace BigDataApp;

public class Top10
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public HashSet<Movie>? Movies { get; set; }
}