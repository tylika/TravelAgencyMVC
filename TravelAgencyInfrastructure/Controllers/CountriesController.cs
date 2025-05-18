// Файл: TravelAgencyInfrastructure/Controllers/CountriesController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyDomain.Model;
using TravelAgencyInfrastructure;

namespace TravelAgencyInfrastructure.Controllers
{
    public class CountriesController : Controller
    {
        private readonly TravelAgencyDbContext _context;

        public CountriesController(TravelAgencyDbContext context)
        {
            _context = context;
        }

        // GET: Countries
        public async Task<IActionResult> Index()
        {
            return View(await _context.Countries.ToListAsync());
        }

        // GET: Countries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var country = await _context.Countries.FirstOrDefaultAsync(m => m.CountryId == id);
            if (country == null) return NotFound();
            return View(country);
        }

        // GET: Countries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CountryName,VisaRequired,Continent")] Country country)
        {
            // Перевірка на унікальність назви країни 
            if (await _context.Countries.AnyAsync(c => c.CountryName.ToLower() == country.CountryName.ToLower()))
            {
                ModelState.AddModelError("CountryName", "Країна з такою назвою вже існує.");
            }
            
            ModelState.Remove("CountryName");
            if (ModelState.IsValid)
            {
                _context.Add(country);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        // GET: Countries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound();
            return View(country);
        }

        // POST: Countries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CountryId,CountryName,VisaRequired,Continent")] Country country)
        {
            if (id != country.CountryId) return NotFound();

            // Перевірка на унікальність назви країни (без урахування регістру), окрім поточного запису
            if (await _context.Countries.AnyAsync(c => c.CountryId != country.CountryId && c.CountryName.ToLower() == country.CountryName.ToLower()))
            {
                ModelState.AddModelError("CountryName", "Країна з такою назвою вже існує.");
            }

            ModelState.Remove("CountryName");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(country);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CountryExists(country.CountryId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        // GET: Countries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var country = await _context.Countries.FirstOrDefaultAsync(m => m.CountryId == id);
            if (country == null) return NotFound();
            return View(country);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Countries == null)
            {
                return Problem("Entity set 'TravelAgencyDbContext.Countries' is null.");
            }
            var country = await _context.Countries.FindAsync(id);
            if (country != null)
            {
                bool hasRelatedTours = await _context.Tours.AnyAsync(t => t.CountryId == id);
                bool hasRelatedHotels = await _context.Hotels.AnyAsync(h => h.CountryId == id);

                if (hasRelatedTours || hasRelatedHotels)
                {
                    ModelState.AddModelError(string.Empty, "Неможливо видалити країну. Вона має пов'язані тури або готелі. Спочатку видаліть або перепризначте їх.");
                    var countryToDeleteView = await _context.Countries.FirstOrDefaultAsync(m => m.CountryId == id); // Повторно завантажуємо для View
                    return View("Delete", countryToDeleteView);
                }
                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CountryExists(int id)
        {
            return (_context.Countries?.Any(e => e.CountryId == id)).GetValueOrDefault();
        }
    }
}