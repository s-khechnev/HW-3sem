using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BigDataApp.Entities;

namespace BigDataApp;

public static class MyParser
{
    public static Dictionary<string, Dictionary<string, List<string>>> _filmIdCategoryPersonsIds = new();
    private static Dictionary<string, List<string>> _filmIdPersonsId = new();

    private static Dictionary<string, float> _filmIdRating = new();
    private static Dictionary<string, List<string>> _filmIdTagsNames = new();

    private static Dictionary<string, string> _filmTitleFilmId = new();

    private static Dictionary<string, List<string>> _filmIdFilmTitles = new();

    private static Dictionary<string, HashSet<string>> _directorIdFilmsIds = new();
    private static Dictionary<string, HashSet<string>> _actorIdFilmsIds = new();
    private static Dictionary<string, HashSet<string>> _tagFilmsIds = new();

    public static Dictionary<string, Movie> _filmIdMovie = new();
    public static Dictionary<string, Tag> _tagNameTag = new();
    public static Dictionary<string, Person> _personIdPerson = new();
    public static List<Title> Titles = new();

    private static int personID = 1;

    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    public static void Run()
    {
        Console.WriteLine("Parsing...");
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        const string movieCodesPath = @"C:\Users\s-khechnev\Desktop\ml-latest\MovieCodes_IMDB.tsv";

        var filmIdTask = Task.Run(() =>
        {
            int movieId = 1;
            int titleId = 1;

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
                    var region = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var lang = lineSpan.Slice(0, index).ToString();

                    index = lineSpan.IndexOf('\t');
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var t = lineSpan.Slice(0, index).ToString();

                    index = lineSpan.IndexOf('\t');
                    lineSpan = lineSpan.Slice(index + 1);

                    var isOriginalTitleString = lineSpan.ToString();

                    var isOriginalTitle = isOriginalTitleString == "1";

                    if (lang == "en" || lang == "ru" || region == "US" || region == "RU")
                    {
                        if (!_filmIdMovie.ContainsKey(filmId))
                        {
                            var movie = new Movie();
                            movie.Id = movieId;
                            movieId++;
                            movie.Titles = new List<Title>();
                            movie.OriginalTitle = filmTitle;

                            _filmIdMovie.Add(filmId, movie);
                            /*lock (context)
                            {
                                context.Add(movie);    
                            }*/
                        }

                        if (_filmIdFilmTitles.ContainsKey(filmId))
                        {
                            _filmIdFilmTitles[filmId].Add(filmTitle);
                        }
                        else
                        {
                            _filmIdFilmTitles[filmId] = new List<string> { filmTitle };
                        }

                        var title = new Title() { Id = titleId, Name = filmTitle, MovieId = _filmIdMovie[filmId].Id };
                        titleId++;
                        Titles.Add(title);
                        _filmIdMovie[filmId].Titles.Add(title);

                        if (!_filmTitleFilmId.ContainsKey(filmTitle))
                        {
                            _filmTitleFilmId.Add(filmTitle, filmId);
                        }

                        if (isOriginalTitle)
                        {
                            _filmIdMovie[filmId].OriginalTitle = filmTitle;
                        }
                    }
                }
            }
        });

        var actorsTask = Task.Run(() =>
        {
            const string pathActorsDirectorsNames =
                @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsNames_IMDB.txt";

            var id = 1;

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

                    var person = new Person() { Name = personName };
                    person.Id = MyParser.personID;
                    MyParser.personID++;
                    _personIdPerson[personId] = person;
                    /*lock (context)
                    {
                        context.Add(person);    
                    }*/
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

                    if (!_filmIdFilmTitles.ContainsKey(filmId))
                        continue;

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

                    if (_filmIdCategoryPersonsIds.ContainsKey(filmId))
                    {
                        if (_filmIdCategoryPersonsIds[filmId].ContainsKey(category))
                        {
                            _filmIdCategoryPersonsIds[filmId][category].Add(personId);
                        }
                        else
                        {
                            _filmIdCategoryPersonsIds[filmId][category] = new List<string>() { personId };
                        }

                        _filmIdPersonsId[filmId].Add(personId);
                    }
                    else
                    {
                        _filmIdCategoryPersonsIds[filmId] = new Dictionary<string, List<string>>();
                        _filmIdCategoryPersonsIds[filmId][category] = new List<string>() { personId };

                        _filmIdPersonsId.Add(filmId, new List<string>() { personId });
                    }
                }
            }
        });

        actorsTask.Wait();
        var t = 1;


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
                    var rating = lineSpan.Slice(0, index).ToString();

                    if (!filmIdTask.IsCompleted)
                    {
                        filmIdTask.Wait();
                    }

                    if (!_filmIdFilmTitles.ContainsKey(filmId))
                        continue;

                    _filmIdRating[filmId] = float.Parse(rating, CultureInfo.InvariantCulture.NumberFormat);
                    ;
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

            var id = 1;

            var codeTagTagName = new Dictionary<string, string>();
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

                    var tagName = lineSpan.ToString();

                    codeTagTagName[codeTag] = tagName;
                    _tagNameTag[tagName] = new Tag() { Name = tagName, Id = id };
                    id++;
                }
            }

            return codeTagTagName;
        });


        var tagsTask = Task.Run(() =>
        {
            var idImdbId = linksIdTask.Result;
            var codeTagTagName = codeTagTask.Result;

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

                    var filmId = idImdbId[movieId];

                    if (!_filmIdFilmTitles.ContainsKey(filmId) ||
                        !(float.Parse(relevance, CultureInfo.InvariantCulture.NumberFormat) > 0.5f))
                        continue;

                    if (_filmIdTagsNames.ContainsKey(filmId))
                    {
                        _filmIdTagsNames[filmId].Add(codeTagTagName[tagId]);
                    }
                    else
                    {
                        _filmIdTagsNames[filmId] = new List<string>() { codeTagTagName[tagId] };
                    }
                }
            }
        });

        Task.WaitAll(actorsTask, ratingTask, tagsTask, codeTagTask, filmIdTask, linksIdTask);

        //personId -> filmIds

        var personFilmsTask = Task.Run(() =>
        {
            foreach (var filmId in _filmIdCategoryPersonsIds.Keys)
            {
                foreach (var category in _filmIdCategoryPersonsIds[filmId].Keys)
                {
                    foreach (var personId in _filmIdCategoryPersonsIds[filmId][category])
                    {
                        if (category == "actor")
                        {
                            if (_actorIdFilmsIds.ContainsKey(personId))
                            {
                                _actorIdFilmsIds[personId].Add(filmId);
                            }
                            else
                            {
                                _actorIdFilmsIds.Add(personId, new HashSet<string>() { filmId });
                            }
                        }

                        if (category == "director")
                        {
                            if (_directorIdFilmsIds.ContainsKey(personId))
                            {
                                _directorIdFilmsIds[personId].Add(filmId);
                            }
                            else
                            {
                                _directorIdFilmsIds.Add(personId, new HashSet<string>() { filmId });
                            }
                        }
                    }
                }
            }
        });

        //filmId -> tags, tag -> filmIds

        var tagFilmIdsTask = Task.Run(() =>
        {
            foreach (var filmId in _filmIdFilmTitles.Keys)
            {
                if (!_filmIdTagsNames.ContainsKey(filmId))
                    continue;

                foreach (var tag in _filmIdTagsNames[filmId])
                {
                    if (_tagFilmsIds.ContainsKey(tag))
                    {
                        _tagFilmsIds[tag].Add(filmId);
                    }
                    else
                    {
                        _tagFilmsIds.Add(tag, new HashSet<string>() { filmId });
                    }
                }
            }
        });

        Task.WaitAll(personFilmsTask, tagFilmIdsTask);

        //top10
        var count = 0;

        Console.WriteLine("init top10");

        Dictionary<string, List<string>> filmIdTopsIds = new();

        Parallel.ForEach(_filmIdFilmTitles.Keys, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, filmId =>
        {
            //var candidatesSet = new HashSet<string>();

            Dictionary<float, HashSet<string>> estimationFilmsIds = new();
            var addedFilmsIds = new HashSet<string>();

            List<string> persons = null;
            List<string> tags = null;

            if (_filmIdPersonsId.ContainsKey(filmId))
                persons = _filmIdPersonsId[filmId];
            if (_filmIdTagsNames.ContainsKey(filmId))
                tags = _filmIdTagsNames[filmId];

            if (persons != null)
            {
                foreach (var personId in persons)
                {
                    if (_actorIdFilmsIds.ContainsKey(personId))
                    {
                        foreach (var movieId in _actorIdFilmsIds[personId])
                        {
                            if (addedFilmsIds.Contains(movieId) || movieId == filmId)
                                continue;

                            var estimation = GetEstimation(filmId, movieId);

                            if (estimationFilmsIds.ContainsKey(estimation))
                                estimationFilmsIds[estimation].Add(movieId);
                            else
                                estimationFilmsIds.Add(estimation, new HashSet<string>() { movieId });

                            addedFilmsIds.Add(movieId);
                        }
                    }

                    if (_directorIdFilmsIds.ContainsKey(personId))
                    {
                        foreach (var movieId in _directorIdFilmsIds[personId])
                        {
                            if (addedFilmsIds.Contains(movieId) || movieId == filmId)
                                continue;

                            var estimation = GetEstimation(filmId, movieId);

                            if (estimationFilmsIds.ContainsKey(estimation))
                                estimationFilmsIds[estimation].Add(movieId);
                            else
                                estimationFilmsIds.Add(estimation, new HashSet<string>() { movieId });

                            addedFilmsIds.Add(movieId);
                        }
                    }
                }
            }

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    if (_tagFilmsIds.ContainsKey(tag))
                    {
                        foreach (var movieId in _tagFilmsIds[tag])
                        {
                            if (addedFilmsIds.Contains(movieId) || movieId == filmId)
                                continue;

                            var estimation = GetEstimation(filmId, movieId);

                            if (estimationFilmsIds.ContainsKey(estimation))
                                estimationFilmsIds[estimation].Add(movieId);
                            else
                                estimationFilmsIds.Add(estimation, new HashSet<string>() { movieId });

                            addedFilmsIds.Add(movieId);
                        }
                    }
                }
            }

            int k = 0;

            if (estimationFilmsIds.Keys.Count == 0)
                return;

            lock (filmIdTopsIds)
            {
                filmIdTopsIds.Add(filmId, new List<string>());    
            }
            
            var orderedDict = estimationFilmsIds.OrderByDescending(x => x.Key);
            foreach (var estMovie in orderedDict)
            {
                foreach (var film in estMovie.Value)
                {
                    lock (filmIdTopsIds)
                    {
                        filmIdTopsIds[filmId].Add(film);    
                    }
                    k++;
                    if (k == 10)
                        break;
                }

                if (k == 10)
                    break;
            }

            count++;
            if (count % 100000 == 0)
            {
                Console.WriteLine(count);
            }
        });

        Console.WriteLine("end init top 10");

        stopwatch.Stop();

        TimeSpan ts = stopwatch.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine(elapsedTime);
        Console.WriteLine("Complete parsing");

        Console.WriteLine("Creating entities");

        //init movie

        stopwatch.Restart();
        Parallel.ForEach(_filmIdFilmTitles.Keys, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, filmId =>
        {
            var movie = _filmIdMovie[filmId];
            //movie.Title = filmTitle;

            if (_filmIdCategoryPersonsIds.ContainsKey(filmId))
            {
                movie.Persons = new HashSet<Person>();
                foreach (var categoryPersonsId in _filmIdCategoryPersonsIds[filmId])
                {
                    foreach (var personId in _filmIdCategoryPersonsIds[filmId][categoryPersonsId.Key])
                    {
                        Person? person;
                        lock (movie)
                        {
                            if (!_personIdPerson.ContainsKey(personId))
                            {
                                person = new Person() { Name = personId };
                                person.Id = MyParser.personID;
                                MyParser.personID++;
                                _personIdPerson.Add(personId, person);
                            }
                        }

                        lock (movie)
                        {
                            person = _personIdPerson[personId];
                            person.Category = categoryPersonsId.Key;
                            movie.Persons.Add(_personIdPerson[personId]);
                        }
                        //context.Update(person);
                    }
                }
            }

            if (_filmIdTagsNames.ContainsKey(filmId))
            {
                movie.Tags = new HashSet<Tag>();
                foreach (var tagName in _filmIdTagsNames[filmId])
                {
                    movie.Tags.Add(_tagNameTag[tagName]);
                }
            }

            if (_filmIdRating.ContainsKey(filmId))
                movie.Rating = _filmIdRating[filmId];

            if (filmIdTopsIds.ContainsKey(filmId))
            {
                movie.Top = new List<Movie>();
                foreach (var topFilmId in filmIdTopsIds[filmId])
                {
                    movie.Top.Add(_filmIdMovie[topFilmId]);
                }
            }
        });

        //init persons
        Parallel.ForEach(_filmIdCategoryPersonsIds.Keys, new ParallelOptions() { MaxDegreeOfParallelism = -1 },
            filmId =>
            {
                foreach (var keyPersonCategory in _filmIdCategoryPersonsIds[filmId].Keys)
                {
                    foreach (var personId in _filmIdCategoryPersonsIds[filmId][keyPersonCategory])
                    {
                        Person? person;
                        lock (filmId)
                        {
                            if (!_personIdPerson.ContainsKey(personId))
                            {
                                person = new Person() { Name = personId };
                                person.Id = MyParser.personID;
                                MyParser.personID++;
                                _personIdPerson.Add(personId, person);
                            }
                        }

                        lock (filmId)
                        {
                            person = _personIdPerson[personId];
                        }

                        if (keyPersonCategory == "director")
                        {
                            lock (person)
                            {
                                person.Category = "director";
                            }

                            if (person.Movies != null)
                            {
                                foreach (var tFilmId in _directorIdFilmsIds[personId])
                                {
                                    lock (person)
                                    {
                                        person.Movies.Add(_filmIdMovie[tFilmId]);
                                    }
                                }
                            }
                            else
                            {
                                lock (person)
                                {
                                    person.Movies = new HashSet<Movie>();
                                }

                                foreach (var tFilmId in _directorIdFilmsIds[personId])
                                {
                                    lock (person)
                                    {
                                        person.Movies.Add(_filmIdMovie[tFilmId]);
                                    }
                                }
                            }
                        }
                        else if (keyPersonCategory == "actor")
                        {
                            lock (person)
                            {
                                person.Category = "actor";
                            }

                            if (person.Movies != null)
                            {
                                foreach (var tFilmId in _actorIdFilmsIds[personId])
                                {
                                    lock (person)
                                    {
                                        person.Movies.Add(_filmIdMovie[tFilmId]);
                                    }
                                }
                            }
                            else
                            {
                                lock (person)
                                {
                                    person.Movies = new HashSet<Movie>();
                                }

                                foreach (var tFilmId in _actorIdFilmsIds[personId])
                                {
                                    lock (person)
                                    {
                                        person.Movies.Add(_filmIdMovie[tFilmId]);
                                    }
                                }
                            }
                        }
                    }
                }
            });

        //init tags
        Parallel.ForEach(_filmIdTagsNames.Keys, filmId =>
        {
            var movie = _filmIdMovie[filmId];

            movie.Tags = new HashSet<Tag>();

            foreach (var tagName in _filmIdTagsNames[filmId])
            {
                movie.Tags.Add(_tagNameTag[tagName]);
            }
        });

        ts = stopwatch.Elapsed;
        elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        Console.WriteLine("End creating etities - " + elapsedTime);
        Console.WriteLine("End parsing");
    }


    private static float GetEstimation(string curFilmId, string otherFilmId)
    {
        float personsEstimation;
        float tagsEstimation;
        float rating;

        if (_filmIdPersonsId.TryGetValue(curFilmId, out var curPersons)
            && _filmIdPersonsId.TryGetValue(otherFilmId, out var otherPersons))
        {
            var intersectionPersonCount = curPersons.Intersect(otherPersons).Count();
            personsEstimation = (float)intersectionPersonCount / (4 * curPersons.Count);
        }
        else
        {
            personsEstimation = 0f;
        }

        if (_filmIdTagsNames.TryGetValue(curFilmId, out var curTags)
            && _filmIdTagsNames.TryGetValue(otherFilmId, out var otherTags))
        {
            var intersectionTagsCount = curTags.Intersect(otherTags).Count();
            tagsEstimation = (float)intersectionTagsCount / (4 * curTags.Count);
        }
        else
        {
            tagsEstimation = 0f;
        }

        if (personsEstimation == 0f && tagsEstimation == 0f)
            return -1f;

        if (!_filmIdRating.TryGetValue(otherFilmId, out rating))
            rating = 0;

        return personsEstimation + tagsEstimation + rating / 20;
    }
}