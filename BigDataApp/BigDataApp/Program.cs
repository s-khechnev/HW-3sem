using System.Diagnostics;
using System.Threading.Channels;
using BigDataApp;

using var dataContext = new DataContext();
Console.WriteLine(dataContext.Movies.Count());


/*MyParser.Run();

using var dataContext = new DataContext();

var stopwatch = new Stopwatch();
stopwatch.Start();

dataContext.Movies.AddRange(MyParser.FilmTitleMovie.Values);
dataContext.Persons.AddRange(MyParser.PersonNameMovies.Keys.Select(item => new Actor() { Name = item }));
dataContext.Persons.AddRange(MyParser.PersonNameMovies.Keys.Select(item => new Director() { Name = item }));
dataContext.Tags.AddRange(MyParser.TagMovies.Keys.Select(item => new Tag() { TagName = item }));
dataContext.SaveChanges();
stopwatch.Stop();

TimeSpan ts = stopwatch.Elapsed;
string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
    ts.Hours, ts.Minutes, ts.Seconds,
    ts.Milliseconds / 10);

Console.WriteLine(elapsedTime);*/
Console.WriteLine("Complete");
/*while (true)
{
    Console.WriteLine("Введите название фильма или имя актера или режиссера или тег");
    string line = Console.ReadLine();

    if (MyParser.FilmTitleMovie.TryGetValue(line, out var movie))
    {
        Console.WriteLine(movie);   
    }
    else if (MyParser.PersonNameMovies.TryGetValue(line, out var movieSet1))
    {
        movieSet1.ToList().ForEach(Console.WriteLine);
    }
    else if (MyParser.TagMovies.TryGetValue(line, out var movieSet2))
    {
        movieSet2.ToList().ForEach(Console.WriteLine);
    }
    else
    {
        Console.WriteLine("Ничего не найдено");
    }

    Console.WriteLine("\n");
}*/