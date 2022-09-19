using System.Collections;
using System.Xml.Linq;

namespace LinqTasks
{
    class BusinessLogic
    {
        private List<User> users = new List<User>();
        private List<Record> records = new List<Record>();

        private readonly string[] names = new string[] {"Андрей", "Алексей", "Антон", "Борис",
            "Богдан"};

        private readonly string[] surnames = new string[] {"Абрикосов", "Антонов", "Андреев", "Алексеев",
            "Борисов"};

        private readonly string[] messages = new string[] { "Привет", "Пока", "Как дела?", "Что делаешь?", "Как прошел день?" };

        public BusinessLogic()
        {
            InitData();
        }

        private void InitData()
        {
            int N = 20;
            Enumerable.Range(0, N).ToList().ForEach(
                i =>
                {
                    Random rand = new();
                    User user = new(i, names[rand.Next(names.Length)], surnames[rand.Next(surnames.Length)]);
                    Record record = new(user, messages[rand.Next(messages.Length)]);

                    users.Add(user);
                    records.Add(record);
                }
            );
        }

        public List<User> GetUsersBySurname(string surname)
        {
            if (string.IsNullOrEmpty(surname))
                throw new ArgumentNullException(nameof(surname));

            return users.Where(user => user.Surname == surname).ToList();
        }

        public User GetUserByID(int id) => users.Where(user => user.ID == id).Single();

        public List<User> GetUsersBySubstring(string substring)
        {
            if (string.IsNullOrEmpty(substring))
                throw new ArgumentNullException(nameof(substring));

            return users.Where(user => user.Name.Contains(substring) || user.Surname.Contains(substring)).ToList();
        }

        public List<string> GetAllUniqueNames() => users.Select(user => user.Name).Distinct().ToList();

        public List<User> GetAllAuthors()
        {
            return users.Join(records, user => user, record => record.Author,
                                (user, record) => new User(
                                record.Author.ID,
                                record.Author.Name,
                                record.Author.Surname)).ToList();
        }

        public Dictionary<int, User> GetUsersDictionary() => users.ToDictionary(user => user.ID, user => user);

        public int GetMaxID() => users.Select(user => user.ID).Max();

        public List<User> GetOrderedUsers() => users.OrderBy(user => user.ID).ToList();

        public List<User> GetDescendingOrderedUsers() => users.OrderByDescending(user => user.ID).ToList();

        public List<User> GetReversedUsers() => users.Reverse<User>().ToList();

        public List<User> GetUsersPage(int pageSize, int pageIndex)
        {
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            return users.Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

    }
}