using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaFinansami.Models;

namespace SystemZarzadzaniaFinansami.Data
{
    /// <summary>
    /// Klasa reprezentująca kontekst bazy danych dla aplikacji.
    /// Dziedziczy po <see cref="IdentityDbContext"/>, aby obsługiwać użytkowników i role.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        /// <summary>
        /// Inicjalizuje nową instancję kontekstu bazy danych <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">Opcje konfiguracji bazy danych.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Zbiór przychodów użytkownika.
        /// </summary>
        public DbSet<Income> Incomes { get; set; }

        /// <summary>
        /// Zbiór wydatków użytkownika.
        /// </summary>
        public DbSet<Expense> Expenses { get; set; }

        /// <summary>
        /// Zbiór kategorii finansowych użytkownika.
        /// </summary>
        public DbSet<Category> Categories { get; set; }
    }
}
