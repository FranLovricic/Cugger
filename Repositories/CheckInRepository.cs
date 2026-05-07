using Cugger.Data;
using Cugger.Models;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Repositories
{
    public class CheckInRepository
    {
        private readonly CuggerDbContext _db;

        public CheckInRepository(CuggerDbContext db)
        {
            _db = db;
        }

        public List<CheckIn> GetAll()
        {
            return _db.CheckIns
                .Include(c => c.User)
                .Include(c => c.Beer).ThenInclude(b => b!.Brewery)
                .Include(c => c.Venue)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public CheckIn? GetById(int id)
        {
            return _db.CheckIns
                .Include(c => c.User)
                .Include(c => c.Beer).ThenInclude(b => b!.Brewery)
                .Include(c => c.Venue)
                .FirstOrDefault(c => c.Id == id);
        }

        public List<CheckIn> GetByUser(int userId)
        {
            return _db.CheckIns
                .Include(c => c.Beer).ThenInclude(b => b!.Brewery)
                .Include(c => c.Venue)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public List<CheckIn> GetByBeer(int beerId)
        {
            return _db.CheckIns
                .Include(c => c.User)
                .Include(c => c.Venue)
                .Where(c => c.BeerId == beerId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public List<CheckIn> GetByVenue(int venueId)
        {
            return _db.CheckIns
                .Include(c => c.User)
                .Include(c => c.Beer)
                .Where(c => c.VenueId == venueId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public List<CheckIn> GetRecent(int count = 10)
        {
            return _db.CheckIns
                .Include(c => c.User)
                .Include(c => c.Beer).ThenInclude(b => b!.Brewery)
                .Include(c => c.Venue)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToList();
        }
    }
}
