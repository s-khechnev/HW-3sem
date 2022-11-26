using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace BigDataApp;

internal static class Program
{
    private static Dictionary<string, Dictionary<string, List<string>>> _filmIdCategoryActors = new();
    private static Dictionary<string, string>? _ratingDict = new();
    private static Dictionary<string, List<string>>? _filmIdTags = new();

    private static Dictionary<string, List<string>> _filmIdFilmTitles = new();
    private static Dictionary<string, string> _personIdPersonName = new();

    private static void Add(string filmId, string category, HashSet<string>? actors)
    {
        if (_filmIdCategoryActors[filmId].ContainsKey(category))
        {
            if (actors != null)
                actors.UnionWith(_filmIdCategoryActors[filmId][category].ToHashSet());
            else
                actors = new HashSet<string>(_filmIdCategoryActors[filmId][category]);
        }
    }

    private static Movie GetMovieByTitleAndFilmId(string filmTitle, string filmId)
    {
        var title = filmTitle;
        float rating;
        HashSet<string>? directors = null;
        HashSet<string>? tags = null;
        HashSet<string>? actors = null;

        if (_ratingDict != null && _ratingDict.ContainsKey(filmId))
        {
            rating = float.Parse(_ratingDict[filmId], CultureInfo.InvariantCulture.NumberFormat);
        }
        else
        {
            rating = -1f;
        }

        if (_filmIdCategoryActors.ContainsKey(filmId))
        {
            Add(filmId, "actor", actors);
            Add(filmId, "actress", actors);
            Add(filmId, "self", actors);
            if (_filmIdCategoryActors[filmId].ContainsKey("director"))
            {
                directors = _filmIdCategoryActors[filmId]["director"].ToHashSet();
            }
        }

        if (_filmIdTags != null && _filmIdTags.ContainsKey(filmId))
        {
            tags = _filmIdTags[filmId].ToHashSet();
        }

        return new Movie()
        {
            Title = title,
            Rating = rating,
            Actors = actors,
            Directors = directors,
            Tags = tags
        };
    } 

    //25.31 release split // 18.14 release without split
    private static void Main(string[] args)
    {
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

                    int index;
                    index = lineSpan.IndexOf('\t');
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
                            _filmIdFilmTitles[filmId].Add(filmTitle);
                        else
                            _filmIdFilmTitles[filmId] = new List<string> { filmTitle };
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

                    int index;
                    index = lineSpan.IndexOf('\t');
                    var personId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var personName = lineSpan.Slice(0, index).ToString();
                    
                    _personIdPersonName[personId] = personName;
                }
            }

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
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var category = lineSpan.Slice(0, index).ToString();
                    
                    if (!filmIdTask.IsCompleted)
                    {
                        filmIdTask.Wait();
                    }

                    if (!_filmIdFilmTitles.ContainsKey(filmId))
                        continue;

                    var personName = _personIdPersonName.ContainsKey(personId)
                        ? _personIdPersonName[personId]
                        : personId;

                    if (_filmIdCategoryActors.ContainsKey(filmId))
                    {
                        if (_filmIdCategoryActors[filmId].ContainsKey(category))
                        {
                            _filmIdCategoryActors[filmId][category].Add(personName);
                        }
                        else
                        {
                            _filmIdCategoryActors[filmId][category] = new List<string>() { personName };
                        }
                    }
                    else
                    {
                        _filmIdCategoryActors[filmId] = new Dictionary<string, List<string>>();
                        _filmIdCategoryActors[filmId][category] = new List<string>() { personName };
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

                    int index;
                    index = lineSpan.IndexOf('\t');
                    var filmId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var rait = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

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

                    int index;
                    index = lineSpan.IndexOf(',');
                    var filmLensId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf(',');
                    var filmIdImdb = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    idImdbId[filmLensId] = string.Concat("tt", filmIdImdb);
                }
            }

            return idImdbId;
        });

        var codeTagTask = Task.Run(() =>
        {
            var pathTagCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\TagCodes_MovieLens.csv";

            var codeTagTag = new Dictionary<string, string>();
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
                    
                    codeTagTag[codeTag] = tag;
                }
            }

            return codeTagTag;
        });

        var tagsTask = Task.Run(() =>
        {
            var idImdbId = linksIdTask.Result;
            var codeTagTag = codeTagTask.Result;

            var pathTagScores = @"C:\Users\s-khechnev\Desktop\ml-latest\TagScores_MovieLens.csv";

            var filmIdTags = new Dictionary<string, List<string>>();
            using (var fs = new FileStream(pathTagScores, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024,
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

                    if (filmIdTags.ContainsKey(movieId))
                    {
                        filmIdTags[idImdbId[movieId]].Add(codeTagTag[tagId]);
                    }
                    else
                    {
                        filmIdTags[idImdbId[movieId]] = new List<string>() { codeTagTag[tagId] };
                    }
                }
            }

            return filmIdTags;
        });

        _filmIdTags = tagsTask.Result;
        actorsTask.Wait();
        //ans

        var ans1 = new Dictionary<string, Movie>();
        foreach (var filmId in _filmIdFilmTitles.Keys)
        {
            foreach (var filmTitle in _filmIdFilmTitles[filmId])
            {
                if (ans1.ContainsKey(filmTitle))
                    continue;
                ans1.Add(filmTitle, GetMovieByTitleAndFilmId(filmTitle, filmId));
            }
        }

        var ansTask2 = Task.Run(() =>
        {
            var ans2 = new Dictionary<string, HashSet<Movie>>();
            foreach (var filmId in _filmIdCategoryActors.Keys)
            {
                foreach (var keyPersonCategory in _filmIdCategoryActors[filmId].Keys)
                {
                    foreach (var personName in _filmIdCategoryActors[filmId][keyPersonCategory])
                    {
                        if (ans2.ContainsKey(personName))
                            ans2[personName].Add(ans1[_filmIdFilmTitles[filmId].First()]);
                        else
                        {
                            ans2[personName] = new HashSet<Movie>();
                            ans2[personName].Add(ans1[_filmIdFilmTitles[filmId].First()]);
                        }
                    }
                }
            }

            return ans2;
        });

        var ansTask3 = Task.Run(() =>
        {
            var ans3 = new Dictionary<string, HashSet<Movie>>();

            foreach (var filmId in _filmIdFilmTitles.Keys)
            {
                if (!_filmIdTags.ContainsKey(filmId))
                    continue;

                foreach (var tag in _filmIdTags[filmId])
                {
                    if (ans3.ContainsKey(tag))
                        ans3[tag].Add(ans1[_filmIdFilmTitles[filmId].First()]);
                    else
                    {
                        ans3[tag] = new HashSet<Movie>();
                        ans3[tag].Add(ans1[_filmIdFilmTitles[filmId].First()]);
                    }
                }
            }

            return ans3;
        });

        var ans2 = ansTask2.Result;
        var ans3 = ansTask3.Result;

        stopwatch.Stop();

        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine(elapsedTime);
        Console.WriteLine("Complete");

        while (true)
        {
            Console.WriteLine("Введите название фильма или имя актера или режиссера или тег");
            string? line = Console.ReadLine();

            if (string.IsNullOrEmpty(line))
                continue;

            if (ans1.TryGetValue(line, out var movie))
                Console.WriteLine(movie);
            if (ans2.TryGetValue(line, out var movieSet1))
                movieSet1.ToList().ForEach(Console.WriteLine);
            if (ans3.TryGetValue(line, out var movieSet2))
                movieSet2.ToList().ForEach(Console.WriteLine);

            Console.WriteLine("\n");
        }
    }
}