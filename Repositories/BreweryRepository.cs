using Cugger.Data;
using Cugger.Models;
using Microsoft.EntityFrameworkCore;

namespace Cugger.Repositories
{
    public class BreweryRepository
    {
        private readonly CuggerDbContext _db;

        public BreweryRepository(CuggerDbContext db)
        {
            _db = db;
        }

        public List<Brewery> GetAll()
        {
            return _db.Breweries
                .OrderBy(b => b.Name)
                .ToList();
        }

        public Brewery? GetById(int id)
        {
            return _db.Breweries
                .Include(b => b.Beers)
                .FirstOrDefault(b => b.Id == id);
        }

        public List<Brewery> GetByCountry(string country)
        {
            return _db.Breweries
                .Where(b => b.Country == country)
                .OrderBy(b => b.Name)
                .ToList();
        }
    }
}
