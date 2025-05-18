using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // ��� SelectList
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // ��� List<T>
using TravelAgencyDomain.Model;    // ���� ����� (Client, Tour, Country, Booking, Employee)
using TravelAgencyInfrastructure.Models; // ��� ErrorViewModel
// �������������, �� ��� DbContext ����������� � ����� namespace, ��� ����� using
// ���� DbContext � TravelAgencyInfrastructure (�� � ������ ProjectEssentials.txt), �� ��� using �� �������
// using TravelAgencyInfrastructure; 

namespace TravelAgencyInfrastructure.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TravelAgencyDbContext _context; // ��� DbContext

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

        // ������� � ������� ������� ������
        public IActionResult SimpleQueriesList()
        {
            ViewData["Title"] = "������ ������� ������";
            return View();
        }

        // ������� � ������� �������� ������
        public IActionResult ComplexQueriesList()
        {
            ViewData["Title"] = "������ �������� ������";
            return View();
        }

        // �������� ����� �1: �볺���, �� ����������� �Ѳ ���� �� ������� �����
        // GET: Home/ClientsBookedAllToursInCountry
        public async Task<IActionResult> ClientsBookedAllToursInCountry(string? countryName)
        {
            ViewData["CurrentCountryName"] = countryName;
            var resultClients = new List<Client>();

            if (string.IsNullOrWhiteSpace(countryName))
            {
                ViewData["InfoMessage"] = "���� �����, ������ ����� ����� ��� ������.";
                return View("ClientsBookedAllToursInCountry", resultClients);
            }

            if (_context.Countries == null || _context.Tours == null || _context.Clients == null || _context.Bookings == null)
            {
                return Problem("������� ������������: ���� ��� ������� ������ ����� (DbSet) �� �����������.");
            }

            var targetCountry = await _context.Countries
                                          .FirstOrDefaultAsync(c => c.CountryName.ToLower() == countryName.ToLower());

            if (targetCountry == null)
            {
                ViewData["ErrorMessage"] = $"����� � ������ '{countryName}' �� ��������.";
                return View("ClientsBookedAllToursInCountry", resultClients);
            }

            var tourIdsInTargetCountry = await _context.Tours
                                                .Where(t => t.CountryId == targetCountry.CountryId)
                                                .Select(t => t.TourId)
                                                .Distinct()
                                                .ToListAsync();

            if (!tourIdsInTargetCountry.Any())
            {
                ViewData["InfoMessage"] = $"��� ����� '{countryName}' ���� ��������� ���� ��� ������.";
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
                ViewData["InfoMessage"] = $"�� �������� �볺���, �� ����������� �� ���� �� ����� '{countryName}'.";
            }

            return View("ClientsBookedAllToursInCountry", resultClients);
        }

        // �������� ����� �2 (����������������): �볺���, �� ��������� ����� ����� ���� ����, �� � �������� �볺��
        // GET: Home/ClientsWithSameTourSetAsClient
        public async Task<IActionResult> ClientsWithSameTourSetAsClient(int? clientId)
        {
            ViewData["Title"] = "�볺��� � ����� �� ������� ����";

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
                ViewData["InfoMessage"] = "���� �����, ������ �볺��� ��� ���������.";
                return View("ClientsWithSameTourSetAsClient", resultClients);
            }

            if (_context.Clients == null || _context.Bookings == null || _context.Tours == null) // ���������� �� null ����� �������������
            {
                return Problem("���� ��� ������� ���������� DbSet �� �����������.");
            }

            var targetClient = await _context.Clients
                                        .Include(c => c.Bookings)
                                            .ThenInclude(b => b.Tour) // ������� ��� ������� �� TourId � Bookings
                                        .FirstOrDefaultAsync(c => c.ClientId == clientId.Value);

            if (targetClient == null)
            {
                ViewData["ErrorMessage"] = $"�볺��� � ID {clientId.Value} �� ��������.";
                return View("ClientsWithSameTourSetAsClient", resultClients);
            }

            var targetClientTourIds = targetClient.Bookings
                                                .Select(b => b.TourId)
                                                .Distinct()
                                                .OrderBy(id => id)
                                                .ToList();

            if (!targetClientTourIds.Any())
            {
                ViewData["InfoMessage"] = $"�볺�� {targetClient.FirstName} {targetClient.LastName} �� �� ������������� ���� ��� ���������.";
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
                ViewData["InfoMessage"] = $"�� �������� ����� �볺���, �� � ����������� ����� ����� ����� ���� ����, �� {targetClient.FirstName} {targetClient.LastName}.";
            }

            return View("ClientsWithSameTourSetAsClient", resultClients);
        }

        // �������� ����� �3 (��� ViewModel): ����������� �� ������� ���������� ���� ��������� �� ���� ������� ����� X
        // GET: Home/EmployeeBookingStatsByTourPrice
        public async Task<IActionResult> EmployeeBookingStatsByTourPrice(decimal? minTourPrice)
        {
            decimal priceThreshold = minTourPrice ?? 15000;
            ViewData["CurrentMinTourPrice"] = priceThreshold;
            ViewData["Title"] = $"���������� ��������� ����������� (���� ������� �� {priceThreshold:C0})";

            if (_context.Employees == null || _context.Bookings == null || _context.Tours == null)
            {
                return Problem("���� ��� ������� ���������� DbSet �� �����������.");
            }

            // 1. �������� �����������. �������� �� ���������� �� ���'���� ���� ��� ������� ����������.
            var employeesWithBookingsAndTours = await _context.Employees
                .Include(e => e.Bookings)          // �������� ���������� �����������
                    .ThenInclude(b => b.Tour)      // ��� ������� ���������� �������� ���'������ ���
                .Where(e => e.Bookings.Any(b => b.Tour != null && b.Tour.PricePerPerson > priceThreshold)) // Գ������� �����������, �� ����� ���� � ���� "������" ����������
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            // 2. ����� ��� ������� ��������� ����������� �� ������ ������� "�������" ���������.
            //    ������� �� ��� ����������� .Tour ��� ������� Booking, �� �������� ���� ���������.
            var expensiveBookingCounts = new Dictionary<int, int>();
            foreach (var emp in employeesWithBookingsAndTours)
            {
                // ������������, �� Tour �� null ����� �������� �� PricePerPerson (���� ������ .Where ���� ��� ��� �� �����������)
                expensiveBookingCounts[emp.EmployeeId] = emp.Bookings
                                                    .Count(b => b.Tour != null && b.Tour.PricePerPerson > priceThreshold);
            }
            ViewData["ExpensiveBookingCounts"] = expensiveBookingCounts;


            if (!employeesWithBookingsAndTours.Any())
            {
                ViewData["InfoMessage"] = $"�� �������� �����������, �� � ����������� ���������� �� ���� ������� �� {priceThreshold:C0}.";
            }

            // �������� � View ������ �������������� �����������
            return View("EmployeeBookingStatsByTourPrice", employeesWithBookingsAndTours);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}