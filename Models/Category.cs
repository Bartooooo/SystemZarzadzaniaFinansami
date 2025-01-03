using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaFinansami.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Nazwa Kategoria musi mieć od 1 do 30 znaków.")]
        [Display(Name = "Nazwa")]
        public string Name { get; set; } = string.Empty; // Domyślna wartość

        [Required]
        public string UserId { get; set; } = string.Empty; // Dodano pole UserId

        public ICollection<Income> Incomes { get; set; } = new List<Income>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
