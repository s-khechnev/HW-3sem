using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BigDataApp
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public HashSet<Person>? Persons { get; set; }
        public HashSet<Tag>? Tags { get; set; }
        public float Rating { get; set; }

        public List<Movie>? Top
        {
            get
            {
                _topList.Sort((a, b) => GetEstimation(b).CompareTo(GetEstimation(a)));
                return _topList;
            }
            set => _topList = value;
        }

        [NotMapped]
        private List<Movie> _topList = new();

        public float GetEstimation(Movie other)
        {
            float personsEstimation;
            float tagsEstimation;

            if (other.Persons != null && Persons != null)
            {
                HashSet<Person> intersectionPersons = new HashSet<Person>(Persons);
                intersectionPersons.IntersectWith(other.Persons);
                personsEstimation = (float)intersectionPersons.Count / (4 * Persons.Count);
            }
            else
            {
                personsEstimation = 0;
            }

            if (other.Tags != null && Tags != null)
            {
                HashSet<Tag> intersectionTags = new HashSet<Tag>(Tags);
                intersectionTags.IntersectWith(other.Tags);
                tagsEstimation = (float)intersectionTags.Count / (4 * Tags.Count);
            }
            else
            {
                tagsEstimation = 0;
            }

            if (personsEstimation == 0 && tagsEstimation == 0)
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
                foreach (var movie in Top)
                {
                    builder.Append($"{k}) {movie.Title} | estimation = {GetEstimation(movie)}\n");
                    k++;
                }
            }

            return builder.ToString();
        }
    }
}