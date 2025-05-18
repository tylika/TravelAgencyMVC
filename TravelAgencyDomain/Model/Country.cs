using System;
using System.Collections.Generic;

namespace TravelAgencyDomain.Model;

public partial class Country
{
    public int CountryId { get; set; }

    public string CountryName { get; set; } = null!;

    public bool VisaRequired { get; set; }

    public string? Continent { get; set; }

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
