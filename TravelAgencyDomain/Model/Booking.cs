using System;
using System.Collections.Generic;

namespace TravelAgencyDomain.Model;

public partial class Booking
{
    public int BookingId { get; set; }

    public int ClientId { get; set; }

    public int TourId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime BookingDate { get; set; }

    public int NumberOfPeople { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
