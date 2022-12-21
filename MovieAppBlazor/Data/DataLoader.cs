using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MovieAppBlazor.Data;

public class DataLoader
{
    private const string ApiKey = "6afd6859";
    private const string ApiUrl = $"http://www.omdbapi.com/?apikey={ApiKey}";
    private const string PosterApiUrl = $"http://img.omdbapi.com/?apikey={ApiKey}";

    private readonly DatabaseContext _context;

    public DataLoader(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Movie>> GetBestMoviesAsync(int countMovies)
    {
        return await _context.Movies
            .AsNoTracking()
            .Where(m => m.Rating > 8.5f)
            .Include(m => m.Tags)
            .Include(m => m.Persons)
            .Where(m => m.Tags.Count != 0 && m.Persons.Count != 0)
            .OrderByDescending(m => m.Rating)
            .Take(countMovies)
            .ToListAsync();
    }

    public async Task<Movie?> GetMovieByIdAsync(int movieId)
    {
        return await _context.Movies.Where(m => m.Id == movieId)
            .Include(m => m.Tags)
            .Include(m => m.Top)
            .AsSplitQuery()
            .Include(m => m.Persons)
            .Include(m => m.Titles)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Movie>> GetMoviesByTitleAsync(string title)
    {
        return await _context.Movies
            .Where(movie => movie.Titles.Any(n => n.Name.ToLower() == title.ToLower()))
            .Include(m => m.Persons)
            .Include(m => m.Tags)
            .ToListAsync();
    }

    public async Task<Person?> GetPersonByIdAsync(int personId)
    {
        return await _context.Persons.FirstOrDefaultAsync(person => person.Id == personId);
    }

    public async Task<Tag?> GetTagByIdAsync(int tagId)
    {
        return await _context.Tags.FirstOrDefaultAsync(tag => tag.Id == tagId);
    }

    public async Task<string?> GetImageByIdImdbAsync(string idImbd)
    {
        var url = string.Concat(ApiUrl, "&i=", idImbd);

        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url)
        };

        using (var response = await client.SendAsync(request))
        {
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                ImdbMovie? movie = JsonConvert.DeserializeObject<ImdbMovie>(jsonString);
                return movie?.Poster;
            }
        }

        return null;
    }
}