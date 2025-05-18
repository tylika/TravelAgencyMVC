using System;
using System.Collections.Generic;

namespace TravelAgencyDomain.Model;

public partial class Hotel
{
    public int HotelId { get; set; }

    public string HotelName { get; set; } = null!;

    public int CountryId { get; set; }

    public string? City { get; set; }

    public int StarRating { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
