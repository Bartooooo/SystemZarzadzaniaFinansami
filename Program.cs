using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SystemZarzadzaniaFinansami.Data;

namespace SystemZarzadzaniaFinansami
{
    /// <summary>
    /// G³ówna klasa aplikacji, która konfiguruje i uruchamia aplikacjê ASP.NET Core.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Punkt wejœcia do aplikacji.
        /// Konfiguruje serwis, routing i middleware.
        /// </summary>
        /// <param name="args">Argumenty wiersza poleceñ.</param>
        public static void Main(string[] args)
        {
            // Tworzenie i konfigurowanie aplikacji
            var builder = WebApplication.CreateBuilder(args);

            // Pobranie ci¹gu po³¹czenia do bazy danych z pliku konfiguracyjnego
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Rejestracja kontekstu bazy danych w DI kontenerze
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Dodanie obs³ugi b³êdów podczas rozwoju aplikacji
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Rejestracja domyœlnego systemu to¿samoœci (Identity)
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Dodanie wsparcia dla kontrolerów i widoków
            builder.Services.AddControllersWithViews();

            // Tworzenie aplikacji
            var app = builder.Build();

            // Konfiguracja pipeline'a HTTP
            app.UseRequestLocalization();

            // Ustawienia dla œrodowiska deweloperskiego
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                // Obs³uga wyj¹tków w produkcji
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Wymuszenie przekierowania na HTTPS
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Obs³uga routingu i autoryzacji
            app.UseRouting();
            app.UseAuthorization();

            // Globalny filtr sprawdzaj¹cy, czy u¿ytkownik jest zalogowany
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower();
                // Przekierowanie niezalogowanych u¿ytkowników na stronê goœcia
                if (!context.User.Identity.IsAuthenticated &&
                    (path.StartsWith("/incomes") ||
                     path.StartsWith("/expenses") ||
                     path.StartsWith("/categories") ||
                     path.StartsWith("/reports")))
                {
                    context.Response.Redirect("/Home/Guest");
                    return;
                }
                await next();
            });

            // Konfiguracja domyœlnego routingu dla kontrolerów
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            // Uruchomienie aplikacji
            app.Run();
        }
    }
}
