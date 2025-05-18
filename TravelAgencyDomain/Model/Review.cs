using System;
using System.Collections.Generic;

namespace TravelAgencyDomain.Model;

public partial class Review
{
    public int ReviewId { get; set; }

    public int TourId { get; set; }

    public int ClientId { get; set; }

    public int Rating { get; set; }

    public string? CommentText { get; set; }

    public DateTime ReviewDate { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
