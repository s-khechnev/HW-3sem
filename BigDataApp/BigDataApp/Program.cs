using BigDataApp;

MyParser.Run();

while (true)
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
}