using BigDataApp;
using System.Globalization;

class Program
{
    static Dictionary<string, Dictionary<string, List<string>>> filmdId_category_actors;
    static Dictionary<string, string> raitingDict;
    static Dictionary<string, List<string>> filmId_tags;

    static Movie GetMovieByTitleAndFilmId(string filmTitle, string filmId)
    {
        var title = filmTitle;
        string raiting = String.Empty;
        HashSet<string> directors = null;
        HashSet<string> tags = null;
        HashSet<string> actors = null;

        if (raitingDict.ContainsKey(filmId))
        {
            raiting = raitingDict[filmId];
        }
        if (filmdId_category_actors.ContainsKey(filmId))
        {
            ///cringe
            if (filmdId_category_actors[filmId].ContainsKey("actor"))
            {
                if (actors != null)
                    actors.Union(filmdId_category_actors[filmId]["actor"].ToHashSet());
                else
                    actors = filmdId_category_actors[filmId]["actor"].ToHashSet();
            }

            if (filmdId_category_actors[filmId].ContainsKey("actress"))
            {
                if (actors != null)
                    actors.Union(filmdId_category_actors[filmId]["actress"].ToHashSet());
                else
                    actors = filmdId_category_actors[filmId]["actress"].ToHashSet();
            }

            if (filmdId_category_actors[filmId].ContainsKey("self"))
            {
                if (actors != null)
                    actors.Union(filmdId_category_actors[filmId]["self"].ToHashSet());
                else
                    actors = filmdId_category_actors[filmId]["self"].ToHashSet();
            }
            ///
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


    static void Main(string[] args)
    {
        string pathMovieCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\MovieCodes_IMDB.tsv";
        var filmId_filmTitles = File.ReadAllLines(pathMovieCodes).Skip(1)
                        .Select(line => line.Split('\t'))
                        .GroupBy(item => item[0])
                        .Select(group => new { Id = group.Key, FilmsInfo = group.Select(filmLine => new { Title = filmLine[2], Language = filmLine[4] }) })
                        .Where(x => x.FilmsInfo.ToList().Where(y => y.Language == "ru" || y.Language == "en").Any())
                        .ToDictionary(x => x.Id, x => x.FilmsInfo.Where(x => x.Language == "ru" || x.Language == "en").Select(x => x.Title).Distinct().ToList());

        var t1 = Task.Factory.StartNew(() =>
        {
            string pathActorsDirectorsNames = @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsNames_IMDB.txt";
            string pathActorsDirectorsCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsCodes_IMDB.tsv";

            var personId_personName = File.ReadAllLines(pathActorsDirectorsNames).Skip(1)
                .Select(line => line.Split('\t'))
                .ToDictionary(x => x[0], x => x[1]);

            var filmdId_category_actors = File.ReadAllLines(pathActorsDirectorsCodes).Skip(1)
                .Select(x => x.Split('\t'))
                .Where(x => filmId_filmTitles.ContainsKey(x[0]))
                .GroupBy(x => new { FilmId = x[0] })
                .Select(group => new { Key = group.Key, CategoryGroup = group.GroupBy(x => x[3]) })
                .ToDictionary(x => x.Key.FilmId,
                              x => x.CategoryGroup.ToDictionary(y => y.Key, y =>
                                  y.Select(z => personId_personName.ContainsKey(z[2]) ? personId_personName[z[2]] : z[2])
                                  .ToList()
                              ));

            return filmdId_category_actors;
        });

        var t2 = Task.Factory.StartNew(() =>
        {
            string pathRating = @"C:\Users\s-khechnev\Desktop\ml-latest\Ratings_IMDB.tsv";

            var raitingDict = File.ReadAllLines(pathRating).Skip(1).Select(line => line.Split('\t'))
                                                    .Where(x => filmId_filmTitles.ContainsKey(x[0]))
                                                    .ToDictionary(x => x[0], x => x[1]);

            var pathLinks = @"C:\Users\s-khechnev\Desktop\ml-latest\links_IMDB_MovieLens.csv";
            var id_imdbId = File.ReadAllLines(pathLinks).Skip(1).Select(line => line.Split(','))
                                                           .ToDictionary(x => x[0], x => "tt" + x[1]);

            var pathTagCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\TagCodes_MovieLens.csv";
            var codeTag_Tag = File.ReadAllLines(pathTagCodes).Skip(1).Select(line => line.Split(','))
                                                                .ToDictionary(x => x[0], x => x[1]);

            var pathTagScores = @"C:\Users\s-khechnev\Desktop\ml-latest\TagScores_MovieLens.csv";

            var filmId_tags = File.ReadAllLines(pathTagScores).Skip(1)
                                .Select(line => line.Split(','))
                                .Where(x => filmId_filmTitles.ContainsKey(id_imdbId[x[0]]) && float.Parse(x[2], CultureInfo.InvariantCulture.NumberFormat) > 0.5f)
                                .GroupBy(x => x[0], x => x[1],
                                            (key, g) => new { Id = id_imdbId[key], Tags = g.Select(x => codeTag_Tag[x]).ToList() })
                                .ToDictionary(x => x.Id, x => x.Tags);

            return Tuple.Create(raitingDict, filmId_tags);
        });

        //ans

        filmdId_category_actors = t1.Result;
        (raitingDict, filmId_tags) = t2.Result;

        var ansTask1 = Task.Factory.StartNew(() =>
        {
            var ans1 = new Dictionary<string, Movie>();
            foreach (var filmId in filmId_filmTitles.Keys)
            {
                foreach (var filmTitle in filmId_filmTitles[filmId])
                {
                    if (ans1.ContainsKey(filmTitle))//cringe
                        continue;
                    ans1.Add(filmTitle, GetMovieByTitleAndFilmId(filmTitle, filmId));
                }
            }

            return ans1;
        });

        var ans1 = ansTask1.Result;

        var ansTask2 = Task.Factory.StartNew(() =>
        {
            var ans2 = new Dictionary<string, HashSet<Movie>>();
            foreach (var filmId in filmdId_category_actors.Keys)
            {
                foreach (var keyPersonCategory in filmdId_category_actors[filmId].Keys.Where(x => x != "writer"))
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
    }
}
