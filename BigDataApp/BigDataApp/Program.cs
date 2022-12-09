using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using BigDataApp;
using BigDataApp.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;


public static class Program
{
    static void ReinitDb()
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
                    writer.Write(movie.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(movie.Rating, NpgsqlTypes.NpgsqlDbType.Real);
                    writer.Write(movie.OriginalTitle, NpgsqlDbType.Text);
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"Persons\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var person in MyParser._personIdPerson.Values)
                {
                    if (string.IsNullOrEmpty(person.Category))
                        continue;
                    writer.StartRow();
                    writer.Write(person.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(person.Name, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(person.Category, NpgsqlTypes.NpgsqlDbType.Text);
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"Tags\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var tag in MyParser._tagNameTag.Values)
                {
                    writer.StartRow();
                    writer.Write(tag.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(tag.Name, NpgsqlTypes.NpgsqlDbType.Text);
                }

                writer.Complete();
            }

            using (var writer = connection.BeginBinaryImport("copy \"Titles\" from STDIN (FORMAT BINARY)"))
            {
                foreach (var title in MyParser.Titles)
                {
                    writer.StartRow();
                    writer.Write(title.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(title.Name, NpgsqlTypes.NpgsqlDbType.Text);
                    writer.Write(title.MovieId, NpgsqlTypes.NpgsqlDbType.Integer);
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
                        writer.Write(movie.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(topMovie.Id, NpgsqlTypes.NpgsqlDbType.Integer);
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
                        writer.Write(movie.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(person.Id, NpgsqlTypes.NpgsqlDbType.Integer);
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
                        writer.Write(movie.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                        writer.Write(tag.Id, NpgsqlTypes.NpgsqlDbType.Integer);
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
        string gloablElapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            gTs.Hours, gTs.Minutes, gTs.Seconds,
            gTs.Milliseconds / 10);
        
        Console.WriteLine("Time execution reinit db: ");
        Console.WriteLine(gloablElapsedTime);
    }

    static void Main()
    {
        Run();
    }

    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    private static void Run()
    {
        using (var context = new DataContext())
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

                        var titles = context.Titles
                            .Where(x => x.Name.ToLower() == line.ToLower())
                            .Include(x => x.Movie);

                        var title = titles.First();
                        var movieId = title.MovieId;

                        var movieEntity = context.Movies
                            .Where(movie => movieId == movie.Id)
                            .Include(x => x.Top)
                            .ThenInclude(x => x.Persons)
                            .AsSplitQuery()
                            .Include(x => x.Top)
                            .ThenInclude(x => x.Tags)
                            .AsSplitQuery()
                            .Include(x => x.Persons)
                            .Include(x => x.Tags);

                        Console.WriteLine();
                        Console.WriteLine(title.Name);
                        Console.WriteLine();
                        movieEntity.ToList().ForEach(Console.WriteLine);

                        break;
                    case "person":

                        line = Console.ReadLine();
                        
                        movies = context.Movies
                            .Include(x => x.Persons)
                            .Include(x => x.Tags)
                            .AsSplitQuery()
                            .Include(x => x.Top)
                            .ThenInclude(x => x.Persons)
                            .AsSplitQuery()
                            .Include(x => x.Top)
                            .ThenInclude(x => x.Tags)
                            .Where(x => x.Persons.Any(p => p.Name.ToLower() == line.ToLower()));
                            
                        movies.ToList().ForEach(Console.WriteLine);

                        break;
                    case "tag":

                        line = Console.ReadLine();
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
    }
}