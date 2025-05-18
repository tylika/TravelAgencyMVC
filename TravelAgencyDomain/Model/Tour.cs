using System;
using System.Collections.Generic;

namespace TravelAgencyDomain.Model;

public partial class Tour
{
    public int TourId { get; set; }

    public string TourName { get; set; } = null!;

    public int CountryId { get; set; }

    public int HotelId { get; set; }

    public DateTime StartDate { get; set; }

    public int DurationDays { get; set; }

    public decimal PricePerPerson { get; set; }

    public string? TransportType { get; set; }

    public string? Description { get; set; }

    public int? MaxParticipants { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Country Country { get; set; } = null!;

    public virtual Hotel Hotel { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
