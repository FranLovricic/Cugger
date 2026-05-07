using Cugger.Data;
using Cugger.Models;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Repositories
{
    public class ReviewRepository
    {
        private readonly CuggerDbContext _db;

        public ReviewRepository(CuggerDbContext db)
        {
            _db = db;
        }

        public List<Review> GetAll()
        {
            return _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Beer).ThenInclude(b => b!.Brewery)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public Review? GetById(int id)
        {
            return _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Beer).ThenInclude(b => b!.Brewery)
                .FirstOrDefault(r => r.Id == id);
        }

        public List<Review> GetByBeer(int beerId)
        {
            return _db.Reviews
                .Include(r => r.User)
                .Where(r => r.BeerId == beerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<Review> GetByUser(int userId)
        {
            return _db.Reviews
                .Include(r => r.Beer).ThenInclude(b => b!.Brewery)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<Review> GetTopLiked(int count = 5)
        {
            return _db.Reviews
                .Include(r => r.User)
                .Include(r => r.Beer)
                .OrderByDescending(r => r.Likes)
                .Take(count)
                .ToList();
        }
    }
}
