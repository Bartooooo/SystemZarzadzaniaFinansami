// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SystemZarzadzaniaFinansami.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Model logowania, obsługujący logowanie użytkownika przy użyciu adresu e-mail i hasła.
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Konstruktor modelu logowania, który przyjmuje menedżera logowania i logger.
        /// </summary>
        /// <param name="signInManager">Menedżer logowania użytkownika.</param>
        /// <param name="logger">Logger do rejestrowania informacji o logowaniu.</param>
        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Właściwość, która przechowuje dane wejściowe użytkownika (e-mail, hasło, zapamiętanie sesji).
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Lista dostępnych zewnętrznych metod logowania (np. Google, Facebook).
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// Adres URL, do którego użytkownik powinien zostać przekierowany po zalogowaniu.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Wiadomość o błędzie do wyświetlenia, jeśli wystąpi problem z logowaniem.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Model danych wejściowych dla formularza logowania.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Adres e-mail użytkownika.
            /// </summary>
            [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
            [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu e-mail.")]
            public string Email { get; set; }

            /// <summary>
            /// Hasło użytkownika.
            /// </summary>
            [Required(ErrorMessage = "Hasło jest wymagane.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// Określa, czy użytkownik chce, aby sesja była zapamiętana.
            /// </summary>
            [Display(Name = "Zapamiętaj mnie?")]
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Obsługuje żądanie GET dla strony logowania, wczytując dane zewnętrznych metod logowania oraz sprawdzając, czy jest komunikat o błędzie.
        /// </summary>
        /// <param name="returnUrl">Adres URL, na który użytkownik zostanie przekierowany po udanym logowaniu.</param>
        /// <returns>Wynik operacji asynchronicznej.</returns>
        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage); // Dodanie błędu do stanu modelu
            }

            returnUrl ??= Url.Content("~/"); // Jeśli returnUrl jest null, ustawiamy domyślny URL

            // Wyczyść istniejące ciasteczka zewnętrznego logowania
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(); // Pobierz zewnętrzne metody logowania
            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Obsługuje żądanie POST dla logowania, walidując dane logowania użytkownika.
        /// </summary>
        /// <param name="returnUrl">Adres URL, na który użytkownik zostanie przekierowany po udanym logowaniu.</param>
        /// <returns>Wynik operacji asynchronicznej.</returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Użytkownik zalogowany pomyślnie.");
                    return LocalRedirect(returnUrl); // Przekierowanie po udanym logowaniu
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Konto użytkownika zostało zablokowane.");
                    return RedirectToPage("./Lockout"); // Przekierowanie na stronę blokady konta
                }
                else
                {
                    ErrorMessage = "Nieudane logowanie. Sprawdź wprowadzone dane.";
                    _logger.LogWarning("Nieudana próba logowania dla użytkownika {Email}.", Input.Email); // Rejestracja nieudanej próby logowania
                }
            }

            // Redisplay form if login failed
            return Page(); // Wyświetlenie formularza logowania w przypadku błędu
        }
    }
}
