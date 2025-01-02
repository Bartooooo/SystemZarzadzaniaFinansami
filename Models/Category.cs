using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaFinansami.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Nazwa Kategoria musi mieć od 3 do 30 znaków.")]
        [Display(Name = "Nazwa")]
        public string Name { get; set; } = string.Empty; // Domyślna wartość dla uniknięcia null

        public ICollection<Income> Incomes { get; set; } = new List<Income>(); // Domyślna kolekcja
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>(); // Domyślna kolekcja
    }
}
