using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BigDataApp
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public HashSet<Person> Persons { get; set; }
        public HashSet<Tag>? Tags { get; set; }
        public float Rating { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder($"Title: {Title}\n");

            if (Persons != null)
            {
                foreach (var person in Persons)
                {
                    builder.Append(person.Name + " = " + person.Category + '\n');
                }
            }

            if (Tags != null)
            {
                builder.Append("Tags:\n");
                Tags.ToList().ForEach(x => builder.Append(string.Concat(x, "\n")));
            }
            else
            {
                builder.Append("Tags: no information available\n");
            }

            builder.Append($"Rating: ");
            if (Rating != -1)
            {
                builder.Append(Rating);
            }
            else
            {
                builder.Append("no information available");
            }

            builder.Append('\n');

            return builder.ToString();
            
        }
    }
}