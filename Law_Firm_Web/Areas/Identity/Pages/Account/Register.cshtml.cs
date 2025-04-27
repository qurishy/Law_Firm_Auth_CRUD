// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using DATA.Repositories.Client_repo;
using DATA.Repositories.Lawyer_repo;
using Law_Model.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using static Law_Model.Static_file.Static_datas;

namespace Law_Firm_Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IClient_Service _client_Service;
        private readonly ILawyer_Service _lawter_Service;

        public RegisterModel(
            IClient_Service client_Service,
            ILawyer_Service lawter_Service,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<ApplicationUser>)GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _client_Service = client_Service;
            _lawter_Service = lawter_Service;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(50)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }


            [StringLength(50)]
            [Display(Name = "Address")]
            public string Addresses { get; set; }



            [StringLength(50)]
            [Display(Name = "Phone Number")]
            public string phonwNumber { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Date Of Birth")]
            public DateTime DateOfBirth { get; set; }



            [Display(Name = "Profile Picture")]
            public IFormFile ProfilePicture { get; set; }


            [StringLength(50)]
            [Display(Name = "Position")]
            public string Position
            {
                get; set;
            }

            [StringLength(50)]
            [Display(Name = "Department")]
            public string Department
            {
                get; set;
            }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync((ApplicationUser)user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync((ApplicationUser)user, Input.Email, CancellationToken.None);

                //checking if we are creating a lawyer or client
                if (_signInManager.IsSignedIn(User))
                {
                    user.FirstName = Input.FirstName;
                    user.LastName = Input.LastName;
                    user.Role = UserRole.Lawyer;
                    user.Created = DateTime.UtcNow;

                }
                else
                {
                    // Set additional user properties
                    user.FirstName = Input.FirstName;
                    user.LastName = Input.LastName;
                    user.Role = UserRole.Client;
                    user.Created = DateTime.UtcNow;

                }



                // Handle profile picture if uploaded
                if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
                {
                    // Create a unique file name
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Input.ProfilePicture.FileName;

                    // Define the path where to save the file
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "profile-pictures");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        await Input.ProfilePicture.CopyToAsync(fileStream);
                    }


                    // Save the relative path to the user
                    user.ProfilePicture = "/profile-pictures/" + uniqueFileName;
                }

                var result = await _userManager.CreateAsync((ApplicationUser)user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        await _lawter_Service.CreatePersonnelAsync(user, Input.Position, Input.Department);


                        return RedirectToAction("Index", "AdminHome", new { area = "Admin_Area" });

                    }
                    else
                    {
                        await _client_Service.CreateClientAsync(user, Input.Addresses, Input.phonwNumber, Input.DateOfBirth);

                    }



                    var userId = await _userManager.GetUserIdAsync((ApplicationUser)user);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync((ApplicationUser)user);
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
                        await _signInManager.SignInAsync((ApplicationUser)user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}