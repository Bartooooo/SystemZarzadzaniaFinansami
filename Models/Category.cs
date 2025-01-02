using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaFinansami.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nazwa")]
        public string Name { get; set; } = string.Empty; // Domyślna wartość dla uniknięcia null

        public ICollection<Income> Incomes { get; set; } = new List<Income>(); // Domyślna kolekcja
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>(); // Domyślna kolekcja
    }
}
