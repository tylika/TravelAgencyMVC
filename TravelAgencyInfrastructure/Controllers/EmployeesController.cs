
using System;
using System.Globalization; 
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyDomain.Model;
using TravelAgencyInfrastructure;

namespace TravelAgencyInfrastructure.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly TravelAgencyDbContext _context;

        public EmployeesController(TravelAgencyDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Position,HireDate,PhoneNumber,Email")] Employee employee)
        {
            // Автоматична зміна регістру
            if (!string.IsNullOrEmpty(employee.FirstName))
            {
                employee.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employee.FirstName.ToLower());
            }
            if (!string.IsNullOrEmpty(employee.LastName))
            {
                employee.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employee.LastName.ToLower());
            }

            // Валідація HireDate (не в майбутньому)
            if (employee.HireDate.HasValue && employee.HireDate.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("HireDate", "Дата прийому на роботу не може бути в майбутньому.");
            }

            // Перевірка на унікальність Email (якщо вказано)
            if (!string.IsNullOrEmpty(employee.Email) && await _context.Employees.AnyAsync(e => e.Email == employee.Email))
            {
                ModelState.AddModelError("Email", "Співробітник з такою електронною поштою вже існує.");
            }

            // Перевірка на унікальність PhoneNumber (якщо вказано)
            if (!string.IsNullOrEmpty(employee.PhoneNumber) && await _context.Employees.AnyAsync(e => e.PhoneNumber == employee.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Співробітник з таким номером телефону вже існує.");
            }


            ModelState.Remove("LastName");
            ModelState.Remove("FirstName");
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,FirstName,LastName,Position,HireDate,PhoneNumber,Email")] Employee employee)
        {
            if (id != employee.EmployeeId) return NotFound();

            // Автоматична зміна регістру
            if (!string.IsNullOrEmpty(employee.FirstName))
            {
                employee.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employee.FirstName.ToLower());
            }
            if (!string.IsNullOrEmpty(employee.LastName))
            {
                employee.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(employee.LastName.ToLower());
            }

            // Валідація HireDate
            if (employee.HireDate.HasValue && employee.HireDate.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("HireDate", "Дата прийому на роботу не може бути в майбутньому.");
            }

            // Перевірка на унікальність Email (окрім поточного запису)
            if (!string.IsNullOrEmpty(employee.Email) && await _context.Employees.AnyAsync(e => e.EmployeeId != employee.EmployeeId && e.Email == employee.Email))
            {
                ModelState.AddModelError("Email", "Інший співробітник з такою електронною поштою вже існує.");
            }

            // Перевірка на унікальність PhoneNumber (окрім поточного запису)
            if (!string.IsNullOrEmpty(employee.PhoneNumber) && await _context.Employees.AnyAsync(e => e.EmployeeId != employee.EmployeeId && e.PhoneNumber == employee.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "Інший співробітник з таким номером телефону вже існує.");
            }
            ModelState.Remove("LastName");
            ModelState.Remove("FirstName");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                // Перевірка на пов'язані бронювання
                bool hasBookings = await _context.Bookings.AnyAsync(b => b.EmployeeId == id);
                if (hasBookings)
                {
                    ModelState.AddModelError(string.Empty, "Неможливо видалити співробітника, оскільки за ним закріплені бронювання. Спочатку перепризначте або видаліть ці бронювання.");
                    
                    var employeeToDeleteView = await _context.Employees.FirstOrDefaultAsync(m => m.EmployeeId == id);
                    return View("Delete", employeeToDeleteView);
                }
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}