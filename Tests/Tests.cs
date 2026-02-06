using System;
using System.Collections.Generic;
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
            SharedContext.Set("DefaultName", "Admin");
        }

        // --- УЖЕ СУЩЕСТВУЮЩИЕ ТЕСТЫ ---

        [Test(Priority = 1, Description = "Check user addition")]
        public void TestAddUser()
        {
            var name = SharedContext.Get<string>("DefaultName");
            _service.AddUser(new User { Username = name, Age = 25 });
            Assert.IsTrue(_service.IsUsernameTaken(name));
        }

        [Test]
        [TestCase("Alice", 20)]
        [TestCase("Bob", 30)] // Это считается за 2 выполнения
        public void TestMultipleUsers(string name, int age)
        {
            _service.AddUser(new User { Username = name, Age = age });
            Assert.IsNotNull(name);
        }

        [Test]
        public void TestValidation()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _service.AddUser(new User { Username = "Kid", Age = 10 }));
        }

        [Test]
        public async Task TestAsyncCount()
        {
            _service.AddUser(new User { Username = "User1", Age = 20 });
            int count = await _service.GetUsersCountAsync();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Ignore(Reason = "Work in progress")]
        public void IgnoredTest() { }

        // --- НОВЫЕ ТЕСТЫ ДЛЯ КОЛИЧЕСТВА И ДЕМОНСТРАЦИИ FAIL ---

        [Test]
        public void TestContainsName()
        {
            string name = "SuperAdmin";
            Assert.Contains("Admin", name); // Успех
        }

        [Test]
        public void TestNotEqual()
        {
            Assert.AreNotEqual("Guest", SharedContext.Get<string>("DefaultName")); // Успех
        }

        [Test]
        public void TestEmptyOnStart()
        {
            // Используем Assert.IsFalse
            Assert.IsFalse(_service.IsUsernameTaken("Anyone"), "Service should be empty"); // Успех
        }

        // --- ТЕСТЫ, КОТОРЫЕ ДОЛЖНЫ УПАСТЬ (FAIL) ---

        [Test]
        public void TestFailOnPurpose()
        {
            // Этот тест упадет, так как мы ожидаем 10, а будет 0
            Assert.AreEqual(10, 0);
        }

        [Test]
        public void TestUserNotFound_Fail()
        {
            // Ожидаем, что пользователь есть, но его нет
            Assert.IsTrue(_service.IsUsernameTaken("NonExistentUser"), "User should have been found");
        }

        [Test]
        public void TestNullCheck_Fail()
        {
            User user = null;
            Assert.IsNotNull(user); // Упадет здесь
        }

        [Test]
        public void TestWrongException_Fail()
        {
            // Ожидаем NullReferenceException, но код бросит ArgumentException
            Assert.Throws<NullReferenceException>(() =>
                _service.AddUser(new User { Username = "", Age = 20 }));
        }

        [After]
        public void Teardown() => SharedContext.Clear();
    }
}