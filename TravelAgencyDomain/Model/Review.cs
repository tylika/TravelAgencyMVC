using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelAgencyDomain.Model;

public partial class Review
{
    [Display(Name = "ID Відгука")]
    public int ReviewId { get; set; }
    [Display(Name = "Тур")]
    [Required(ErrorMessage = "Необхідно обрати тур.")]
    public int TourId { get; set; }
    [Display(Name = "Клієнт")]
    [Required(ErrorMessage = "Необхідно обрати клієнта.")]
    public int ClientId { get; set; }
    [Display(Name = "Рейтинг")]
    [Required(ErrorMessage = "Рейтинг є обов'язковим.")]
    [Range(1, 5, ErrorMessage = "Рейтинг має бути від 1 до 5.")]
    public int Rating { get; set; }
    [Display(Name = "Відгук")]
    [DataType(DataType.MultilineText)]
    public string? CommentText { get; set; }
    [Display(Name = "Дата відгуку")]
    [Required(ErrorMessage = "Дата відгуку є обов'язковою.")]
    [DataType(DataType.DateTime)]
    public DateTime ReviewDate { get; set; }
    [Display(Name = "Клієнт")]
    public virtual Client Client { get; set; } = null!;
    [Display(Name = "Тур")]
    public virtual Tour Tour { get; set; } = null!;
}
