using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelAgencyDomain.Model;

public partial class Hotel
{
    public int HotelId { get; set; }
    [Display(Name = "Назва Готелю")]
    [Required(ErrorMessage = "Назва готелю є обов'язковим полем.")]
    [StringLength(100, ErrorMessage = "Назва готелю не може перевищувати 100 символів.")]
    public string HotelName { get; set; } = null!;
    [Display(Name = "Країна")]
    [Required(ErrorMessage = "Необхідно обрати країну.")]
    public int CountryId { get; set; }

    [Display(Name = "Місто")]
    [StringLength(100, ErrorMessage = "Назва міста не може перевищувати 100 символів.")]
    [RegularExpression(@"^[^0-9]*$", ErrorMessage = "Назва міста не може містити цифри.")]
    public string? City { get; set; }
    [Display(Name = "Рейтинг (зірки)")]
    [Required(ErrorMessage = "Рейтинг є обов'язковим полем.")]
    [Range(1, 5, ErrorMessage = "Рейтинг має бути від 1 до 5.")]
    public int StarRating { get; set; }
    [Display(Name = "Адреса")]
    [StringLength(200, ErrorMessage = "Адреса не може перевищувати 200 символів.")]
    public string? Address { get; set; }
    [Display(Name = "Опис")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }
    [Display(Name = "Країна")]
    public virtual Country Country { get; set; } = null!;
    [Display(Name = "Тур")]
    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
