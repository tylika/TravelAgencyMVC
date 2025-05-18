// Файл: TravelAgencyInfrastructure/Controllers/ReviewsController.cs
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
    public class ReviewsController : Controller
    {
        private readonly TravelAgencyDbContext _context;

        public ReviewsController(TravelAgencyDbContext context)
        {
            _context = context;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var travelAgencyDbContext = _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Tour);
            return View(await travelAgencyDbContext.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Tour)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null) return NotFound();
            return View(review);
        }

        // GET: Reviews/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName }), "ClientId", "FullName");
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName");
            var review = new Review
            {
                ReviewDate = DateTime.Now // Встановлюємо поточну дату для нового відгуку
            };
            return View(review);
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TourId,ClientId,Rating,CommentText,ReviewDate")] Review review)
        {
            // Перевірка на унікальність: один клієнт - один відгук на один тур
            bool reviewExists = await _context.Reviews
                .AnyAsync(r => r.ClientId == review.ClientId && r.TourId == review.TourId);
            if (reviewExists)
            {
                ModelState.AddModelError(string.Empty, "Цей клієнт вже залишив відгук на цей тур.");
            }

            // Перевірка існування обраних сутностей
            if (!await _context.Clients.AnyAsync(c => c.ClientId == review.ClientId))
                ModelState.AddModelError("ClientId", "Обраного клієнта не існує.");
            if (!await _context.Tours.AnyAsync(t => t.TourId == review.TourId))
                ModelState.AddModelError("TourId", "Обраного туру не існує.");


            if (ModelState.IsValid)
            {
                if (review.ReviewDate == default(DateTime)) // Якщо дата не прийшла з форми, встановимо поточну
                {
                    review.ReviewDate = DateTime.Now;
                }
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName }), "ClientId", "FullName", review.ClientId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", review.TourId);
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName }), "ClientId", "FullName", review.ClientId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", review.TourId);
            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,TourId,ClientId,Rating,CommentText,ReviewDate")] Review review)
        {
            if (id != review.ReviewId) return NotFound();

            // Перевірка на унікальність при редагуванні (якщо змінився клієнт або тур)
            // Ігноруємо поточний відгук при перевірці
            bool reviewExists = await _context.Reviews
                .AnyAsync(r => r.ReviewId != review.ReviewId && r.ClientId == review.ClientId && r.TourId == review.TourId);
            if (reviewExists)
            {
                ModelState.AddModelError(string.Empty, "Цей клієнт вже залишив відгук на цей тур.");
            }

            if (!await _context.Clients.AnyAsync(c => c.ClientId == review.ClientId))
                ModelState.AddModelError("ClientId", "Обраного клієнта не існує.");
            if (!await _context.Tours.AnyAsync(t => t.TourId == review.TourId))
                ModelState.AddModelError("TourId", "Обраного туру не існує.");

            if (ModelState.IsValid)
            {
                try
                {
                    // Можливо, варто оновити ReviewDate при редагуванні
                    // review.ReviewDate = DateTime.Now;
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName }), "ClientId", "FullName", review.ClientId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", review.TourId);
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var review = await _context.Reviews
                .Include(r => r.Client)
                .Include(r => r.Tour)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null) return NotFound();
            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ForRecentTours(DateTime? afterDate)
        {
            ViewData["CurrentAfterDate"] = afterDate?.ToString("yyyy-MM-dd"); // Для input type="date"

            if (_context.Reviews == null)
            {
                return Problem("Entity set 'TravelAgencyDbContext.Reviews' is null.");
            }

            var reviewsQuery = _context.Reviews
                                     .Include(r => r.Tour)
                                        .ThenInclude(t => t.Country) 
                                     .Include(r => r.Client)
                                     .AsQueryable();

            if (afterDate.HasValue)
            {
                
                DateTime dateOnlyAfter = afterDate.Value.Date;
                reviewsQuery = reviewsQuery.Where(r => r.Tour.StartDate.Date >= dateOnlyAfter);
            }

            var reviews = await reviewsQuery.OrderByDescending(r => r.Tour.StartDate).ThenByDescending(r => r.ReviewDate).ToListAsync();

            if (!reviews.Any() && afterDate.HasValue)
            {
                ViewData["NoResultsMessage"] = "Відгуків на тури, що починаються після вказаної дати, не знайдено.";
            }
            else if (!afterDate.HasValue && !reviews.Any())
            {
                ViewData["NoResultsMessage"] = "Відгуків ще немає.";
            }
            else if (!afterDate.HasValue && reviews.Any())
            {
                ViewData["InfoMessage"] = "Показано всі відгуки. Вкажіть дату, щоб відфільтрувати тури, що почалися після неї.";
            }

            return View(reviews); 
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.ReviewId == id);
        }
    }
}