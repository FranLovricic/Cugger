using Cugger.Data;
using Cugger.Models;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Repositories
{
    public class VenueRepository
    {
        private readonly CuggerDbContext _db;

        public VenueRepository(CuggerDbContext db)
        {
            _db = db;
        }

        public List<Venue> GetAll()
        {
            return _db.Venues
                .OrderBy(v => v.Name)
                .ToList();
        }

        public Venue? GetById(int id)
        {
            return _db.Venues
                .Include(v => v.CheckIns)
                .FirstOrDefault(v => v.Id == id);
        }

        public List<Venue> GetByCity(string city)
        {
            return _db.Venues
                .Where(v => v.City == city)
                .OrderBy(v => v.Name)
                .ToList();
        }

        public int GetCheckInCount(int venueId)
            => _db.CheckIns.Count(c => c.VenueId == venueId);
    }
}
