namespace LinqTasks
{
    class BusinessLogic
    {
        private List<User> users = new List<User>();
        private List<Record> records = new List<Record>();

        private string[] names = new string[] {"Андрей", "Алексей", "Антон", "Борис",
            "Богдан"};

        private string[] surnames = new string[] {"Абрикосов", "Антонов", "Андреев", "Алексеев",
            "Борисов"};

        private string[] messages = new string[] { "Привет", "Пока", "Как дела?", "Что делаешь?", "Как прошел день?" };

        public BusinessLogic()
        {
            InitData();
        }

        private void InitData()
        {
            int n = 100;
            for (int i = 0; i < n; i++)
            {
                User user = new(i, names[i % 5], surnames[i % 5]);
                Record record = new(user, messages[i % 5]);

                users.Add(user);
                records.Add(record);
            }
        }

        public List<User> GetUsersBySurname(string surname)
        {
            if (string.IsNullOrEmpty(surname))
                throw new ArgumentNullException(nameof(surname));

            return users.Where(user => user.Surname == surname).ToList();
        }

        public User GetUserByID(int id)
        {
            return users.Where(user => user.ID == id).Single();
        }

        public List<User> GetUsersBySubstring(string substring)
        {
            if (string.IsNullOrEmpty(substring))
                throw new ArgumentNullException(nameof(substring));

            return users.Where(user => user.Name.Contains(substring) || user.Surname.Contains(substring)).ToList();
        }

        public List<string> GetAllUniqueNames()
        {
            return users.Select(user => user.Name).Distinct().ToList();
        }

        public List<User> GetAllAuthors()
        {
            return records.Where(record => !string.IsNullOrEmpty(record.Message)).Select(record => record.Author).ToList();
        }

        public Dictionary<int, User> GetUsersDictionary()
        {
            return users.ToDictionary(user => user.ID, user => user);
        }

        public int GetMaxID()
        {
            return users.Select(user => user.ID).Max();
        }

        public List<User> GetOrderedUsers()
        {
            return users.OrderBy(user => user.ID).ToList();
        }

        public List<User> GetDescendingOrderedUsers()
        {
            return users.OrderByDescending(user => user.ID).ToList();
        }

        public List<User> GetReversedUsers()
        {
            IEnumerable<User> usersEnum = users;
            return usersEnum.Reverse().ToList();
        }

        public List<User> GetUsersPage(int pageSize, int pageIndex)
        {
            if (pageSize <= 0)
                throw new ArgumentNullException(nameof(pageSize));
            if (pageIndex < 0)
                throw new ArgumentNullException(nameof(pageIndex));

            return users.Skip(pageSize * pageIndex).Take(pageSize).ToList();
        }

    }
}