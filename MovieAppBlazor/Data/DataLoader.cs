using System.Diagnostics.CodeAnalysis;
using IMDbApiLib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MovieAppBlazor.Data;

public class DataLoader
{
    private const string ApiKey = "94a4aec7";
    private const string ApiUrl = $"http://www.omdbapi.com/?apikey={ApiKey}";

    private readonly DatabaseContext _context;

    public DataLoader()
    {
        _context = new DatabaseContext();
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
        return await _context.Movies
            .Where(m => m.Id == movieId)
            .Include(m => m.Tags)
            .Include(m => m.Persons)
            .Include(m => m.Top)
            .ThenInclude(m => m.Persons)
            .Include(m => m.Top)
            .ThenInclude(m => m.Tags)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Movie>> GetMoviesByTitleAsync(string title)
    {
        return await _context.Movies
            .Where(movie => movie.Titles.Any(n => n.Name.ToLower() == title.ToLower()))
            .Include(m => m.Persons)
            .Include(m => m.Tags)
            .OrderByDescending(m => m.Rating)
            .ToListAsync();
    }

    public async Task<Person?> GetPersonByIdAsync(int personId)
    {
        return await _context.Persons
            .FirstOrDefaultAsync(person => person.Id == personId);
    }
    
    public async Task<Tag?> GetTagByIdAsync(int tagId)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(tag => tag.Id == tagId);
    }

    public async Task<ImdbMovie?> GetImdbMovieByIdImdbAsync(string idImdb)
    {
        var url = string.Concat(ApiUrl, "&i=", idImdb);

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

                return JsonConvert.DeserializeObject<ImdbMovie>(jsonString);
            }
        }

        return null;
    }

    public async Task<IEnumerable<Movie>?> GetMoviesByTagNameAsync(string tagName)
    {
        return await _context.Movies
            .Where(m => m.Tags.Any(t => t.Name == tagName))
            .Include(m => m.Persons)
            .Include(m => m.Tags)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Movie>?> GetMoviesByPersonNameAsync(string personName)
    {
        return await _context.Movies
            .Include(m => m.Persons)
            .Where(m => m.Persons.Any(p => p.Name == personName))
            .Include(m => m.Tags)
            .ToListAsync();
    }

    public async Task<IEnumerable<Tag>?> GetTagsByName(string inputLine)
    {
        return await _context.Tags
            .Where(t => t.Name.Contains(inputLine))
            .ToListAsync();
    }

    public async Task<IEnumerable<Person>?> GetPersonsByName(string inputLine)
    {
        return await _context.Persons
            .Where(t => t.Name == inputLine)
            .ToListAsync();
    }

    public async Task<string?> GetPersonImageByNameAsync(string name)
    {
        var apiLib = new ApiLib("k_mwrcvcj7");

        var data = await apiLib.SearchNameAsync(name.Replace(" ", "%20"));

        return data.Results.FirstOrDefault()?.Image;
    }
}