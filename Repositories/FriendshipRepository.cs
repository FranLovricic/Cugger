using Cugger.Data;
using Cugger.Models;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Repositories
{
    public class FriendshipRepository
    {
        private readonly CuggerDbContext _db;

        public FriendshipRepository(CuggerDbContext db)
        {
            _db = db;
        }

        public List<Friendship> GetAll()
        {
            return _db.Friendships
                .Include(f => f.FromUser)
                .Include(f => f.ToUser)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();
        }

        public Friendship? GetById(int id)
        {
            return _db.Friendships
                .Include(f => f.FromUser)
                .Include(f => f.ToUser)
                .FirstOrDefault(f => f.Id == id);
        }
    }
}
