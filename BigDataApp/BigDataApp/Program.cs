using System.Diagnostics;
using BigDataApp;
using Microsoft.EntityFrameworkCore;

void ReinitDb()
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();

    MyParser.Run();

    Console.WriteLine("Saving to db...");
    using (var dataContext = new DataContext())
    {
        dataContext.Database.EnsureDeleted();
        dataContext.Database.EnsureCreated();

        dataContext.Movies.AddRange(MyParser.FilmTitleMovie.Values);
        dataContext.Persons.AddRange(MyParser.ActorMovies.Keys);
        dataContext.Persons.AddRange(MyParser.DirectorMovies.Keys);
        dataContext.Tags.AddRange(MyParser.TagMovies.Keys);
        dataContext.Top10s.AddRange(MyParser.tops);

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
                movies = dataContext.Movies
                    .Include(x => x.Persons)
                    .Include(x => x.Tags)
                    .Include(x => x.Top10)
                    .Where(x => x.Title.ToLower() == line.ToLower());
                
                movies.ToList().ForEach(Console.WriteLine);
                
                break;
            case "person":
                
                line = Console.ReadLine();
                movies = dataContext.Movies
                    .Include(x => x.Persons)
                    .Include(x => x.Tags)
                    .Where(x => x.Persons.Any(p => p.Name.ToLower() == line.ToLower()));
                
                movies.ToList().ForEach(Console.WriteLine);
                
                break;
            case "tag":
                
                line = Console.ReadLine();
                movies = dataContext.Movies
                    .Include(x => x.Tags)
                    .Include(x => x.Persons)
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

/*var dataContext = new DataContext();

dataContext.Database.EnsureDeleted();
dataContext.Database.EnsureCreated();

var top1 = new Top10();
var top2 = new Top10();
var top3 = new Top10();

var movie1 = new Movie() { Title = "Movie1", Top10 = top1 };
var movie2 = new Movie() { Title = "Movie2", Top10 = top1 };
var movie3 = new Movie() { Title = "Movie3", Top10 = top2 };

dataContext.Movies.AddRange(movie1, movie2, movie3);
dataContext.Top10s.AddRange(top1, top2, top3);

dataContext.SaveChanges();

var t = dataContext.Movies.Include(x => x.Top10);

foreach (var item in t)
{
    Console.WriteLine(item);
}*/