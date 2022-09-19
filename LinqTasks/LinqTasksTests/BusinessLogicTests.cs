using LinqTasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace LinqTasksTests
{
    [TestClass]
    public class BusinessLogicTests
    {
        User[]? users;
        Record[]? records;
        BusinessLogic? businessLogic;

        [TestInitialize]
        public void Initialize()
        {
            users = new User[]
            {
                new User(0, "Alex", "Johnson"),
                new User(1, "John", "Smith"),
                new User(2, "Donald", "Smith"),
                new User(3, "Mike", "Green"),
                new User(4, "Sarah", "Paulson"),
                new User(5, "Paul", "Watson"),
                new User(6, "Paul", "Biden")
            };

            records = new Record[]
            {
                new Record(users[0], "Hello"),
                new Record(users[1], "Good bye"),
                new Record(users[2], "How are you?"),
                new Record(users[3], "How old are you?")
            };

            businessLogic = new(users.ToList(), records.ToList());
        }

        [TestCleanup]
        public void Cleanup() { }

        [TestMethod]
        public void GetUsersBySurname_RealSurname()
        {
            string arg = "Smith";

            List<User> expectedUsers = new() { users[1], users[2] };

            List<User> result = businessLogic.GetUsersBySurname(arg);

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetUsersBySurname_NullSurname()
        {
            string arg = null;

            businessLogic.GetUsersBySurname(arg);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetUsersBySurname_EmptySurname()
        {
            string arg = "";

            businessLogic.GetUsersBySurname(arg);
        }

        [TestMethod]
        public void GetUsersBySurname_UnrealSurname()
        {
            string arg = "Trump";

            List<User> expectedUsers = new();

            List<User> result = businessLogic.GetUsersBySurname(arg);

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        public void GetUsersBySubstring_RealSubstring()
        {
            string arg = "Smi";

            List<User> expectedUsers = new() { users[1], users[2] };

            List<User> result = businessLogic.GetUsersBySubstring(arg);

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetUsersBySubstring_NullSubstring()
        {
            string arg = null;

            businessLogic.GetUsersBySubstring(arg);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetUsersBySubstring_EmptySubstring()
        {
            string arg = "";

            businessLogic.GetUsersBySubstring(arg);
        }

        [TestMethod]
        public void GetUsersBySubstring_UnrealSubstring()
        {
            string arg = "Trump";

            List<User> expectedUsers = new();

            List<User> result = businessLogic.GetUsersBySubstring(arg);

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        public void GetUsersBySurname_UnrealSubstring()
        {
            string arg = "Trump";

            List<User> expectedUsers = new();

            List<User> result = businessLogic.GetUsersBySubstring(arg);

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        public void GetUserById_RealId()
        {
            int arg = 4;

            User expectedUser = users[4];

            User result = businessLogic.GetUserByID(arg);

            Assert.AreEqual(expectedUser, result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetUserById_UnrealId()
        {
            int arg = 150;

            User expectedUser = null;

            User result = businessLogic.GetUserByID(arg);

            Assert.AreEqual(expectedUser, result);
        }

        [TestMethod]
        public void GetAllUniqueNames()
        {
            List<string> expectedNames = new() { "Alex", "John", "Donald", "Mike", "Sarah", "Paul" };

            List<string> result = businessLogic.GetAllUniqueNames();

            CollectionAssert.AreEqual(expectedNames, result);
        }

        [TestMethod]
        public void GetAllAuthors()
        {
            List<User> expectedUsers = users.ToList().Take(4).ToList();

            List<User> result = businessLogic.GetAllAuthors();

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        public void GetUsersDictionary()
        {
            Dictionary<int, User> expectedDict = new()
            {
                {0, users[0] },
                {1, users[1] },
                {2, users[2] },
                {3, users[3] },
                {4, users[4] },
                {5, users[5] },
                {6, users[6] }
            };

            Dictionary<int, User> result = businessLogic.GetUsersDictionary();

            CollectionAssert.AreEqual(expectedDict, result);
        }

        [TestMethod]
        public void GetMaxID()
        {
            int expectedId = 6;

            int result = businessLogic.GetMaxID();

            Assert.AreEqual(expectedId, result);
        }

        [TestMethod]
        public void GetOrderedUsers()
        {
            List<User> expectedUsers = users.ToList();

            List<User> result = businessLogic.GetOrderedUsers();

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        public void GetDescendingOrderedUsers()
        {
            List<User> expectedUsers = users.Reverse().ToList();

            List<User> result = businessLogic.GetDescendingOrderedUsers();

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        public void GetReversedUsers()
        {
            List<User> expectedUsers = users.Reverse().ToList();

            List<User> result = businessLogic.GetReversedUsers();

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetUsersPage_PageSizeLessZero_PageIndexLessZero()
        {
            int arg1 = -1;
            int arg2 = -1;

            List<User> result = businessLogic.GetUsersPage(arg1, arg2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetUsersPage_PageSizeLessZero_PageIndexGreaterZero()
        {
            int arg1 = -1;
            int arg2 = 1;

            List<User> result = businessLogic.GetUsersPage(arg1, arg2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetUsersPage_PageSizeGreaterZero_PageIndexLessZero()
        {
            int arg1 = 1;
            int arg2 = -1;

            List<User> result = businessLogic.GetUsersPage(arg1, arg2);
        }

        [TestMethod]
        public void GetUsersPage_CorrectData()
        {
            int arg1 = 3;
            int arg2 = 2;

            List<User> expectedUsers = new() { users[6] };

            List<User> result = businessLogic.GetUsersPage(arg1, arg2);

            CollectionAssert.AreEqual(expectedUsers, result);
        }

        [TestMethod]
        public void GetUsersPage_More()
        {
            int arg1 = 3;
            int arg2 = 1000;

            List<User> expectedUsers = new();

            List<User> result = businessLogic.GetUsersPage(arg1, arg2);

            CollectionAssert.AreEqual(expectedUsers, result);
        }
    }
}