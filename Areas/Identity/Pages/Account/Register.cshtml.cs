using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace SystemZarzadzaniaFinansami.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Model strony rejestracji użytkownika. Obsługuje proces tworzenia nowego konta.
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Konstruktor modelu rejestracji, który inicjalizuje menedżera użytkowników i inne usługi.
        /// </summary>
        /// <param name="userManager">Menedżer użytkowników do zarządzania procesem rejestracji.</param>
        /// <param name="userStore">Przechowuje dane o użytkownikach w bazie danych.</param>
        /// <param name="signInManager">Menedżer logowania użytkowników.</param>
        /// <param name="logger">Logger do rejestrowania działań związanych z rejestracją.</param>
        /// <param name="emailSender">Serwis wysyłania e-maili (np. do potwierdzenia e-maila).</param>
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Właściwość, która przechowuje dane wejściowe użytkownika podczas rejestracji (e-mail, hasło).
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Adres URL, na który użytkownik zostanie przekierowany po zakończeniu rejestracji.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Lista dostępnych zewnętrznych metod logowania (np. Google, Facebook).
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// Model danych wejściowych do formularza rejestracji.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Adres e-mail użytkownika.
            /// </summary>
            [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
            [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu e-mail.")]
            [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Wprowadź poprawny adres e-mail (np. test@test.pl).")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            /// Hasło użytkownika.
            /// </summary>
            [Required(ErrorMessage = "Hasło jest wymagane.")]
            [StringLength(100, ErrorMessage = "Hasło musi mieć co najmniej {2} i maksymalnie {1} znaków.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }

            /// <summary>
            /// Potwierdzenie hasła w procesie rejestracji.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Potwierdź hasło")]
            [Compare("Password", ErrorMessage = "Hasło i potwierdzenie hasła muszą się zgadzać.")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        /// Inicjuje dane do formularza rejestracji, w tym dostępne zewnętrzne metody logowania.
        /// </summary>
        /// <param name="returnUrl">Adres URL, na który użytkownik powinien zostać przekierowany po udanym logowaniu.</param>
        /// <returns>Wynik operacji asynchronicznej.</returns>
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        /// <summary>
        /// Obsługuje proces rejestracji użytkownika na stronie. Po pomyślnej rejestracji użytkownik otrzymuje e-mail z linkiem potwierdzającym.
        /// </summary>
        /// <param name="returnUrl">Adres URL, na który użytkownik zostanie przekierowany po udanym logowaniu.</param>
        /// <returns>Wynik operacji asynchronicznej.</returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        /// <summary>
        /// Tworzy nowego użytkownika na podstawie danych wejściowych.
        /// </summary>
        /// <returns>Nowo utworzony obiekt użytkownika.</returns>
        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        /// <summary>
        /// Zwraca instancję użytkownika, który obsługuje operacje e-mailowe.
        /// </summary>
        /// <returns>Instancja użytkownika obsługująca operacje e-mailowe.</returns>
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
