// Файл: TravelAgencyInfrastructure/Controllers/BookingsController.cs
using System;
using System.Collections.Generic; // Для List<SelectListItem>
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList та SelectListItem
using Microsoft.EntityFrameworkCore;
using TravelAgencyDomain.Model;
using TravelAgencyInfrastructure;

namespace TravelAgencyInfrastructure.Controllers
{
    public class BookingsController : Controller
    {
        private readonly TravelAgencyDbContext _context;

        public BookingsController(TravelAgencyDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var travelAgencyDbContext = _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Employee)
                .Include(b => b.Tour);
            return View(await travelAgencyDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Employee)
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            
            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName }), "ClientId", "FullName");
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName }), "EmployeeId", "FullName");
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName");

            ViewData["StatusList"] = GetStatusSelectList();

            var booking = new Booking
            {
                BookingDate = DateTime.Now // Встановлюємо поточну дату для нового бронювання
            };
            return View(booking);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,TourId,EmployeeId,BookingDate,NumberOfPeople,TotalPrice,Status")] Booking booking)
        {
            // Валідація дати бронювання
            if (booking.BookingDate < DateTime.Today.AddYears(-1) || booking.BookingDate > DateTime.Today.AddYears(1))
            {
                ModelState.AddModelError("BookingDate", "Дата бронювання має бути в межах одного року від поточної дати.");
            }

            var clientExists = await _context.Clients.AnyAsync(c => c.ClientId == booking.ClientId);
            var tourExists = await _context.Tours.AnyAsync(t => t.TourId == booking.TourId);
            var employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == booking.EmployeeId);

            if (!clientExists) ModelState.AddModelError("ClientId", "Обраного клієнта не існує.");
            if (!tourExists) ModelState.AddModelError("TourId", "Обраного туру не існує.");
            if (!employeeExists) ModelState.AddModelError("EmployeeId", "Обраного співробітника не існує.");

            var tour = await _context.Tours.FindAsync(booking.TourId);
            if (tour != null && booking.NumberOfPeople > 0)
            {
                // Розраховуємо TotalPrice тільки якщо воно не було встановлено (або = 0)
                if (booking.TotalPrice == 0)
                {
                    booking.TotalPrice = tour.PricePerPerson * booking.NumberOfPeople;
                }
            }
            else if (tour == null && ModelState.ContainsKey("TourId") && !ModelState["TourId"].Errors.Any()) // Якщо помилки по TourId ще не було
            {
                ModelState.AddModelError("TourId", "Не вдалося знайти тур для розрахунку ціни.");
            }

            // Перевірка, чи обраний статус є допустимим
            var validStatuses = GetStatusSelectList().Select(s => s.Value);
            if (!validStatuses.Contains(booking.Status))
            {
                ModelState.AddModelError("Status", "Обрано некоректний статус.");
            }
            ModelState.Remove("Status");
            ModelState.Remove("Client");
            ModelState.Remove("Employee");
            ModelState.Remove("Tour");

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName }), "ClientId", "FullName", booking.ClientId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName }), "EmployeeId", "FullName", booking.EmployeeId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", booking.TourId);
            ViewData["StatusList"] = GetStatusSelectList(booking.Status);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName }), "ClientId", "FullName", booking.ClientId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName }), "EmployeeId", "FullName", booking.EmployeeId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", booking.TourId);
            ViewData["StatusList"] = GetStatusSelectList(booking.Status);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,ClientId,TourId,EmployeeId,BookingDate,NumberOfPeople,TotalPrice,Status")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (booking.BookingDate < DateTime.Today.AddYears(-1) || booking.BookingDate > DateTime.Today.AddYears(1))
            {
                ModelState.AddModelError("BookingDate", "Дата бронювання має бути в межах одного року від поточної дати.");
            }
            if (!await _context.Clients.AnyAsync(c => c.ClientId == booking.ClientId))
                ModelState.AddModelError("ClientId", "Обраного клієнта не існує.");
            if (!await _context.Tours.AnyAsync(t => t.TourId == booking.TourId))
                ModelState.AddModelError("TourId", "Обраного туру не існує.");
            if (!await _context.Employees.AnyAsync(e => e.EmployeeId == booking.EmployeeId))
                ModelState.AddModelError("EmployeeId", "Обраного співробітника не існує.");

            var tour = await _context.Tours.FindAsync(booking.TourId);
            if (tour != null && booking.NumberOfPeople > 0)
            {
                if (booking.TotalPrice == 0)
                {
                    booking.TotalPrice = tour.PricePerPerson * booking.NumberOfPeople;
                }
            }
            else if (tour == null && ModelState.ContainsKey("TourId") && !ModelState["TourId"].Errors.Any())
            {
                ModelState.AddModelError("TourId", "Не вдалося знайти тур для розрахунку ціни.");
            }

            var validStatuses = GetStatusSelectList().Select(s => s.Value);
            if (!validStatuses.Contains(booking.Status))
            {
                ModelState.AddModelError("Status", "Обрано некоректний статус.");
            }
            ModelState.Remove("Status");
            ModelState.Remove("Client");
            ModelState.Remove("Employee");
            ModelState.Remove("Tour");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName }), "ClientId", "FullName", booking.ClientId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName }), "EmployeeId", "FullName", booking.EmployeeId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourName", booking.TourId);
            ViewData["StatusList"] = GetStatusSelectList(booking.Status);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Employee)
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }

        // Допоміжний метод для створення списку статусів
        private SelectList GetStatusSelectList(string? selectedStatus = null)
        {
            var statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "PendingPayment", Text = "Очікує оплати" },
                new SelectListItem { Value = "Confirmed", Text = "Підтверджено" },
                new SelectListItem { Value = "Cancelled", Text = "Скасовано" },
                new SelectListItem { Value = "Completed", Text = "Завершено" }
                // Додайте інші статуси, якщо потрібно
            };
            return new SelectList(statuses, "Value", "Text", selectedStatus);
        }
        // GET: Bookings/SearchByEmployeeAndDate
        public async Task<IActionResult> SearchByEmployeeAndDate(int? employeeId, DateTime? startDate, DateTime? endDate)
        {
            // Заповнюємо ViewData для випадаючого списку співробітників та збереження параметрів
            ViewData["EmployeeIdList"] = new SelectList(_context.Employees.Select(e => new { e.EmployeeId, FullName = e.FirstName + " " + e.LastName }), "EmployeeId", "FullName", employeeId);
            ViewData["CurrentEmployeeId"] = employeeId;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-ddTHH:mm"); // Для datetime-local
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-ddTHH:mm");     // Для datetime-local

            if (_context.Bookings == null)
            {
                return Problem("Entity set 'TravelAgencyDbContext.Bookings' is null.");
            }

            var bookingsQuery = _context.Bookings
                                      .Include(b => b.Client)
                                      .Include(b => b.Tour)
                                      .Include(b => b.Employee)
                                      .AsQueryable();

            bool hasParameters = false;

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                bookingsQuery = bookingsQuery.Where(b => b.EmployeeId == employeeId.Value);
                hasParameters = true;
            }

            if (startDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= startDate.Value);
                hasParameters = true;
            }

            if (endDate.HasValue)
            {
                // Додаємо один день до endDate, щоб включити весь день endDate
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate < endDate.Value.AddDays(1));
                hasParameters = true;
            }

            List<Booking> bookingsResult;
            if (hasParameters)
            {
                bookingsResult = await bookingsQuery.OrderByDescending(b => b.BookingDate).ToListAsync();
            }
            else
            {
               
                bookingsResult = new List<Booking>();
                if (employeeId.HasValue || startDate.HasValue || endDate.HasValue)
                {
                    
                }
                else
                {
                    ViewData["NoParametersMessage"] = "Будь ласка, оберіть співробітника та/або вкажіть діапазон дат для пошуку.";
                }

            }
            return View(bookingsResult);
        }

    }
}