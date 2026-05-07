using Cugger.Data;
using Cugger.Models;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Repositories
{
    public class BeerRepository
    {
        private readonly CuggerDbContext _db;

        public BeerRepository(CuggerDbContext db)
        {
            _db = db;
        }

        public List<Beer> GetAll()
        {
            return _db.Beers
                .Include(b => b.Brewery)
                .OrderBy(b => b.Name)
                .ToList();
        }

        public Beer? GetById(int id)
        {
            return _db.Beers
                .Include(b => b.Brewery)
                .Include(b => b.CheckIns)
                .Include(b => b.Reviews)
                .FirstOrDefault(b => b.Id == id);
        }

        public List<Beer> GetByBrewery(int breweryId)
        {
            return _db.Beers
                .Where(b => b.BreweryId == breweryId)
                .OrderBy(b => b.Name)
                .ToList();
        }

        public List<Beer> GetByStyle(BeerStyle style)
        {
            return _db.Beers
                .Include(b => b.Brewery)
                .Where(b => b.Style == style)
                .OrderBy(b => b.Name)
                .ToList();
        }

        public List<Beer> Search(string? query)
        {
            var q = _db.Beers.Include(b => b.Brewery).AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                var trimmed = query.Trim();
                q = q.Where(b => b.Name.Contains(trimmed) || b.Description.Contains(trimmed));
            }
            return q.OrderBy(b => b.Name).ToList();
        }

        public double GetAverageRating(int beerId)
        {
            var ratings = _db.CheckIns.Where(c => c.BeerId == beerId).Select(c => c.Rating).ToList();
            return ratings.Count > 0 ? ratings.Average() : 0.0;
        }

        public int GetRatingCount(int beerId)
            => _db.CheckIns.Count(c => c.BeerId == beerId);

        public List<Beer> GetTopRated(int count = 5)
        {
            // Project ratings via group join
            return _db.Beers
                .Include(b => b.Brewery)
                .Select(b => new
                {
                    Beer = b,
                    Avg = b.CheckIns.Any() ? b.CheckIns.Average(c => c.Rating) : 0.0,
                    Cnt = b.CheckIns.Count()
                })
                .OrderByDescending(x => x.Avg)
                .ThenByDescending(x => x.Cnt)
                .Take(count)
                .AsEnumerable()
                .Select(x =>
                {
                    x.Beer.AverageRating = x.Avg;
                    x.Beer.RatingCount = x.Cnt;
                    return x.Beer;
                })
                .ToList();
        }
    }
}
