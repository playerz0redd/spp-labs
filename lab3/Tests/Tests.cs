using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestFramework;
using TargetApp;

namespace Tests
{
    public class UserTests
    {
        private UserService _service;

        [Before]
        public void Setup()
        {
            _service = new UserService();
            _service.AddUser(new User { Username = "Admin", Age = 30, Email = "admin@system.com" });
        }
        [Test(Priority = 1, Description = "add test")]
        public void TestAddUser()
        {
            Thread.Sleep(100);
            _service.AddUser(new User { Username = "NewUser", Age = 25, Email = "test@test.com" });
            Assert.IsTrue(_service.IsUsernameTaken("NewUser"));
        }

        [Test]
        public void TestValidation_InvalidEmail_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                _service.AddUser(new User { Username = "BadEmailUser", Age = 20, Email = "not-an-email" }));
        }

        [Test]
        [TestCase("Alice", 20)]
        [TestCase("Bob", 30)]
        public void TestMultipleUsers(string name, int age)
        {
            _service.AddUser(new User { Username = name, Age = age });
            Assert.IsNotNull(_service.GetUser(name));
        }

        [Test]
        public void TestValidation_TooYoung()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _service.AddUser(new User { Username = "Kid", Age = 10 }));
        }
        [Test(Description = "remove test")]
        public void TestRemoveUser_Success()
        {
            _service.AddUser(new User { Username = "Charlie", Age = 28 });
            bool isRemoved = _service.RemoveUser("Charlie");
            Assert.IsTrue(isRemoved);
            Assert.IsFalse(_service.IsUsernameTaken("Charlie"));
        }

        [Test]
        public void TestUpdateUserAge_Success()
        {
            _service.UpdateUserAge("Admin", 35);
            Assert.AreEqual(35, _service.GetUser("Admin").Age);
        }
        [Test(Description = "bulk test")]
        public void TestAddUsers_Bulk()
        {
            var newUsers = new List<User>
            {
                new User { Username = "Bulk1", Age = 20 },
                new User { Username = "Bulk2", Age = 22 },
                new User { Username = "Bulk3", Age = 24 }
            };

            _service.AddUsers(newUsers);
            Assert.IsTrue(_service.IsUsernameTaken("Bulk2"));
        }
        [Test(Description = "deactivate")]
        public void TestDeactivateUser()
        {
            _service.AddUser(new User { Username = "Worker", Age = 25 });
            _service.DeactivateUser("Worker");

            var worker = _service.GetUser("Worker");
            Assert.IsNotNull(worker);
            Assert.IsFalse(worker.IsActive);
        }
        [Test(Description = "get active")]
        public void TestGetActiveUsers()
        {
            _service.AddUser(new User { Username = "ActiveUser", Age = 25 });
            _service.AddUser(new User { Username = "BannedUser", Age = 30 });
            _service.DeactivateUser("BannedUser");

            var active = _service.GetActiveUsers().ToList();
            Assert.AreEqual(2, active.Count);
        }

        [Test(Description = "search")]
        public void TestSearchUsers()
        {
            _service.AddUser(new User { Username = "JohnDoe", Age = 40 });
            _service.AddUser(new User { Username = "Johnny", Age = 25 });

            var results = _service.SearchUsers("john").ToList();
            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public async Task TestAsyncCount()
        {
            int count = await _service.GetUsersCountAsync();
            Assert.GreaterThan(count, 0);
        }

        [Test]
        public async Task TestAverageAgeAsync()
        {
            _service.AddUser(new User { Username = "Ivan", Age = 20 });
            _service.AddUser(new User { Username = "Judy", Age = 40 });
            double avg = await _service.GetAverageAgeAsync();
            Assert.AreEqual(30.0, avg);
        }

        [Test(Description = "oldest user")]
        public async Task TestGetOldestUserAsync()
        {
            _service.AddUser(new User { Username = "Young", Age = 18 });
            _service.AddUser(new User { Username = "Elder", Age = 85 });
            _service.AddUser(new User { Username = "Mid", Age = 40 });

            var oldest = await _service.GetOldestUserAsync();
            Assert.IsNotNull(oldest);
            Assert.AreEqual("Elder", oldest.Username);
        }

        [Test]
        [Timeout(1000)]
        public void TestSuccessWithTimeout()
        {
            Thread.Sleep(200);
            Assert.IsTrue(true);
        }

        [Test]
        [Timeout(200)]
        public void TestFailByTimeout()
        {
            Thread.Sleep(1000);
            Assert.IsTrue(true);
        }
        [Test(Description = "crash test")]
        public void TestFatalCrash_PoolShouldSurvive()
        {
            throw new DivideByZeroException("test crashed");
        }

        [Test(Description = "hang test")]
        public void TestHanging_ShouldBeReplacedBySupervisor()
        {
            Thread.Sleep(10000);
            Assert.IsTrue(true);
        }

        [Test]
        [Ignore(Reason = "wip")]
        public void IgnoredTest() { }
    }
}