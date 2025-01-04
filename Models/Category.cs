using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SystemZarzadzaniaFinansami.Models
{
    /// <summary>
    /// Reprezentuje kategorię finansową użytkownika, która może być używana do klasyfikowania przychodów i wydatków.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unikalny identyfikator kategorii.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nazwa kategorii finansowej.
        /// </summary>
        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Nazwa kategorii musi mieć od 1 do 30 znaków.")]
        [Display(Name = "Nazwa")]
        public string Name { get; set; } = string.Empty; // Domyślna wartość

        /// <summary>
        /// Identyfikator użytkownika, do którego należy kategoria.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty; // Domyślna wartość

        /// <summary>
        /// Kolekcja przychodów przypisanych do tej kategorii.
        /// </summary>
        public ICollection<Income> Incomes { get; set; } = new List<Income>();

        /// <summary>
        /// Kolekcja wydatków przypisanych do tej kategorii.
        /// </summary>
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
