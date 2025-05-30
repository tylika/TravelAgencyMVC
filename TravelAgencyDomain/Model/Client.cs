﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelAgencyDomain.Model;

public partial class Client
{
    public int ClientId { get; set; }

    [Display(Name = "Ім'я")]
    [Required(ErrorMessage = "Поле 'Ім'я' є обов'язковим.")]
    [StringLength(50, ErrorMessage = "Ім'я не може перевищувати 50 символів.")]
    [RegularExpression(@"^[А-ЯІЇЄҐA-Z][а-яіїєґa-z'-]*$", ErrorMessage = "Ім'я має починатися з великої літери та містити тільки літери, апостроф або дефіс.")]
    public string FirstName { get; set; } = null!;
    [Display(Name = "Прізвище")]
    [Required(ErrorMessage = "Поле 'Прізвище' є обов'язковим.")]
    [StringLength(50, ErrorMessage = "Прізвище не може перевищувати 50 символів.")]
    [RegularExpression(@"^[А-ЯІЇЄҐA-Z][а-яіїєґa-z'-]*$", ErrorMessage = "Прізвище має починатися з великої літери та містити тільки літери, апостроф або дефіс.")]
    public string LastName { get; set; } = null!;
    [Display(Name = "Номер телефону")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Номер телефону має складатися рівно з 10 цифр.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Номер телефону має складатися рівно з 10 цифр.")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }
    [Display(Name = "Електронна пошта")]
    [StringLength(100, ErrorMessage = "Електронна пошта не може перевищувати 100 символів.")]
    [EmailAddress(ErrorMessage = "Некоректний формат електронної пошти.")]
    public string? Email { get; set; }
    [Display(Name = "Дата народження")]
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public static ValidationResult? ValidateAgeRange(DateOnly? dateOfBirth, ValidationContext context)
    {
        if (dateOfBirth == null)
        {
            return new ValidationResult("Дата народження є обов'язковим полем.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        
        var minBirthDateFor18 = today.AddYears(-18); 
        var maxBirthDateFor100 = today.AddYears(-100); 

        if (dateOfBirth > minBirthDateFor18) 
        {
            return new ValidationResult($"Клієнту має бути щонайменше 18 років. Максимально допустима дата народження: {minBirthDateFor18:yyyy-MM-dd}.");
        }

        if (dateOfBirth < maxBirthDateFor100) 
        {
            return new ValidationResult($"Вік клієнта не може перевищувати 100 років. Мінімально допустима дата народження: {maxBirthDateFor100:yyyy-MM-dd}.");
        }

        return ValidationResult.Success;
    }

}
