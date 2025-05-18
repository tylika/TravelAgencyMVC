using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelAgencyDomain.Model;

public partial class Employee
{
    public int EmployeeId { get; set; }
    [Display(Name = "Ім'я")]
    [Required(ErrorMessage = "Ім'я співробітника є обов'язковим.")]
    [StringLength(50, ErrorMessage = "Ім'я не може перевищувати 50 символів.")]
    public string FirstName { get; set; } = null!;
    [Display(Name = "Прізвище")]
    [Required(ErrorMessage = "Прізвище співробітника є обов'язковим.")]
    [StringLength(50, ErrorMessage = "Прізвище не може перевищувати 50 символів.")]
    public string LastName { get; set; } = null!;
    [Display(Name = "Посада")]
    [StringLength(100, ErrorMessage = "Назва посади не може перевищувати 100 символів.")]
    public string? Position { get; set; }
    [Display(Name = "Дата прийому")]
    [DataType(DataType.Date)]
    public DateOnly? HireDate { get; set; }
    [Display(Name = "Номер телефону")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Номер телефону має складатися рівно з 10 цифр.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Номер телефону має складатися рівно з 10 цифр (тільки цифри).")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }
    [Display(Name = "Електронна пошта")]
    [StringLength(100, ErrorMessage = "Електронна пошта не може перевищувати 100 символів.")]
    [EmailAddress(ErrorMessage = "Некоректний формат електронної пошти.")]
    public string? Email { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
