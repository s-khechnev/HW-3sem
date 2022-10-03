using BigDataApp;
using System.Diagnostics;
using System.Globalization;

class Program
{
    static Dictionary<string, Dictionary<string, List<string>>> filmdId_category_actors;
    static Dictionary<string, string> raitingDict;
    static Dictionary<string, List<string>> filmId_tags;

    static void Add(string filmId, string category, HashSet<string> actors)
    {
        if (filmdId_category_actors[filmId].ContainsKey(category))
        {
            if (actors != null)
                actors.Union(filmdId_category_actors[filmId][category].ToHashSet());
            else
                actors = filmdId_category_actors[filmId][category].ToHashSet();
        }
    }

    static Movie GetMovieByTitleAndFilmId(string filmTitle, string filmId)
    {
        var title = filmTitle;
        float raiting;
        HashSet<string> directors = null;
        HashSet<string> tags = null;
        HashSet<string> actors = null;

        if (raitingDict.ContainsKey(filmId))
        {
            raiting = float.Parse(raitingDict[filmId], CultureInfo.InvariantCulture.NumberFormat);
        }
        else
        {
            raiting = -1f;
        }

        if (filmdId_category_actors.ContainsKey(filmId))
        {
            Add(filmId, "actor", actors);
            Add(filmId, "actress", actors);
            Add(filmId, "self", actors);
            if (filmdId_category_actors[filmId].ContainsKey("director"))
            {
                directors = filmdId_category_actors[filmId]["director"].ToHashSet();
            }
        }
        if (filmId_tags.ContainsKey(filmId))
        {
            tags = filmId_tags[filmId].ToHashSet();
        }

        return new Movie()
        {
            Title = title,
            Rating = raiting,
            Actors = actors,
            Directors = directors,
            Tags = tags
        };
    }

    //46sec 42 sec
    static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string pathMovieCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\MovieCodes_IMDB.tsv";
        //134056
        var filmId_filmTitles = File.ReadAllLines(pathMovieCodes).Skip(1)
                        .Select(line => line.Split('\t'))
                        .Where(item => item[4] == "ru" || item[4] == "en")
                        .GroupBy(x => x[0])
                        .ToDictionary(x => x.Key, x => x.Select(x => x[2]).ToList());

        var actorsTask = Task.Factory.StartNew(() =>
        {
            string pathActorsDirectorsNames = @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsNames_IMDB.txt";
            string pathActorsDirectorsCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsCodes_IMDB.tsv";

            var personId_personName = File.ReadAllLines(pathActorsDirectorsNames).Skip(1)
                .Select(line => line.Split('\t'))
                .ToDictionary(x => x[0], x => x[1]);

            //131467 48sec 42 sec
            var filmdId_category_actors = File.ReadAllLines(pathActorsDirectorsCodes).Skip(1)
                .Select(x => x.Split('\t'))
                .Where(x => filmId_filmTitles.ContainsKey(x[0]))
                .GroupBy(x => x[0])
                .Select(group => new { idKey = group.Key, CategoryGroupKey = group.GroupBy(x => x[3]) })
                .ToDictionary(x => x.idKey,
                              x => x.CategoryGroupKey.ToDictionary(y => y.Key, y =>
                                  y.Select(z => personId_personName.ContainsKey(z[2]) ? personId_personName[z[2]] : z[2])
                                  .ToList()
                              ));

            return filmdId_category_actors;
        });

        var raitingTask = Task.Factory.StartNew(() =>
        {
            string pathRating = @"C:\Users\s-khechnev\Desktop\ml-latest\Ratings_IMDB.tsv";
            var raitingDict = File.ReadAllLines(pathRating).Skip(1).Select(line => line.Split('\t'))
                                        .Where(x => filmId_filmTitles.ContainsKey(x[0]))
                                        .ToDictionary(x => x[0], x => x[1]);

            return raitingDict;
        });

        var linksIdTask = Task.Factory.StartNew(() =>
        {
            var pathLinks = @"C:\Users\s-khechnev\Desktop\ml-latest\links_IMDB_MovieLens.csv";
            var id_imdbId = File.ReadAllLines(pathLinks).Skip(1).Select(line => line.Split(','))
                                                           .ToDictionary(x => x[0], x => string.Concat("tt", x[1]));
            return id_imdbId;
        });

        var codeTagTask = Task.Factory.StartNew(() =>
        {
            var pathTagCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\TagCodes_MovieLens.csv";
            var codeTag_Tag = File.ReadAllLines(pathTagCodes).Skip(1).Select(line => line.Split(','))
                                                                .ToDictionary(x => x[0], x => x[1]);

            return codeTag_Tag;
        });

        var tagsTask = Task.Factory.StartNew(() =>
        {
            var id_imdbId = linksIdTask.Result;
            var codeTag_Tag = codeTagTask.Result;

            var pathTagScores = @"C:\Users\s-khechnev\Desktop\ml-latest\TagScores_MovieLens.csv";

            var filmId_tags = File.ReadAllLines(pathTagScores).Skip(1)
                                .Select(line => line.Split(','))
                                .Where(x => filmId_filmTitles.ContainsKey(id_imdbId[x[0]]) && float.Parse(x[2], CultureInfo.InvariantCulture.NumberFormat) > 0.5f)
                                .GroupBy(x => x[0], x => x[1],
                                            (key, g) => new { Id = id_imdbId[key], Tags = g.Select(x => codeTag_Tag[x]).ToList() })
                                .ToDictionary(x => x.Id, x => x.Tags);

            return filmId_tags;
        });

        filmdId_category_actors = actorsTask.Result;
        filmId_tags = tagsTask.Result;
        raitingDict = raitingTask.Result;

        //ans

        var ans1 = new Dictionary<string, Movie>();
        foreach (var filmId in filmId_filmTitles.Keys)
        {
            foreach (var filmTitle in filmId_filmTitles[filmId])
            {
                if (ans1.ContainsKey(filmTitle))
                    continue;
                ans1.Add(filmTitle, GetMovieByTitleAndFilmId(filmTitle, filmId));
            }
        }

        var ansTask2 = Task.Factory.StartNew(() =>
        {
            var ans2 = new Dictionary<string, HashSet<Movie>>();
            foreach (var filmId in filmdId_category_actors.Keys)
            {
                foreach (var keyPersonCategory in filmdId_category_actors[filmId].Keys)
                {
                    foreach (var personName in filmdId_category_actors[filmId][keyPersonCategory])
                    {
                        if (ans2.ContainsKey(personName))
                            ans2[personName].Add(ans1[filmId_filmTitles[filmId].First()]);
                        else
                        {
                            ans2[personName] = new HashSet<Movie>();
                            ans2[personName].Add(ans1[filmId_filmTitles[filmId].First()]);
                        }
                    }
                }
            }

            return ans2;
        });

        var ansTask3 = Task.Factory.StartNew(() =>
        {
            var ans3 = new Dictionary<string, HashSet<Movie>>();

            foreach (var filmId in filmId_filmTitles.Keys)
            {
                if (!filmId_tags.ContainsKey(filmId))
                    continue;

                foreach (var tag in filmId_tags[filmId])
                {
                    if (ans3.ContainsKey(tag))
                        ans3[tag].Add(ans1[filmId_filmTitles[filmId].First()]);
                    else
                    {
                        ans3[tag] = new HashSet<Movie>();
                        ans3[tag].Add(ans1[filmId_filmTitles[filmId].First()]);
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
    }
}
