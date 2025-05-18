using System;
using System.Collections.Generic;

namespace TravelAgencyDomain.Model;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Position { get; set; }

    public DateOnly? HireDate { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
