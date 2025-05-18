using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyDomain.Model;

public partial class Tour
{
    public int TourId { get; set; }
    [Display(Name = "Назва туру")]
    [Required(ErrorMessage = "Назва туру є обов'язковим полем.")]
    [StringLength(200, ErrorMessage = "Назва туру не може перевищувати 200 символів.")]
    public string TourName { get; set; } = null!;
    [Display(Name = "Країна")]
    [Required(ErrorMessage = "Необхідно обрати країну.")]
    public int CountryId { get; set; }
    [Display(Name = "Готель")]
    [Required(ErrorMessage = "Необхідно обрати готель.")]
    public int HotelId { get; set; }
    [Display(Name = "Дата початку")]
    [Required(ErrorMessage = "Дата початку є обов'язковим полем.")]
    [DataType(DataType.DateTime)]
    public DateTime StartDate { get; set; }
    [Display(Name = "Тривалість (днів)")]
    [Required(ErrorMessage = "Тривалість є обов'язковим полем.")]
    [Range(1, 90, ErrorMessage = "Тривалість туру має бути від 1 до 90 днів.")]
    public int DurationDays { get; set; }
    [Display(Name = "Ціна за особу")]
    [Required(ErrorMessage = "Ціна є обов'язковим полем.")]
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Ціна має бути позитивним числом.")]
    public decimal PricePerPerson { get; set; }
    [Display(Name = "Тип транспорту")]
    [StringLength(50)]
    public string? TransportType { get; set; }
    [Display(Name = "Опис")]
    public string? Description { get; set; }
    [Display(Name = "Макс. учасників")]
    [Range(1, 200, ErrorMessage = "Максимальна кількість учасників має бути позитивним числом (до 200).")]
    public int? MaxParticipants { get; set; }
    [Display(Name = "Бронювання")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    [Display(Name = "Країна")]
    [ForeignKey("CountryId")]
    public virtual Country Country { get; set; } = null!;
    [Display(Name = "Готель")]
    [ForeignKey("HotelId")]
    public virtual Hotel Hotel { get; set; } = null!;
    [Display(Name = "Відгуки")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
