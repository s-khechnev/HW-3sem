using System.Text;

namespace BigDataApp
{
    public class Movie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public HashSet<Actor>? Actors { get; set; }
        public HashSet<Director>? Directors { get; set; }
        public HashSet<Tag>? Tags { get; set; }
        public float Rating { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder($"Title: {Title}\n");

            if (Actors != null)
            {
                builder.Append("Actors:\n");
                Actors.ToList().ForEach(x => builder.Append(string.Concat(x, "\n")));
            }
            else
            {
                builder.Append("Actors: no information available\n");
            }

            if (Directors != null)
            {
                builder.Append("Directors:\n");
                Directors.ToList().ForEach(x => builder.Append(string.Concat(x, "\n")));
            }
            else
            {
                builder.Append("Directors: no information available\n");
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