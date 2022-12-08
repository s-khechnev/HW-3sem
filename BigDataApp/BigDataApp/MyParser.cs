using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BigDataApp.Entities;

namespace BigDataApp;

public static class MyParser
{
    private static Dictionary<string, Dictionary<string, List<Person>>> _filmIdCategoryPersons = new();
    private static Dictionary<string, string>? _ratingDict = new();
    private static Dictionary<string, List<Tag>>? _filmIdTags = new();

    private static Dictionary<string, string> _filmTitleFilmId = new();

    private static Dictionary<string, List<string>> _filmIdFilmTitles = new();
    private static Dictionary<string, Person> _personIdPerson = new();

    public static Dictionary<string, Movie> FilmTitleMovie = new();
    public static Dictionary<Person, HashSet<Movie>> DirectorMovies = new();
    public static Dictionary<Person, HashSet<Movie>> ActorMovies = new();
    public static Dictionary<Tag, HashSet<Movie>> TagMovies = new();

    private static void AddPersons(string filmId, string category, Movie movie)
    {
        if (_filmIdCategoryPersons[filmId].ContainsKey(category))
        {
            foreach (var item in _filmIdCategoryPersons[filmId][category])
            {
                if (category == "director")
                {
                    movie.Persons.Add(item);
                }
                else
                {
                    movie.Persons.Add(item);
                }
            }
        }
    }

