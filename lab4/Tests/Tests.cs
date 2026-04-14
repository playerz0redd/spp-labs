using System;
using System.Collections.Generic;
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
            _service.AddUser(new User { Username = "Admin", Age = 30 });
        }


        public static IEnumerable<object[]> GetUsersData()
        {
            yield return new object[] { "Yoda", 900 };
            yield return new object[] { "Luke", 25 };
            yield return new object[] { "Vader", 45 };
        }

   
        [Test]
        [TestCaseSource(nameof(GetUsersData))]
        public void TestAddUserYield(string name, int age)
        {
            _service.AddUser(new User { Username = name, Age = age });
            Assert.IsNotNull(_service.GetUser(name));
        }


        [Test, Category("skip")]
        public void TestWillBeSkipped()
        {
            throw new Exception("этот тест не должен был запуститься!");
        }

        [Test]
        public void TestExpressionTreeParse()
        {
            int limit = 18;
            int currentAge = 15;


            Assert.Expr(() => currentAge >= limit);
        }

        [Test]
        public void TestValidation_InvalidEmail()
        {
            Assert.Throws<ArgumentException>(() =>
                _service.AddUser(new User { Username = "Bad", Age = 20, Email = "no" }));
        }

        [Test(Description = "hang test")]
        public void TestHanging_ShouldBeReplacedBySupervisor()
        {
            Thread.Sleep(6000);
            Assert.IsTrue(true);
        }
    }
}