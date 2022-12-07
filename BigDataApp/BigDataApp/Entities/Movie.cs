using System.Text;

namespace BigDataApp.Entities
{
    public sealed class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public HashSet<Person>? Persons { get; set; }
        public HashSet<Tag>? Tags { get; set; }
        public float Rating { get; set; }
        public List<Movie>? Top { get; set; }
        public List<Movie> Movies { get; set; }

        public float GetEstimation(Movie other)
        {
            float personsEstimation;
            float tagsEstimation;

            if (other.Persons != null && Persons != null)
            {
                var intersectionPersonCount = Persons.Intersect(other.Persons).Count();
                personsEstimation = (float)intersectionPersonCount / (4 * Persons.Count);
            }
            else
            {
                personsEstimation = 0f;
            }

            if (other.Tags != null && Tags != null)
            {
                var intersectionTagsCount = Tags.Intersect(other.Tags).Count();
                tagsEstimation = (float)intersectionTagsCount / (4 * Tags.Count);
            }
            else
            {
                tagsEstimation = 0f;
            }

            if (personsEstimation == 0f && tagsEstimation == 0f)
                return -1f;

            return personsEstimation + tagsEstimation + other.Rating / 20;
        }

        public override string ToString()
        {
            var builder = new StringBuilder($"Title: {Title}\n");

            if (Persons != null && Persons.Count != 0)
            {
                foreach (var person in Persons)
                {
                    builder.Append(person.Name + " = " + person.Category + '\n');
                }
            }

            if (Tags != null && Tags.Count != 0)
            {
                builder.Append("Tags:\n");
                Tags.ToList().ForEach(x => builder.Append(string.Concat(x, "\n")));
            }
            else
            {
                builder.Append("Tags: no information available\n");
            }

            builder.Append($"Rating:");
            if (Rating != -1)
            {
                builder.Append(Rating);
                builder.Append('\n');
            }
            else
            {
                builder.Append("no information available\n");
            }

            if (Top != null)
            {
                builder.Append("Top10:\n");
                var k = 1;
                Top.Sort((a, b) => GetEstimation(b).CompareTo(GetEstimation(a)));
                foreach (var movie in Top)
                {
                    var est = GetEstimation(movie);
                    builder.Append($"{k}) {movie.Title} | estimation = {est}\n");
                    k++;
                }
            }

            return builder.ToString();
        }
    }
}