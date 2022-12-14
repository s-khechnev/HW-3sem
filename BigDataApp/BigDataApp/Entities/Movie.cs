using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BigDataApp.Entities
{
    //title -> filmId
    //[Table("movies")]
    public class Movie
    {
        [Key]
        public int Id { get; set; }
        public string IdImdb { get; set; }
        public virtual HashSet<Person>? Persons { get; set; }
        public virtual HashSet<Tag>? Tags { get; set; }
        public float Rating { get; set; }
        public virtual List<Movie>? Top { get; set; }
        public virtual List<Title> Titles { get; set; }

        public string OriginalTitle { get; set; }

        public float GetEstimation(Movie other)
        {
            float personsEstimation;
            float tagsEstimation;

            if (other.Persons != null && Persons != null && other.Persons.Count != 0 && Persons.Count != 0)
            {
                var intersectionPersonCount = Persons.Intersect(other.Persons).Count();
                personsEstimation = (float)intersectionPersonCount / (4 * Persons.Count);
            }
            else
            {
                personsEstimation = 0f;
            }

            if (other.Tags != null && Tags != null && other.Tags.Count != 0 && Tags.Count != 0)
            {
                var intersectionTagsCount = Tags.Intersect(other.Tags).Count();
                tagsEstimation = (float)intersectionTagsCount / (4 * Tags.Count);
            }
            else
            {
                tagsEstimation = 0f;
            }

            return personsEstimation + tagsEstimation + other.Rating / 20;
        }

        public override string ToString()
        {
            var builder = new StringBuilder($"Original Title: {OriginalTitle}\n");

            if (Persons != null && Persons.Count != 0)
            {
                foreach (var person in Persons)
                {
                    builder.Append(person.Name + " = " + person.Category + '\n');
                }
            }

            if (Tags != null && Tags.Count != 0)
            {
                /*builder.Append("Tags:\n");
                Tags.ToList().ForEach(x => builder.Append(string.Concat(x, "\n")));*/

                builder.Append("Tags count: " + Tags.Count + '\n');
            }
            else
            {
                builder.Append("Tags: no information available\n");
            }

            builder.Append("Rating: ");
            if (Rating != -1)
            {
                builder.Append(Rating);
                builder.Append('\n');
            }
            else
            {
                builder.Append("no information available\n");
            }

            if (Top != null && Top.Count != 0)
            {
                builder.Append("Top10:\n");
                var k = 1;
                var sortedTop = Top.OrderByDescending(a => GetEstimation(a));
                foreach (var movie in sortedTop)
                {
                    builder.Append($"{k}) {movie.OriginalTitle} | estimation {GetEstimation(movie)}\n");
                    k++;
                }
            }

            return builder.ToString();
        }
    }
}