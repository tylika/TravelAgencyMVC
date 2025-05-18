// Файл: TravelAgencyInfrastructure/Controllers/ClientsController.cs
using System;
using System.Globalization; // Для CultureInfo
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyDomain.Model;
using TravelAgencyInfrastructure; // Ваш DbContext namespace

namespace TravelAgencyInfrastructure.Controllers
{
    public class ClientsController : Controller
    {
        private readonly TravelAgencyDbContext _context;

        public ClientsController(TravelAgencyDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clients.ToListAsync());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FirstOrDefaultAsync(m => m.ClientId == id);
            if (client == null) return NotFound();
            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,PhoneNumber,Email,DateOfBirth")] Client client)
        {
            // 1. Автоматична зміна регістру для Ім'я та Прізвище
            if (!string.IsNullOrEmpty(client.FirstName))
            {
                client.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(client.FirstName.ToLower());
            }
            if (!string.IsNullOrEmpty(client.LastName))
            {
                client.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(client.LastName.ToLower());
            }

            
            // 5. Перевірка на унікальність (Ім'я + Прізвище + Пошта + Телефон)
            if (!string.IsNullOrEmpty(client.Email) || !string.IsNullOrEmpty(client.PhoneNumber))
            {
                bool exists = await _context.Clients.AnyAsync(c =>
                    c.FirstName == client.FirstName &&
                    c.LastName == client.LastName &&
                    ((!string.IsNullOrEmpty(client.Email) && c.Email == client.Email) ||
                     (!string.IsNullOrEmpty(client.PhoneNumber) && c.PhoneNumber == client.PhoneNumber))
                );
                if (exists)
                {
                    ModelState.AddModelError(string.Empty, "Клієнт з таким Ім'ям, Прізвищем та контактними даними (пошта/телефон) вже існує.");
                }
            }


            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,FirstName,LastName,PhoneNumber,Email,DateOfBirth")] Client client)
        {
            if (id != client.ClientId) return NotFound();

            // 1. Автоматична зміна регістру
            if (!string.IsNullOrEmpty(client.FirstName))
            {
                client.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(client.FirstName.ToLower());
            }
            if (!string.IsNullOrEmpty(client.LastName))
            {
                client.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(client.LastName.ToLower());
            }

            // 4. Валідація дати народження
            if (client.DateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var age = today.Year - client.DateOfBirth.Value.Year;
                if (client.DateOfBirth.Value > today.AddYears(-age)) age--;

                if (age < 18 || age > 100)
                {
                    ModelState.AddModelError("DateOfBirth", "Вік клієнта має бути від 18 до 100 років.");
                }
            }
            else
            {
                ModelState.AddModelError("DateOfBirth", "Дата народження є обов'язковою.");
            }

            // 5. Перевірка на унікальність (окрім поточного запису)
            if (!string.IsNullOrEmpty(client.Email) || !string.IsNullOrEmpty(client.PhoneNumber))
            {
                bool exists = await _context.Clients.AnyAsync(c =>
                    c.ClientId != client.ClientId && // Ігноруємо поточний запис
                    c.FirstName == client.FirstName &&
                    c.LastName == client.LastName &&
                    ((!string.IsNullOrEmpty(client.Email) && c.Email == client.Email) ||
                     (!string.IsNullOrEmpty(client.PhoneNumber) && c.PhoneNumber == client.PhoneNumber))
                );
                if (exists)
                {
                    ModelState.AddModelError(string.Empty, "Інший клієнт з таким Ім'ям, Прізвищем та контактними даними (пошта/телефон) вже існує.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.ClientId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var client = await _context.Clients.FirstOrDefaultAsync(m => m.ClientId == id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                // Перевірка на пов'язані бронювання або відгуки
                bool hasBookings = await _context.Bookings.AnyAsync(b => b.ClientId == id);
                bool hasReviews = await _context.Reviews.AnyAsync(r => r.ClientId == id);

                if (hasBookings || hasReviews)
                {
                    ModelState.AddModelError(string.Empty, "Неможливо видалити клієнта, оскільки є пов'язані бронювання або відгуки.");
                    // Потрібно передати модель назад у View, щоб відобразити помилку
                    return View("Delete", client);
                }
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Clients/SearchActiveClients
        public async Task<IActionResult> SearchActiveClients(string? searchTerm)
        {
            ViewData["CurrentSearchTerm"] = searchTerm;

            if (_context.Clients == null)
            {
                return Problem("Entity set 'TravelAgencyDbContext.Clients' is null.");
            }

            // Починаємо з IQueryable<Client>
            var clientsQuery = _context.Clients.AsQueryable();

            // Фільтрація за searchTerm, якщо він є
            if (!string.IsNullOrEmpty(searchTerm))
            {
                clientsQuery = clientsQuery.Where(c =>
                    (c.FirstName != null && c.FirstName.Contains(searchTerm)) ||
                    (c.LastName != null && c.LastName.Contains(searchTerm))
                );
            }

            // Фільтруємо клієнтів, щоб залишилися тільки ті, хто має хоча б одне бронювання
            // Це явно використовує зв'язок з таблицею Bookings для фільтрації
            clientsQuery = clientsQuery.Where(c => c.Bookings.Any());

            // Тепер вибираємо дані, включаючи кількість бронювань
            // Ми все ще можемо передати Client і розкрити Bookings.Count у View,
            // або створити ViewModel. Для простоти залишаємо передачу Client з .Include().
            var activeClients = await clientsQuery
                                        .Include(c => c.Bookings) // Включаємо бронювання для підрахунку та, можливо, для деталей
                                        .OrderBy(c => c.LastName).ThenBy(c => c.FirstName) // Сортування
                                        .ToListAsync();

            if (!activeClients.Any() && !string.IsNullOrEmpty(searchTerm))
            {
                ViewData["NoResultsMessage"] = "Активних клієнтів (з бронюваннями) за вашим запитом не знайдено.";
            }
            else if (!activeClients.Any() && string.IsNullOrEmpty(searchTerm))
            {
                // Якщо немає searchTerm і немає активних клієнтів взагалі
                ViewData["NoResultsMessage"] = "Активних клієнтів (з бронюваннями) не знайдено.";
            }


            return View(activeClients); // Потрібно створити View Views/Clients/SearchActiveClients.cshtml
        }
        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }
}