    private static Movie GetMovieByTitleAndFilmId(string filmTitle, string filmId)
    {
        var result = new Movie();
        result.Title = filmTitle;
        result.Persons = new();

        if (_ratingDict != null && _ratingDict.ContainsKey(filmId))
        {
            result.Rating = float.Parse(_ratingDict[filmId], CultureInfo.InvariantCulture.NumberFormat);
        }
        else
        {
            result.Rating = -1f;
        }

        if (_filmIdCategoryPersons.ContainsKey(filmId))
        {
            AddPersons(filmId, "actor", result);
            AddPersons(filmId, "director", result);
        }

        if (_filmIdTags != null && _filmIdTags.ContainsKey(filmId))
        {
            result.Tags = _filmIdTags[filmId].ToHashSet();
        }

        return result;
    }

    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    public static void Run(DataContext context)
    {
        Console.WriteLine("Parsing...");
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        const string movieCodesPath = @"C:\Users\s-khechnev\Desktop\ml-latest\MovieCodes_IMDB.tsv";

        var filmIdTask = Task.Run(() =>
        {
            using var fs = new FileStream(movieCodesPath, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024,
                FileOptions.SequentialScan);
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    var index = lineSpan.IndexOf('\t');
                    var filmId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var filmTitle = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var lang = lineSpan.Slice(0, index).ToString();

                    if (lang == "en" || lang == "ru")
                    {
                        if (_filmIdFilmTitles.ContainsKey(filmId))
                        {
                            _filmIdFilmTitles[filmId].Add(filmTitle);
                        }
                        else
                        {
                            _filmIdFilmTitles[filmId] = new List<string> { filmTitle };
                        }

                        if (!_filmTitleFilmId.ContainsKey(filmTitle))
                            _filmTitleFilmId.Add(filmTitle, filmId);
                    }
                }
            }
        });

        var actorsTask = Task.Run(() =>
        {
            const string pathActorsDirectorsNames =
                @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsNames_IMDB.txt";

            using (var fs = new FileStream(pathActorsDirectorsNames, FileMode.Open, FileAccess.Read, FileShare.None,
                       64 * 1024, FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    var index = lineSpan.IndexOf('\t');
                    var personId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var personName = lineSpan.Slice(0, index).ToString();

                    _personIdPerson[personId] = new Person() { Name = personName };
                }
            }

            filmIdTask.Wait();

            const string pathActorsDirectorsCodes =
                @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsCodes_IMDB.tsv";
            using (var fs = new FileStream(pathActorsDirectorsCodes, FileMode.Open, FileAccess.Read, FileShare.None,
                       64 * 1024, FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    int index;
                    index = lineSpan.IndexOf('\t');
                    var filmId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var personId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var category = lineSpan.Slice(0, index).ToString();

                    if (!(category == "actor" || category == "actress" || category == "self" || category == "director"))
                        continue;

                    if (category == "actress" || category == "self")
                        category = "actor";

                    if (!_filmIdFilmTitles.ContainsKey(filmId))
                        continue;

                    var person = _personIdPerson.ContainsKey(personId)
                        ? _personIdPerson[personId]
                        : new Person() { Name = personId };

                    person.Category = category;

                    if (_filmIdCategoryPersons.ContainsKey(filmId))
                    {
                        if (_filmIdCategoryPersons[filmId].ContainsKey(category))
                        {
                            _filmIdCategoryPersons[filmId][category].Add(person);
                        }
                        else
                        {
                            _filmIdCategoryPersons[filmId][category] = new List<Person>() { person };
                        }
                    }
                    else
                    {
                        _filmIdCategoryPersons[filmId] = new Dictionary<string, List<Person>>();
                        _filmIdCategoryPersons[filmId][category] = new List<Person>() { person };
                    }
                }
            }
        });

        var ratingTask = Task.Run(() =>
        {
            string pathRating = @"C:\Users\s-khechnev\Desktop\ml-latest\Ratings_IMDB.tsv";
            using (var fs = new FileStream(pathRating, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024,
                       FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    var index = lineSpan.IndexOf('\t');
                    var filmId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var rait = lineSpan.Slice(0, index).ToString();

                    if (!filmIdTask.IsCompleted)
                    {
                        filmIdTask.Wait();
                    }

                    if (!_filmIdFilmTitles.ContainsKey(filmId))
                        continue;

                    _ratingDict[filmId] = rait;
                }
            }
        });

        var linksIdTask = Task.Run(() =>
        {
            var pathLinks = @"C:\Users\s-khechnev\Desktop\ml-latest\links_IMDB_MovieLens.csv";

            var idImdbId = new Dictionary<string, string>();
            using (var fs = new FileStream(pathLinks, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024,
                       FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    var index = lineSpan.IndexOf(',');
                    var filmLensId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf(',');
                    var filmIdImdb = lineSpan.Slice(0, index).ToString();

                    idImdbId[filmLensId] = string.Concat("tt", filmIdImdb);
                }
            }

            return idImdbId;
        });

        var codeTagTask = Task.Run(() =>
        {
            var pathTagCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\TagCodes_MovieLens.csv";

            var codeTagTag = new Dictionary<string, Tag>();
            using (var fs = new FileStream(pathTagCodes, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024,
                       FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    int index;
                    index = lineSpan.IndexOf(',');
                    var codeTag = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    var tag = lineSpan.ToString();

                    codeTagTag[codeTag] = new Tag() { Name = tag };
                }
            }

            return codeTagTag;
        });

        var tagsTask = Task.Run(() =>
        {
            var idImdbId = linksIdTask.Result;
            var codeTagTag = codeTagTask.Result;

            var pathTagScores = @"C:\Users\s-khechnev\Desktop\ml-latest\TagScores_MovieLens.csv";

            using (var fs = new FileStream(pathTagScores, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024,
                       FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    var index = lineSpan.IndexOf(',');
                    var movieId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf(',');
                    var tagId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    var relevance = lineSpan.ToString();

                    if (!filmIdTask.IsCompleted)
                    {
                        filmIdTask.Wait();
                    }

                    if (!_filmIdFilmTitles.ContainsKey(idImdbId[movieId]) ||
                        !(float.Parse(relevance, CultureInfo.InvariantCulture.NumberFormat) > 0.5f))
                        continue;

                    if (_filmIdTags.ContainsKey(idImdbId[movieId]))
                    {
                        _filmIdTags[idImdbId[movieId]].Add(codeTagTag[tagId]);
                    }
                    else
                    {
                        _filmIdTags[idImdbId[movieId]] = new List<Tag>() { codeTagTag[tagId] };
                    }
                }
            }
        });

        Task.WaitAll(actorsTask, ratingTask, tagsTask, codeTagTask, filmIdTask, linksIdTask);
        //ans

        //ans1
        foreach (var filmId in _filmIdFilmTitles.Keys)
        {
            foreach (var filmTitle in _filmIdFilmTitles[filmId])
            {
                if (FilmTitleMovie.ContainsKey(filmTitle))
                    continue;
                FilmTitleMovie.Add(filmTitle, GetMovieByTitleAndFilmId(filmTitle, filmId));
            }
        }

        var ansTask2 = Task.Run(() =>
        {
            foreach (var filmId in _filmIdCategoryPersons.Keys)
            {
                foreach (var keyPersonCategory in _filmIdCategoryPersons[filmId].Keys)
                {
                    foreach (var person in _filmIdCategoryPersons[filmId][keyPersonCategory])
                    {
                        if (keyPersonCategory == "director")
                            AddToPersonDict(filmId, person, DirectorMovies);
                        else if (keyPersonCategory == "actor")
                            AddToPersonDict(filmId, person, ActorMovies);
                    }
                }
            }
        });

        var ansTask3 = Task.Run(() =>
        {
            foreach (var filmId in _filmIdFilmTitles.Keys)
            {
                if (!_filmIdTags.ContainsKey(filmId))
                    continue;

                foreach (var tag in _filmIdTags[filmId])
                {
                    if (TagMovies.ContainsKey(tag))
                        TagMovies[tag].Add(FilmTitleMovie[_filmIdFilmTitles[filmId].First()]);
                    else
                    {
                        TagMovies[tag] = new HashSet<Movie>();
                        TagMovies[tag].Add(FilmTitleMovie[_filmIdFilmTitles[filmId].First()]);
                    }
                }
            }
        });

        Task.WaitAll(ansTask2, ansTask3);

        Console.WriteLine("Complete answers dictionary");

        //InitTop10();
        var count = 0;

        Console.WriteLine("Candidates solving");

        /*var candidates = new Dictionary<Movie, HashSet<Movie>>();
        foreach (var movie in FilmTitleMovie.Values)
        {
            if (movie.Tags != null)
                foreach (var tag in movie.Tags)
                {
                    if (candidates.ContainsKey(movie))
                        candidates[movie].UnionWith(TagMovies[tag].ToHashSet());
                    else
                        candidates[movie] = TagMovies[tag].ToHashSet();
                }

            if (movie.Persons != null)
                foreach (var person in movie.Persons)
                {
                    if (person.Category == "actor")
                    {
                        if (candidates.ContainsKey(movie))
                            candidates[movie].UnionWith(ActorMovies[person]);
                        else
                            candidates[movie] = ActorMovies[person];
                    }
                    if (person.Category == "director")
                    {
                        if (candidates.ContainsKey(movie))
                            candidates[movie].UnionWith(DirectorMovies[person]);
                        else
                            candidates[movie] = DirectorMovies[person];
                    }
                }
        }*/

        var candidates = FilmTitleMovie.Values
            .Where(m => Math.Abs(m.Rating - (-1f)) > float.Epsilon && m.Tags != null && m.Persons != null);
        
        Console.WriteLine("Candidates are done");

        Console.WriteLine("Init top");
        Parallel.ForEach(FilmTitleMovie.Values, item =>
        {
            Dictionary<float, HashSet<Movie>> estimationMovies = new();
            Dictionary<string, string> addedMovies = new(); //id -> title
            foreach (var movie in candidates)
            {
                if (movie == item)
                    continue;

                if (_filmTitleFilmId[item.Title] == _filmTitleFilmId[movie.Title])
                    continue;

                if (addedMovies.ContainsKey(_filmTitleFilmId[movie.Title]))
                    continue;

                var estimation = item.GetEstimation(movie);

                if (estimationMovies.ContainsKey(estimation))
                    estimationMovies[estimation].Add(movie);
                else
                    estimationMovies.Add(estimation, new HashSet<Movie>() { movie });

                addedMovies[_filmTitleFilmId[movie.Title]] = movie.Title;
            }

            int k = 0;

            item.Top = new List<Movie>();
            var orderedDict = estimationMovies.OrderByDescending(x => x.Key);
            foreach (var estMovie in orderedDict)
            {
                foreach (var film in estMovie.Value)
                {
                    item.Top.Add(film);
                    k++;
                    if (k == 10)
                        break;
                }

                if (k == 10)
                    break;
            }
            
            count++;
            if (count % 10000 == 0)
            {
                Console.WriteLine(count);
            }
                
        });

        Console.WriteLine("End init top");
        stopwatch.Stop();

        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine(elapsedTime);
        Console.WriteLine("Complete parsing");
    }

    private static void AddToPersonDict(string filmId, Person personName, Dictionary<Person, HashSet<Movie>> dict)
    {
        if (dict.ContainsKey(personName))
            dict[personName].Add(FilmTitleMovie[_filmIdFilmTitles[filmId].First()]);
        else
        {
            dict[personName] = new HashSet<Movie>();
            dict[personName].Add(FilmTitleMovie[_filmIdFilmTitles[filmId].First()]);
        }
    }
}