using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemZarzadzaniaFinansami.Models
{
    public class Income
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kwota jest wymagana")]       
        [Display(Name = "Kwota",Description = "Proszę podać kwotę przychodu")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000000, ErrorMessage = "Minimalna wartość '0,01', maksymalna wartość '10000000'")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Data jest wymagana")]
        [Display(Name = "Data", Description = "Proszę podać datę przychodu")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Kategoria jest wymagana")]
        [Display(Name = "Kategoria", Description = "Wybierz odpowiednią kategorię.")]
        public int CategoryId { get; set; }

        [Display(Name = "Kategoria", Description = "Wybierz odpowiednią kategorię.")]
        public Category? Category { get; set; } // Nullable, jeśli brak kategorii jest dopuszczalny

        [Required]
        public string UserId { get; set; } = string.Empty; // Domyślna wartość dla uniknięcia null
    }
}
