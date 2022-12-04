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
        public HashSet<Movie>? Top { get; set; }

        public float GetEstimation(Movie other)
        {
            float personsEstimation;
            float tagsEstimation;

            if (other.Persons != null && Persons != null)
            {
                HashSet<Person> intersectionPersons = new HashSet<Person>(Persons);
                intersectionPersons.IntersectWith(other.Persons);
                personsEstimation = (float)intersectionPersons.Count / (2 * Persons.Count);
            }
            else
            {
                personsEstimation = 0;
            }

            if (other.Tags != null && Tags != null)
            {
                HashSet<Tag> intersectionTags = new HashSet<Tag>(Tags);
                intersectionTags.IntersectWith(other.Tags);
                tagsEstimation = (float)intersectionTags.Count / (2 * Tags.Count);
            }
            else
            {
                tagsEstimation = 0;
            }

            if (personsEstimation == 0 && tagsEstimation == 0)
                return -1f;

            return personsEstimation + tagsEstimation /* + Rating / 20 */;
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

            /*if (Top10s != null)
            {
                builder.Append("Top10: \n");
                foreach (var top in Top10s)
                {
                    builder.Append(top + '\n');
                }
            }
            else
            {
                builder.Append("Top10: no information available");
            }*/

            if (Top != null)
            {
                builder.Append("Top10:\n");
                foreach (var movie in Top)
                {
                    builder.Append(movie.Title + '\n');
                }
            }

            return builder.ToString();
        }
    }
}