using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Для List<T>
using TravelAgencyDomain.Model;    // Ваші моделі (Client, Tour, Country, Booking, Employee)
using TravelAgencyInfrastructure.Models; // Для ErrorViewModel
// Переконайтеся, що ваш DbContext знаходиться в цьому namespace, або змініть using
// Якщо DbContext в TravelAgencyInfrastructure (як у вашому ProjectEssentials.txt), то цей using не потрібен
// using TravelAgencyInfrastructure; 

namespace TravelAgencyInfrastructure.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TravelAgencyDbContext _context; // Ваш DbContext

        public HomeController(ILogger<HomeController> logger, TravelAgencyDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Сторінка зі списком простих запитів
        public IActionResult SimpleQueriesList()
        {
            ViewData["Title"] = "Список Простих Запитів";
            return View();
        }

        // Сторінка зі списком складних запитів
        public IActionResult ComplexQueriesList()
        {
            ViewData["Title"] = "Список Складних Запитів";
            return View();
        }

        // СКЛАДНИЙ ЗАПИТ №1: Клієнти, що забронювали ВСІ тури до вказаної країни
        // GET: Home/ClientsBookedAllToursInCountry
        public async Task<IActionResult> ClientsBookedAllToursInCountry(string? countryName)
        {
            ViewData["CurrentCountryName"] = countryName;
            var resultClients = new List<Client>();

            if (string.IsNullOrWhiteSpace(countryName))
            {
                ViewData["InfoMessage"] = "Будь ласка, введіть назву країни для пошуку.";
                return View("ClientsBookedAllToursInCountry", resultClients);
            }

            if (_context.Countries == null || _context.Tours == null || _context.Clients == null || _context.Bookings == null)
            {
                return Problem("Помилка конфігурації: один або декілька наборів даних (DbSet) не ініціалізовані.");
            }

            var targetCountry = await _context.Countries
                                          .FirstOrDefaultAsync(c => c.CountryName.ToLower() == countryName.ToLower());

            if (targetCountry == null)
            {
                ViewData["ErrorMessage"] = $"Країну з назвою '{countryName}' не знайдено.";
                return View("ClientsBookedAllToursInCountry", resultClients);
            }

            var tourIdsInTargetCountry = await _context.Tours
                                                .Where(t => t.CountryId == targetCountry.CountryId)
                                                .Select(t => t.TourId)
                                                .Distinct()
                                                .ToListAsync();

            if (!tourIdsInTargetCountry.Any())
            {
                ViewData["InfoMessage"] = $"Для країни '{countryName}' немає доступних турів для аналізу.";
                return View("ClientsBookedAllToursInCountry", resultClients);
            }

            var potentialClients = await _context.Clients
                .Where(c => c.Bookings.Any(b => b.Tour.CountryId == targetCountry.CountryId))
                .Select(c => new
                {
                    ClientObject = c,
                    BookedTourIdsInCountry = c.Bookings
                                             .Where(b => b.Tour.CountryId == targetCountry.CountryId)
                                             .Select(b => b.TourId)
                                             .Distinct()
                                             .ToList()
                })
                .ToListAsync();

            foreach (var pc in potentialClients)
            {
                bool bookedAllTours = !tourIdsInTargetCountry.Except(pc.BookedTourIdsInCountry).Any();
                if (bookedAllTours)
                {
                    resultClients.Add(pc.ClientObject);
                }
            }

            if (!resultClients.Any() && !string.IsNullOrEmpty(countryName))
            {
                ViewData["InfoMessage"] = $"Не знайдено клієнтів, які забронювали всі тури до країни '{countryName}'.";
            }

            return View("ClientsBookedAllToursInCountry", resultClients);
        }

        // СКЛАДНИЙ ЗАПИТ №2 (параметризований): Клієнти, що бронювали такий самий набір турів, як і вказаний клієнт
        // GET: Home/ClientsWithSameTourSetAsClient
        public async Task<IActionResult> ClientsWithSameTourSetAsClient(int? clientId)
        {
            ViewData["Title"] = "Клієнти з таким же набором турів";

            var clientListItems = new List<SelectListItem>();
            if (_context.Clients != null)
            {
                clientListItems = await _context.Clients
                   .Select(c => new SelectListItem { Value = c.ClientId.ToString(), Text = c.FirstName + " " + c.LastName })
                   .OrderBy(c => c.Text)
                   .ToListAsync();
            }
            ViewData["ClientIdList"] = new SelectList(clientListItems, "Value", "Text", clientId);
            ViewData["SelectedClientId"] = clientId;

            var resultClients = new List<Client>();

            if (!clientId.HasValue || clientId.Value == 0)
            {
                ViewData["InfoMessage"] = "Будь ласка, оберіть клієнта для порівняння.";
                return View("ClientsWithSameTourSetAsClient", resultClients);
            }

            if (_context.Clients == null || _context.Bookings == null || _context.Tours == null) // Перевіряємо на null перед використанням
            {
                return Problem("Один або декілька необхідних DbSet не ініціалізовані.");
            }

            var targetClient = await _context.Clients
                                        .Include(c => c.Bookings)
                                            .ThenInclude(b => b.Tour) // Потрібно для доступу до TourId з Bookings
                                        .FirstOrDefaultAsync(c => c.ClientId == clientId.Value);

            if (targetClient == null)
            {
                ViewData["ErrorMessage"] = $"Клієнта з ID {clientId.Value} не знайдено.";
                return View("ClientsWithSameTourSetAsClient", resultClients);
            }

            var targetClientTourIds = targetClient.Bookings
                                                .Select(b => b.TourId)
                                                .Distinct()
                                                .OrderBy(id => id)
                                                .ToList();

            if (!targetClientTourIds.Any())
            {
                ViewData["InfoMessage"] = $"Клієнт {targetClient.FirstName} {targetClient.LastName} не має заброньованих турів для порівняння.";
                return View("ClientsWithSameTourSetAsClient", resultClients);
            }

            ViewData["TargetClientName"] = $"{targetClient.FirstName} {targetClient.LastName}";
            ViewData["TargetClientTourIds"] = string.Join(", ", targetClientTourIds);

            var otherClientsWithBookedTourIds = await _context.Clients
                .Where(c => c.ClientId != clientId.Value && c.Bookings.Any())
                .Select(c => new
                {
                    Client = c,
                    BookedTourIds = c.Bookings.Select(b => b.TourId).Distinct().OrderBy(id => id).ToList()
                })
                .ToListAsync();

            foreach (var otherClientData in otherClientsWithBookedTourIds)
            {
                if (targetClientTourIds.SequenceEqual(otherClientData.BookedTourIds))
                {
                    resultClients.Add(otherClientData.Client);
                }
            }

            if (!resultClients.Any())
            {
                ViewData["InfoMessage"] = $"Не знайдено інших клієнтів, які б забронювали точно такий самий набір турів, як {targetClient.FirstName} {targetClient.LastName}.";
            }

            return View("ClientsWithSameTourSetAsClient", resultClients);
        }

        // СКЛАДНИЙ ЗАПИТ №3 (без ViewModel): Співробітники та кількість оформлених ними бронювань на тури вартістю понад X
        // GET: Home/EmployeeBookingStatsByTourPrice
        public async Task<IActionResult> EmployeeBookingStatsByTourPrice(decimal? minTourPrice)
        {
            decimal priceThreshold = minTourPrice ?? 15000;
            ViewData["CurrentMinTourPrice"] = priceThreshold;
            ViewData["Title"] = $"Статистика бронювань співробітників (тури дорожчі за {priceThreshold:C0})";

            if (_context.Employees == null || _context.Bookings == null || _context.Tours == null)
            {
                return Problem("Один або декілька необхідних DbSet не ініціалізовані.");
            }

            // 1. Отримуємо співробітників. Включаємо всі бронювання та пов'язані тури для кожного бронювання.
            var employeesWithBookingsAndTours = await _context.Employees
                .Include(e => e.Bookings)          // Включаємо бронювання співробітника
                    .ThenInclude(b => b.Tour)      // ДЛЯ КОЖНОГО БРОНЮВАННЯ включаємо пов'язаний ТУР
                .Where(e => e.Bookings.Any(b => b.Tour != null && b.Tour.PricePerPerson > priceThreshold)) // Фільтруємо співробітників, які мають хоча б одне "дороге" бронювання
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            // 2. Тепер для кожного відібраного співробітника ми рахуємо кількість "дорогих" бронювань.
            //    Оскільки ми вже завантажили .Tour для кожного Booking, ця операція буде безпечною.
            var expensiveBookingCounts = new Dictionary<int, int>();
            foreach (var emp in employeesWithBookingsAndTours)
            {
                // Переконуємося, що Tour не null перед доступом до PricePerPerson (хоча фільтр .Where вище вже мав це забезпечити)
                expensiveBookingCounts[emp.EmployeeId] = emp.Bookings
                                                    .Count(b => b.Tour != null && b.Tour.PricePerPerson > priceThreshold);
            }
            ViewData["ExpensiveBookingCounts"] = expensiveBookingCounts;


            if (!employeesWithBookingsAndTours.Any())
            {
                ViewData["InfoMessage"] = $"Не знайдено співробітників, які б оформлювали бронювання на тури дорожчі за {priceThreshold:C0}.";
            }

            // Передаємо у View список відфільтрованих співробітників
            return View("EmployeeBookingStatsByTourPrice", employeesWithBookingsAndTours);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}