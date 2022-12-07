using System.Diagnostics;
using BigDataApp;
using BigDataApp.Entities;
using Microsoft.EntityFrameworkCore;

void ReinitDb()
{
    using (var dataContext = new DataContext())
    {
        dataContext.Database.EnsureDeleted();
        
        MyParser.Run(dataContext);

        Console.WriteLine("Saving to db...");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        
        dataContext.Database.EnsureCreated();

        dataContext.Movies.AddRange(MyParser.FilmTitleMovie.Values);
        dataContext.Persons.AddRange(MyParser.ActorMovies.Keys);
        dataContext.Persons.AddRange(MyParser.DirectorMovies.Keys);
        dataContext.Tags.AddRange(MyParser.TagMovies.Keys);

        dataContext.SaveChanges();
        stopwatch.Stop();

        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        Console.WriteLine(elapsedTime);
        Console.WriteLine("Complete saving to db...");
    }
}

using (var dataContext = new DataContext())
{
    while (true)
    {
        Console.WriteLine("film, person, tag");
        string line = Console.ReadLine();

        if (line == null)
            continue;

        IQueryable<Movie> movies;

        switch (line.ToLower())
        {
            case "film":

                line = Console.ReadLine();

                Stopwatch stopwatch = new();
                stopwatch.Start();

                movies = dataContext.Movies
                    .Include(x => x.Persons)
                    .Include(x => x.Tags)
                    .AsSplitQuery()
                    .Include(x => x.Top)!
                    .ThenInclude(x => x.Persons)
                    .AsSplitQuery()
                    .Include(x => x.Top)!
                    .ThenInclude(x => x.Tags)
                    .Where(x => line != null && x.Title.ToLower() == line.ToLower());

                movies.ToList().ForEach(Console.WriteLine);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine(elapsedTime);

                break;
            case "person":

                line = Console.ReadLine();
                movies = dataContext.Movies
                    .Include(x => x.Persons)
                    .Include(x => x.Tags)
                    .AsSplitQuery()
                    .Include(x => x.Top)!
                    .ThenInclude(x => x.Persons)
                    .AsSplitQuery()
                    .Include(x => x.Top)!
                    .ThenInclude(x => x.Tags)
                    .Where(x => x.Persons.Any(p => p.Name.ToLower() == line.ToLower()));

                movies.ToList().ForEach(Console.WriteLine);

                break;
            case "tag":

                line = Console.ReadLine();
                movies = dataContext.Movies
                    .Include(x => x.Persons)
                    .Include(x => x.Tags)
                    .AsSplitQuery()
                    .Include(x => x.Top)!
                    .ThenInclude(x => x.Persons)
                    .AsSplitQuery()
                    .Include(x => x.Top)!
                    .ThenInclude(x => x.Tags)
                    .Where(x => x.Tags.Any(t => t.Name.ToLower() == line.ToLower()));

                movies.ToList().ForEach(Console.WriteLine);

                break;
            case "reinit":
                ReinitDb();
                break;
        }

        Console.WriteLine();
    }
}

/*
var dataContext = new DataContext();

dataContext.Database.EnsureDeleted();
dataContext.Database.EnsureCreated();

var movie1 = new Movie() { Title = "Movie1" };
var movie2 = new Movie() { Title = "Movie2" };
var movie3 = new Movie() { Title = "Movie3" };

movie1.Top = new HashSet<Movie>() { movie2, movie3 };
movie2.Top = new HashSet<Movie>() { movie1, movie3 };
movie3.Top = new HashSet<Movie>() { movie1, movie2 };

dataContext.Movies.AddRange(movie1, movie2, movie3);

dataContext.SaveChanges();

var t = dataContext.Movies;

foreach (var item in t)
{
    Console.WriteLine(item);
}*/