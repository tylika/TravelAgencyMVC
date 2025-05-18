// Файл: TravelAgencyInfrastructure/Controllers/ToursController.cs
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
    public class ToursController : Controller
    {
        private readonly TravelAgencyDbContext _context;

        public ToursController(TravelAgencyDbContext context)
        {
            _context = context;
        }

        // GET: Tours
        public async Task<IActionResult> Index()
        {
            var tours = _context.Tours
                .Include(t => t.Country)
                .Include(t => t.Hotel);
            return View(await tours.ToListAsync());
        }

        // GET: Tours/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var tour = await _context.Tours
                .Include(t => t.Country)
                .Include(t => t.Hotel)
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null) return NotFound();
            return View(tour);
        }

        // GET: Tours/Create
        public IActionResult Create()
        {
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName");
            ViewData["HotelId"] = new SelectList(_context.Hotels, "HotelId", "HotelName");
            return View();
        }

        // POST: Tours/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TourName,CountryId,HotelId,StartDate,DurationDays,PricePerPerson,TransportType,Description,MaxParticipants")] Tour tour)
        {
            // Валідація StartDate: не може бути в минулому
            if (tour.StartDate < DateTime.Today) // Порівнюємо тільки дату, без часу
            {
                ModelState.AddModelError("StartDate", "Дата початку туру не може бути в минулому.");
            }

            // Додаткова перевірка, чи обраний готель належить обраній країні (опціонально, але логічно)
            var hotel = await _context.Hotels.FindAsync(tour.HotelId);
            if (hotel != null && hotel.CountryId != tour.CountryId)
            {
                ModelState.AddModelError("HotelId", "Обраний готель не знаходиться в обраній країні.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName", tour.CountryId);
            // Фільтруємо готелі за обраною країною, якщо країна обрана
            ViewData["HotelId"] = new SelectList(_context.Hotels.Where(h => h.CountryId == tour.CountryId), "HotelId", "HotelName", tour.HotelId);
            return View(tour);
        }

        // GET: Tours/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null) return NotFound();
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName", tour.CountryId);
            ViewData["HotelId"] = new SelectList(_context.Hotels.Where(h => h.CountryId == tour.CountryId), "HotelId", "HotelName", tour.HotelId); // Фільтр готелів
            return View(tour);
        }

        // POST: Tours/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TourId,TourName,CountryId,HotelId,StartDate,DurationDays,PricePerPerson,TransportType,Description,MaxParticipants")] Tour tour)
        {
            if (id != tour.TourId) return NotFound();

            if (tour.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "Дата початку туру не може бути в минулому.");
            }
            var hotel = await _context.Hotels.FindAsync(tour.HotelId);
            if (hotel != null && hotel.CountryId != tour.CountryId)
            {
                ModelState.AddModelError("HotelId", "Обраний готель не знаходиться в обраній країні.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.TourId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CountryId"] = new SelectList(_context.Countries, "CountryId", "CountryName", tour.CountryId);
            ViewData["HotelId"] = new SelectList(_context.Hotels.Where(h => h.CountryId == tour.CountryId), "HotelId", "HotelName", tour.HotelId);
            return View(tour);
        }

        // GET: Tours/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tour = await _context.Tours
                .Include(t => t.Country)
                .Include(t => t.Hotel)
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null) return NotFound();
            return View(tour);
        }

        // POST: Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                bool hasBookings = await _context.Bookings.AnyAsync(b => b.TourId == id);
                bool hasReviews = await _context.Reviews.AnyAsync(r => r.TourId == id);
                if (hasBookings || hasReviews)
                {
                    ModelState.AddModelError(string.Empty, "Неможливо видалити тур. Існують пов'язані бронювання або відгуки.");
                    // Повторно завантажуємо дані для View
                    var tourToDeleteView = await _context.Tours
                        .Include(t => t.Country)
                        .Include(t => t.Hotel)
                        .FirstOrDefaultAsync(m => m.TourId == id);
                    return View("Delete", tourToDeleteView);
                }
                _context.Tours.Remove(tour);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.TourId == id);
        }
    }
}