// Файл: TravelAgencyInfrastructure/Controllers/HotelsController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelAgencyDomain.Model;
using TravelAgencyInfrastructure;

namespace TravelAgencyInfrastructure.Controllers
{
    public class HotelsController : Controller
    {
        private readonly TravelAgencyDbContext _context;

        public HotelsController(TravelAgencyDbContext context)
        {
            _context = context;
        }

        // GET: Hotels
        public async Task<IActionResult> Index()
        {
            var travelAgencyDbContext = _context.Hotels.Include(h => h.Country);
            return View(await travelAgencyDbContext.ToListAsync());
        }

        // GET: Hotels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var hotel = await _context.Hotels
                .Include(h => h.Country)
                .FirstOrDefaultAsync(m => m.HotelId == id);
            if (hotel == null) return NotFound();
            return View(hotel);
        }

        // GET: Hotels/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName");
            return View();
        }

        // POST: Hotels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HotelName,CountryId,City,StarRating,Address,Description")] Hotel hotel)
        {
            // Перевірка на унікальність назви готелю в межах обраної країни
            if (await _context.Hotels.AnyAsync(h => h.HotelName.ToLower() == hotel.HotelName.ToLower() && h.CountryId == hotel.CountryId))
            {
                ModelState.AddModelError("HotelName", "Готель з такою назвою вже існує в обраній країні.");
            }

            if (!await _context.Countries.AnyAsync(c => c.CountryId == hotel.CountryId))
            {
                ModelState.AddModelError("CountryId", "Обраної країни не існує.");
            }


            if (ModelState.IsValid)
            {
                _context.Add(hotel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName", hotel.CountryId);
            return View(hotel);
        }

        // GET: Hotels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null) return NotFound();
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName", hotel.CountryId);
            return View(hotel);
        }

        // POST: Hotels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HotelId,HotelName,CountryId,City,StarRating,Address,Description")] Hotel hotel)
        {
            if (id != hotel.HotelId) return NotFound();

            // Перевірка на унікальність назви готелю в межах обраної країни (крім поточного готелю)
            if (await _context.Hotels.AnyAsync(h => h.HotelId != hotel.HotelId && h.HotelName.ToLower() == hotel.HotelName.ToLower() && h.CountryId == hotel.CountryId))
            {
                ModelState.AddModelError("HotelName", "Інший готель з такою назвою вже існує в обраній країні.");
            }

            if (!await _context.Countries.AnyAsync(c => c.CountryId == hotel.CountryId))
            {
                ModelState.AddModelError("CountryId", "Обраної країни не існує.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hotel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HotelExists(hotel.HotelId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName", hotel.CountryId);
            return View(hotel);
        }

        // GET: Hotels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var hotel = await _context.Hotels
                .Include(h => h.Country)
                .FirstOrDefaultAsync(m => m.HotelId == id);
            if (hotel == null) return NotFound();
            return View(hotel);
        }

        // POST: Hotels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel != null)
            {
                // Перевірка на пов'язані тури
                bool hasTours = await _context.Tours.AnyAsync(t => t.HotelId == id);
                if (hasTours)
                {
                    ModelState.AddModelError(string.Empty, "Неможливо видалити готель, оскільки він використовується в турах. Спочатку видаліть або перепризначте ці тури.");
                    var hotelToDeleteView = await _context.Hotels.Include(h => h.Country).FirstOrDefaultAsync(m => m.HotelId == id);
                    return View("Delete", hotelToDeleteView);
                }
                _context.Hotels.Remove(hotel);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HotelExists(int id)
        {
            return _context.Hotels.Any(e => e.HotelId == id);
        }
    }
}