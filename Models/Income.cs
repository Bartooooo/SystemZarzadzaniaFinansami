using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SystemZarzadzaniaFinansami.Models
{
    /// <summary>
    /// Reprezentuje pojedynczy przychód użytkownika.
    /// </summary>
    public class Income
    {
        /// <summary>
        /// Unikalny identyfikator przychodu.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Kwota przychodu.
        /// </summary>
        [Required(ErrorMessage = "Kwota jest wymagana")]
        [Display(Name = "Kwota", Description = "Proszę podać kwotę przychodu")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000000, ErrorMessage = "Minimalna wartość '0,01', maksymalna wartość '10000000'")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Data przychodu.
        /// </summary>
        [Required(ErrorMessage = "Data jest wymagana")]
        [Display(Name = "Data", Description = "Proszę podać datę przychodu")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Identyfikator kategorii, do której przypisano przychód.
        /// </summary>
        [Required(ErrorMessage = "Kategoria jest wymagana")]
        [Display(Name = "Kategoria", Description = "Wybierz odpowiednią kategorię.")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Kategoria finansowa przypisana do przychodu.
        /// </summary>
        [Display(Name = "Kategoria", Description = "Wybierz odpowiednią kategorię.")]
        public Category? Category { get; set; } // Nullable, jeśli brak kategorii jest dopuszczalny

        /// <summary>
        /// Identyfikator użytkownika, do którego należy przychód.
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty; // Domyślna wartość dla uniknięcia null
    }
}
