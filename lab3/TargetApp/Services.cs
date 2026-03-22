using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TargetApp
{
    public class User
    {
        public string Username { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UserService
    {
        private List<User> _users = new List<User>();

        public void AddUser(User user)
        {
            if (string.IsNullOrEmpty(user.Username)) throw new ArgumentException("Name empty");
            if (user.Age < 18) throw new ArgumentOutOfRangeException("Too young");
            if (IsUsernameTaken(user.Username)) throw new InvalidOperationException("User already exists");

            if (!string.IsNullOrEmpty(user.Email) && !user.Email.Contains("@"))
                throw new ArgumentException("Invalid email format");

            _users.Add(user);
        }

        public void AddUsers(IEnumerable<User> users)
        {
            if (users == null) throw new ArgumentNullException(nameof(users));
            foreach (var user in users)
            {
                AddUser(user);
            }
        }

        public User GetUser(string name) => _users.FirstOrDefault(u => u.Username == name);

        public IEnumerable<User> GetUsersByAgeRange(int minAge, int maxAge) =>
            _users.Where(u => u.Age >= minAge && u.Age <= maxAge).ToList();

        public IEnumerable<User> GetActiveUsers() => _users.Where(u => u.IsActive).ToList();

        public IEnumerable<User> SearchUsers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<User>();
            return _users.Where(u => u.Username.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public void UpdateUserAge(string username, int newAge)
        {
            if (newAge < 18) throw new ArgumentOutOfRangeException("Too young");
            var user = GetUser(username);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.Age = newAge;
        }

        public void DeactivateUser(string username)
        {
            var user = GetUser(username);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.IsActive = false;
        }

        public bool RemoveUser(string username)
        {
            var user = GetUser(username);
            if (user != null) return _users.Remove(user);
            return false;
        }

        public void Clear() => _users.Clear();

        public async Task<double> GetAverageAgeAsync()
        {
            await Task.Delay(150);
            if (!_users.Any()) return 0;
            return _users.Average(u => u.Age);
        }

        public async Task<User> GetOldestUserAsync()
        {
            await Task.Delay(200);
            if (!_users.Any()) return null;
            return _users.OrderByDescending(u => u.Age).First();
        }

        public async Task<int> GetUsersCountAsync()
        {
            await Task.Delay(50);
            return _users.Count;
        }

        public bool IsUsernameTaken(string name) => _users.Exists(u => u.Username == name);
    }
}