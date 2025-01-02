using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemZarzadzaniaFinansami.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kwota jest wymagana")]
        [Display(Name = "Kwota")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, 10000000, ErrorMessage = "Minimalna wartość 1, maksymalna wartość 10000000")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Data")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Kategoria")]
        public int CategoryId { get; set; }

        [Display(Name = "Kategoria")]
        public Category? Category { get; set; } // Nullable, jeśli brak kategorii jest dopuszczalny

        [Required]
        public string UserId { get; set; } = string.Empty; // Domyślna wartość dla uniknięcia null
    }
}
