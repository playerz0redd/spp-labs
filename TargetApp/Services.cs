using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TargetApp
{
    public class User
    {
        public string Username { get; set; }
        public int Age { get; set; }
    }

    public class UserService
    {
        private List<User> _users = new List<User>();

        public void AddUser(User user)
        {
            if (string.IsNullOrEmpty(user.Username)) throw new ArgumentException("Name empty");
            if (user.Age < 18) throw new ArgumentOutOfRangeException("Too young");
            _users.Add(user);
        }

        public async Task<int> GetUsersCountAsync()
        {
            await Task.Delay(50); // Имитация асинхронной работы
            return _users.Count;
        }

        public bool IsUsernameTaken(string name) => _users.Exists(u => u.Username == name);
    }
}