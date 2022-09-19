using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqTasks
{
    public class Record
    {
        public User Author { get; set; }
        public string Message { get; set; }
        public Record(User author, string message)
        {
            this.Author = author;
            this.Message = message;
        }
    }
}
