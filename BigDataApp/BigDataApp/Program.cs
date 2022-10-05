using BigDataApp;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

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

    //25.31 release split // 18.14 release without split
    static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string pathMovieCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\MovieCodes_IMDB.tsv";
        //134056
        /*var filmId_filmTitles = File.ReadAllLines(pathMovieCodes).AsParallel().Skip(1)
                        .Select(line => line.Split('\t'))
                        .Where(item => item[4] == "ru" || item[4] == "en")
                        .GroupBy(x => x[0])
                        .ToDictionary(x => x.Key, x => x.Select(x => x[2]).ToList());
        */

        var filmId_filmTitles = new Dictionary<string, List<string>>();

        using (var fs = new FileStream(pathMovieCodes, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
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
                    if (filmId_filmTitles.ContainsKey(filmId))
                        filmId_filmTitles[filmId].Add(filmTitle);
                    else
                        filmId_filmTitles[filmId] = new List<string> { filmTitle };
                }

                /*var s = line.Split('\t');
                if (s[4] == "en" || s[4] == "ru")
                {
                    if (filmId_filmTitles.ContainsKey(s[0]))
                        filmId_filmTitles[s[0]].Add(s[2]);
                    else
                        filmId_filmTitles[s[0]] = new List<string> { s[2] };
                }*/
            }
        }


        var actorsTask = Task.Factory.StartNew(() =>
        {
            string pathActorsDirectorsNames = @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsNames_IMDB.txt";
            string pathActorsDirectorsCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsCodes_IMDB.tsv";

            /*var personId_personName = File.ReadAllLines(pathActorsDirectorsNames).AsParallel().Skip(1)
                .Select(line => line.Split('\t'))
                .ToDictionary(x => x[0], x => x[1]);*/
            var personId_personName = new Dictionary<string, string>();
            using (var fs = new FileStream(pathActorsDirectorsNames, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    int index;
                    index = lineSpan.IndexOf('\t');
                    var personId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var personName = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    personId_personName[personId] = personName;
                }
            }


            //131467 48sec 42 sec
            /*var filmdId_category_actors = File.ReadAllLines(pathActorsDirectorsCodes).AsParallel().Skip(1)
                .Select(x => x.Split('\t'))
                .Where(x => filmId_filmTitles.ContainsKey(x[0]))
                .GroupBy(x => x[0])
                .ToDictionary(x => x.Key, x => x.GroupBy(x => x[3]).ToDictionary(x => x.Key, x => x.Select(z => personId_personName.ContainsKey(z[2]) ? personId_personName[z[2]] : z[2])
                                  .ToList()));*/
            var filmId_category_actors = new Dictionary<string, Dictionary<string, List<string>>>();

            using (var fs = new FileStream(pathActorsDirectorsCodes, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
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
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var personId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf('\t');
                    var category = lineSpan.Slice(0, index).ToString();

                    if (!filmId_filmTitles.ContainsKey(filmId))
                        continue;

                    var personName = personId_personName.ContainsKey(personId) ? personId_personName[personId] : personId;

                    if (filmId_category_actors.ContainsKey(filmId))
                    {
                        if (filmId_category_actors[filmId].ContainsKey(category))
                        {
                            filmId_category_actors[filmId][category].Add(personName);
                        }
                        else
                        {
                            filmId_category_actors[filmId][category] = new List<string>() { personName };
                        }
                    }
                    else
                    {
                        filmId_category_actors[filmId] = new Dictionary<string, List<string>>();
                        filmId_category_actors[filmId][category] = new List<string>() { personName };
                    }
                }
            }

            return filmId_category_actors;
        }, TaskCreationOptions.LongRunning);

        var raitingTask = Task.Run(() =>
        {
            string pathRating = @"C:\Users\s-khechnev\Desktop\ml-latest\Ratings_IMDB.tsv";
            /*var raitingDict = File.ReadAllLines(pathRating).AsParallel().Skip(1).Select(line => line.Split('\t'))
                                        .Where(x => filmId_filmTitles.ContainsKey(x[0]))
                                        .ToDictionary(x => x[0], x => x[1]);*/

            var raitingDict = new Dictionary<string, string>();
            using (var fs = new FileStream(pathRating, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
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

                    if (!filmId_filmTitles.ContainsKey(filmId))
                        continue;

                    raitingDict[filmId] = rait;
                }
            }

            return raitingDict;
        });

        var linksIdTask = Task.Factory.StartNew(() =>
        {
            var pathLinks = @"C:\Users\s-khechnev\Desktop\ml-latest\links_IMDB_MovieLens.csv";
            /*var id_imdbId = File.ReadAllLines(pathLinks).AsParallel().Skip(1).Select(line => line.Split(','))
                                                           .ToDictionary(x => x[0], x => string.Concat("tt", x[1]));*/

            var id_imdbId = new Dictionary<string, string>();
            using (var fs = new FileStream(pathLinks, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    int index;
                    index = lineSpan.IndexOf(',');
                    var filmId = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    index = lineSpan.IndexOf(',');
                    var filmIdImdb = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    id_imdbId[filmId] = string.Concat("tt", filmIdImdb);
                }
            }

            return id_imdbId;
        }, TaskCreationOptions.LongRunning);

        var codeTagTask = Task.Factory.StartNew(() =>
        {
            var pathTagCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\TagCodes_MovieLens.csv";
            /*var codeTag_Tag = File.ReadAllLines(pathTagCodes).AsParallel().Skip(1).Select(line => line.Split(','))
                                                                .ToDictionary(x => x[0], x => x[1]);*/

            var codeTag_Tag = new Dictionary<string, string>();
            using (var fs = new FileStream(pathTagCodes, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var lineSpan = line.AsSpan();

                    int index;
                    index = lineSpan.IndexOf(',');
                    var codeTag = lineSpan.Slice(0, index).ToString();
                    lineSpan = lineSpan.Slice(index + 1);

                    var tag = lineSpan.ToString();

                    codeTag_Tag[codeTag] = tag;
                }
            }

            return codeTag_Tag;
        }, TaskCreationOptions.LongRunning);

        var tagsTask = Task.Factory.StartNew(() =>
        {
            var id_imdbId = linksIdTask.Result;
            var codeTag_Tag = codeTagTask.Result;

            var pathTagScores = @"C:\Users\s-khechnev\Desktop\ml-latest\TagScores_MovieLens.csv";

            //7013
            /*var filmId_tags = File.ReadAllLines(pathTagScores).AsParallel().Skip(1)
                                .Select(line => line.Split(','))
                                .Where(x => filmId_filmTitles.ContainsKey(id_imdbId[x[0]]) && float.Parse(x[2], CultureInfo.InvariantCulture.NumberFormat) > 0.5f)
                                .GroupBy(x => x[0])
                                .ToDictionary(x => id_imdbId[x.Key], x => x.Select(x => codeTag_Tag[x[1]]).ToList());*/

            var filmId_tags = new Dictionary<string, List<string>>();
            using (var fs = new FileStream(pathTagScores, FileMode.Open, FileAccess.Read, FileShare.None, 64 * 1024, FileOptions.SequentialScan))
            using (var reader = new StreamReader(fs))
            {
                reader.ReadLine();
                string line;
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

                    if (!filmId_filmTitles.ContainsKey(id_imdbId[movieId]) || !(float.Parse(relevance, CultureInfo.InvariantCulture.NumberFormat) > 0.5f))
                        continue;

                    if (filmId_tags.ContainsKey(movieId))
                    {
                        filmId_tags[movieId].Add(codeTag_Tag[tagId]);
                    }
                    else
                    {
                        filmId_tags[movieId] = new List<string>() { codeTag_Tag[tagId] };
                    }
                }
            }

            return filmId_tags;
        }, TaskCreationOptions.LongRunning);

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

        var ansTask2 = Task.Run(() =>
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

        var ansTask3 = Task.Run(() =>
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
