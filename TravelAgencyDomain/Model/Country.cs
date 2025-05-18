using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelAgencyDomain.Model;

public partial class Country
{
    public int CountryId { get; set; }
    [Display(Name = "Країна")]
    [Required(ErrorMessage = "Назва країни є обов'язковим полем.")]
    [StringLength(100, ErrorMessage = "Назва країни не може перевищувати 100 символів.")]
    [RegularExpression(@"^[^0-9]*$", ErrorMessage = "Назва країни не може містити цифри.")]
    public string CountryName { get; set; } = null!;
    [Display(Name = "Чи потрібна віза")]
    public bool VisaRequired { get; set; }
    [Display(Name = "Частина світу")]
    [StringLength(50, ErrorMessage = "Назва частини світу не може перевищувати 50 символів.")]
    [RegularExpression(@"^[^0-9]*$", ErrorMessage = "Назва частини світу не може містити цифри.")]
    public string? Continent { get; set; }
    [Display(Name = "Готель")]
    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
    [Display(Name = "Тур")]
    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
