using System.Diagnostics;
using System.Threading.Channels;
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

//ReinitDb();

using (var dataContext = new DataContext())
{
    while (true)
    {
        Console.WriteLine("film, person, tag");
        string line = Console.ReadLine();

        if (line == null)
            continue;

        switch (line.ToLower())
        {
            case "film":
                
                line = Console.ReadLine();
                List<Movie> searchMovies = dataContext.Movies.Include(x => x.Persons)
                    .Include(x => x.Tags).Where(x => x.Title.ToLower() == line.ToLower()).ToList();
                searchMovies.ForEach(Console.WriteLine);
                
                break;
            case "person":
                
                line = Console.ReadLine();
                List<Person> searchPersons = dataContext.Persons
                    .Include(x => x.Movies)
                    .Where(x => x.Name.ToLower() == line.ToLower()).ToList();
                searchPersons.ForEach(Console.WriteLine);
                
                break;
            case "tag":
                
                line = Console.ReadLine();
                var movies = dataContext.Movies
                    .Include(x => x.Tags)
                    .Include(x => x.Persons)
                    .Where(x => x.Tags.Select(x => x.Name).Contains(line.ToLower()));

                foreach (var movie in movies)
                {
                    Console.WriteLine(movie);
                }
                
                /*var movie = dataContext.Tags
                    .Include(x => x.Movies)
                    .Where(x => x.Name == line.ToLower());

                foreach (var item in movie)
                {
                    foreach (var t in item.Movies)
                    {
                        Console.WriteLine(t);
                    }
                }*/
                
                break;
            case "reinit":
                ReinitDb();
                break;
        }

        Console.WriteLine();
    }
}