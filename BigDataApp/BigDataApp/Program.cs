using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BigDataApp.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace BigDataApp;

public static class Program
{
    private static void ReinitDb()
    {
        var globalStopWatch = new Stopwatch();
        globalStopWatch.Start();

        MyParser.Run();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        using (var context = new DataContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        Console.WriteLine("Adding to db...");
        var connectionString = new NpgsqlConnectionStringBuilder()
        {
            Host = "localhost",
            Port = 5432,
            Database = "bigDataAppDB",
            Username = "test",
            Password = "12345",
            Pooling = false,
            Timeout = 300,
            CommandTimeout = 300
        }.ToString();

        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            using (var writer = connection.BeginBinaryImport("copy \"Movies\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var movie in MyParser._filmIdMovie.Values)
                {
                    writer.StartRow();
                    writer.Write(movie.Id, NpgsqlDbType.Integer);
                    writer.Write(movie.IdImdb, NpgsqlDbType.Text);
                    writer.Write(movie.Rating, NpgsqlDbType.Real);
                    writer.Write(movie.OriginalTitle, NpgsqlDbType.Text);
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"Persons\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var person in MyParser._personIdPerson.Values)
                {
                    writer.StartRow();
                    writer.Write(person.Id, NpgsqlDbType.Integer);
                    writer.Write(person.IdImdb, NpgsqlDbType.Text);
                    writer.Write(person.Name, NpgsqlDbType.Text);
                    writer.Write(person.Category, NpgsqlDbType.Text);
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"Tags\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var tag in MyParser._tagNameTag.Values)
                {
                    writer.StartRow();
                    writer.Write(tag.Id, NpgsqlDbType.Integer);
                    writer.Write(tag.Name, NpgsqlDbType.Text);
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"Titles\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var title in MyParser.Titles)
                {
                    writer.StartRow();
                    writer.Write(title.Id, NpgsqlDbType.Integer);
                    writer.Write(title.Name, NpgsqlDbType.Text);
                    writer.Write(title.MovieId, NpgsqlDbType.Integer);
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"MovieMovie\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var movie in MyParser._filmIdMovie.Values)
                {
                    if (movie.Top == null)
                        continue;

                    foreach (var topMovie in movie.Top)
                    {
                        writer.StartRow();
                        writer.Write(movie.Id, NpgsqlDbType.Integer);
                        writer.Write(topMovie.Id, NpgsqlDbType.Integer);
                    }
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"MoviePerson\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var movie in MyParser._filmIdMovie.Values)
                {
                    if (movie.Persons == null || movie.Persons.Count == 0)
                        continue;

                    var persons = movie.Persons;
                    foreach (var person in persons)
                    {
                        writer.StartRow();
                        writer.Write(movie.Id, NpgsqlDbType.Integer);
                        writer.Write(person.Id, NpgsqlDbType.Integer);
                    }
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"MovieTag\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var movie in MyParser._filmIdMovie.Values)
                {
                    if (movie.Tags == null)
                        continue;

                    foreach (var tag in movie.Tags)
                    {
                        writer.StartRow();
                        writer.Write(movie.Id, NpgsqlDbType.Integer);
                        writer.Write(tag.Id, NpgsqlDbType.Integer);
                    }
                }

                writer.Complete();
            }
        }

        stopwatch.Stop();
        globalStopWatch.Stop();

        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        Console.WriteLine(elapsedTime);
        Console.WriteLine("Complete saving to db...");

        TimeSpan gTs = globalStopWatch.Elapsed;
        string globalElapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            gTs.Hours, gTs.Minutes, gTs.Seconds,
            gTs.Milliseconds / 10);

        Console.WriteLine("Time execution reinit db: ");
        Console.WriteLine(globalElapsedTime);
    }

    static void Main()
    {
        Run();
    }

    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    private static void Run()
    {
        using var context = new DataContext();
        while (true)
        {
            Console.WriteLine("film, person, tag");
            string? line = Console.ReadLine();

            if (line == null)
                continue;

            IQueryable<Movie> movies;

            switch (line.ToLower())
            {
                case "film":

                    line = Console.ReadLine();

                    var movies1 = GetMovies(line);
                    
                    foreach (var movie in movies1)
                    {
                        Console.WriteLine(movie);
                    }

                    break;
                case "person":

                    line = Console.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        break;

                    movies = context.Movies
                        .Where(x => x.Persons.Any(p => p.Name.ToLower() == line.ToLower()))
                        .Include(x => x.Persons)
                        .Include(x => x.Tags)
                        .AsSplitQuery()
                        .Include(x => x.Top)!
                        .ThenInclude(x => x.Persons)
                        .AsSplitQuery()
                        .Include(x => x.Top)!
                        .ThenInclude(x => x.Tags);

                    movies.ToList().ForEach(Console.WriteLine);

                    break;
                case "tag":

                    line = Console.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        break;

                    movies = context.Movies
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
                    Console.WriteLine("Are you sure?");
                    var ans = Console.ReadKey();
                    if (ans.Key == ConsoleKey.Y)
                    {
                        Console.WriteLine();
                        ReinitDb();
                    }

                    break;
            }

            Console.WriteLine();
        }
    }

    public static List<Movie> GetMovies(string? line)
    {
        var movies = new List<Movie>();
        using (var context = new DataContext())
        {
            if (string.IsNullOrEmpty(line))
                return null;

            Stopwatch stopwatch = new();
            stopwatch.Start();

            var titles = context.Titles
                .Include(x => x.Movie)
                .Where(x => x.Name.ToLower() == line.ToLower())
                .GroupBy(x => x.MovieId).Select(x => x.First());

            if (!titles.Any())
            {
                //Console.WriteLine("Not found");
                return null;
            }

            //Console.WriteLine("Фильмы с title = " + line);
            foreach (var title in titles)
            {
                var movieId = title.MovieId;

                using (var ctx = new DataContext())
                {
                    var movieEntity = ctx.Movies
                        .Where(movie => movieId == movie.Id)
                        .Include(x => x.Top)!
                        .ThenInclude(x => x.Persons)
                        .AsSplitQuery()
                        .Include(x => x.Top)!
                        .ThenInclude(x => x.Tags)
                        .AsSplitQuery()
                        .Include(x => x.Persons)
                        .Include(x => x.Tags)
                        .Single();

                    movies.Add(movieEntity);
                }
            }
        }
        return movies;
    }
}