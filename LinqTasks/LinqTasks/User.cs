using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqTasks
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public User(int id, string name, string surname)
        {
            this.ID = id;
            this.Name = name;
            this.Surname = surname;
        }

        public override string ToString()
        {
            return string.Format("ID={0}: {1} {2}", ID, Name, Surname);
        }

        public override bool Equals(object? other)
        {
            var toCompareWith = other as User;
            if (toCompareWith == null)
                return false;
            return this.Name == toCompareWith.Name &&
                this.Surname == toCompareWith.Surname && this.ID == toCompareWith.ID;

        }
    }
}
