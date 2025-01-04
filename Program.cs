using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SystemZarzadzaniaFinansami.Data;

namespace SystemZarzadzaniaFinansami
{
    /// <summary>
    /// G��wna klasa aplikacji, kt�ra konfiguruje i uruchamia aplikacj� ASP.NET Core.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Punkt wej�cia do aplikacji.
        /// Konfiguruje serwis, routing i middleware.
        /// </summary>
        /// <param name="args">Argumenty wiersza polece�.</param>
        public static void Main(string[] args)
        {
            // Tworzenie i konfigurowanie aplikacji
            var builder = WebApplication.CreateBuilder(args);

            // Pobranie ci�gu po��czenia do bazy danych z pliku konfiguracyjnego
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Rejestracja kontekstu bazy danych w DI kontenerze
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Dodanie obs�ugi b��d�w podczas rozwoju aplikacji
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Rejestracja domy�lnego systemu to�samo�ci (Identity)
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Dodanie wsparcia dla kontroler�w i widok�w
            builder.Services.AddControllersWithViews();

            // Tworzenie aplikacji
            var app = builder.Build();

            // Konfiguracja pipeline'a HTTP
            app.UseRequestLocalization();

            // Ustawienia dla �rodowiska deweloperskiego
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                // Obs�uga wyj�tk�w w produkcji
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Wymuszenie przekierowania na HTTPS
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Obs�uga routingu i autoryzacji
            app.UseRouting();
            app.UseAuthorization();

            // Globalny filtr sprawdzaj�cy, czy u�ytkownik jest zalogowany
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value?.ToLower();
                // Przekierowanie niezalogowanych u�ytkownik�w na stron� go�cia
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

            // Konfiguracja domy�lnego routingu dla kontroler�w
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            // Uruchomienie aplikacji
            app.Run();
        }
    }
}
