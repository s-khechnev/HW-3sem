using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigDataApp
{
    class Movie
    {
        public string Title { get; set; }
        public HashSet<string> Actors { get; set; }
        public string Directors { get; set; }
        public HashSet<string> Tags { get; set; }
        public string Rating { get; set; }
    }
}
