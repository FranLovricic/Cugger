using Cugger.Data;
using Cugger.Models;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Repositories
{
    public class UserRepository
    {
        private readonly CuggerDbContext _db;

        public UserRepository(CuggerDbContext db)
        {
            _db = db;
        }

        public List<User> GetAll()
        {
            return _db.Users
                .OrderBy(u => u.Username)
                .ToList();
        }

        public User? GetById(int id)
        {
            return _db.Users
                .Include(u => u.CheckIns)
                .Include(u => u.Reviews)
                .FirstOrDefault(u => u.Id == id);
        }

        public User? GetByUsername(string username)
        {
            return _db.Users.FirstOrDefault(u => u.Username == username);
        }

        public int GetCheckInCount(int userId)
            => _db.CheckIns.Count(c => c.UserId == userId);

        public int GetFriendsCount(int userId)
            => _db.Friendships.Count(f => f.FromUserId == userId);

        public List<User> GetFriends(int userId)
        {
            var friendIds = _db.Friendships
                .Where(f => f.FromUserId == userId)
                .Select(f => f.ToUserId)
                .ToList();
            return _db.Users.Where(u => friendIds.Contains(u.Id)).ToList();
        }

        public List<User> GetMostActive(int count = 5)
        {
            return _db.Users
                .Select(u => new { U = u, Cnt = u.CheckIns.Count() })
                .OrderByDescending(x => x.Cnt)
                .Take(count)
                .Select(x => x.U)
                .ToList();
        }
    }
}
