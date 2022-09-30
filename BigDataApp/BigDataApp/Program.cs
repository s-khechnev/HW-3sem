using BigDataApp;
using System.Globalization;

class Program
{
    static void Main(string[] args)
    {
        string pathMovieCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\MovieCodes_IMDB.tsv";
        var movieCodesDict = File.ReadAllLines(pathMovieCodes).Skip(1)
                        .Select(line => line.Split('\t'))
                        .GroupBy(item => item[0])
                        .Select(group => new { Id = group.Key, FilmsInfo = group.Select(filmLine => new { Title = filmLine[2], Language = filmLine[4] }) })
                        .Where(x => x.FilmsInfo.ToList().Where(y => y.Language == "ru" || y.Language == "en").Any())
                        .ToDictionary(x => x.Id, x => x.FilmsInfo.Select(x => x.Title).ToList());

        string pathActorsDirectorsNames = @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsNames_IMDB.txt";
        string pathActorsDirectorsCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\ActorsDirectorsCodes_IMDB.tsv";

        var actorsDirectorsNamesDict = File.ReadAllLines(pathActorsDirectorsNames).Skip(1)
            .Select(line => line.Split('\t'))
            .ToDictionary(x => x[0], x => x[1]);

        var actorsDirectorsCodesDict = File.ReadAllLines(pathActorsDirectorsCodes).Skip(1)
            .Select(x => x.Split('\t'))
            .Where(x => movieCodesDict.ContainsKey(x[0]))
            .GroupBy(x => new { FilmId = x[0] })
            .Select(group => new { Key = group.Key, CategoryGroup = group.GroupBy(x => x[3]) })
            .ToDictionary(x => x.Key.FilmId, x => x.CategoryGroup.ToDictionary(y => y.Key, y => y.Select(z => actorsDirectorsNamesDict[z[2]]).ToList()));

        string pathRating = @"C:\Users\s-khechnev\Desktop\ml-latest\Ratings_IMDB.tsv";

        var raitingDict = File.ReadAllLines(pathRating).Skip(1).Select(line => line.Split('\t'))
                                                .ToDictionary(x => x[0], x => x[1]);

        var pathLinks = @"C:\Users\s-khechnev\Desktop\ml-latest\links_IMDB_MovieLens.csv";
        var linksDict = File.ReadAllLines(pathLinks).Skip(1).Select(line => line.Split(','))
                                                       .ToDictionary(x => x[0], x => "tt" + x[1]);

        var pathTagCodes = @"C:\Users\s-khechnev\Desktop\ml-latest\TagCodes_MovieLens.csv";
        var tagCodesDict = File.ReadAllLines(pathTagCodes).Skip(1).Select(line => line.Split(','))
                                                            .ToDictionary(x => x[0], x => x[1]);

        var pathTagScores = @"C:\Users\s-khechnev\Desktop\ml-latest\TagScores_MovieLens.csv";

        var filmTagsDict = File.ReadAllLines(pathTagScores).Skip(1)
                            .Select(line => line.Split(','))
                            .Where(x => float.Parse(x[2], CultureInfo.InvariantCulture.NumberFormat) > 0.5f)
                            .GroupBy(x => x[0], x => x[1],
                                        (key, g) => new { Id = linksDict[key], Tags = g.Select(x => tagCodesDict[x]).ToList() })
                            .ToDictionary(x => x.Id, x => x.Tags);


        //ans

        var ans1 = new Dictionary<string, Movie>();

        foreach (var key in movieCodesDict.Keys)
        {
            foreach (var item in movieCodesDict[key])
            {
                ans1.Add(item,
                    new Movie()
                    {
                        Title = item,
                        Rating = raitingDict[key],
                        Actors = actorsDirectorsCodesDict[key]["actors"].ToHashSet(),
                        Directors = actorsDirectorsCodesDict[key]["director"].ToHashSet(),
                        Tags = filmTagsDict[key].ToHashSet()
                    });
            }
        }
    }
}
