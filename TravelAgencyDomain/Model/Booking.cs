using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyDomain.Model;

public partial class Booking
{
    [Display(Name = "ID Бронювання")]
    public int BookingId { get; set; }
    [Display(Name = "Клієнт")]
    [Required(ErrorMessage = "Необхідно обрати клієнта.")]
    public int ClientId { get; set; }
    [Display(Name = "Тур")]
    [Required(ErrorMessage = "Необхідно обрати тур.")]
    public int TourId { get; set; }
    [Display(Name = "Співробітник")]
    [Required(ErrorMessage = "Необхідно обрати співробітника.")]
    public int EmployeeId { get; set; }
    [Display(Name = "Дата бронювання")]
    [Required(ErrorMessage = "Дата бронювання є обов'язковою.")]
    [DataType(DataType.DateTime)]
    public DateTime BookingDate { get; set; }
    [Display(Name = "Кількість осіб")]
    [Required(ErrorMessage = "Кількість осіб є обов'язковим полем.")]
    [Range(1, 20, ErrorMessage = "Кількість осіб має бути від 1 до 20.")]
    public int NumberOfPeople { get; set; }
    [Display(Name = "Загальна вартість")]
    [Required(ErrorMessage = "Загальна вартість є обов'язковим полем.")]
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Загальна вартість має бути позитивним числом.")]
    public decimal TotalPrice { get; set; }
    [Display(Name = "Статус")]
    [Required(ErrorMessage = "Статус є обов'язковим полем.")]
    [StringLength(50, ErrorMessage = "Статус не може перевищувати 50 символів.")]
    public string Status { get; set; } = null!;
    [Display(Name = "Клієнт")]
    public virtual Client Client { get; set; } = null!;
    [Display(Name = "Працівник")]
    public virtual Employee Employee { get; set; } = null!;
    [Display(Name = "Тур")]
    public virtual Tour Tour { get; set; } = null!;
}
