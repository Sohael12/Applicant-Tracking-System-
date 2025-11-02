using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authentication;
using Stageproject_ATS_AP2025Q2.Models;
using Stageproject_ATS_AP2025Q2.Data;
using System.Text;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Stageproject_ATS_AP2025Q2.Services;

namespace Stageproject_ATS_AP2025Q2.Controllers
{
    /// <summary>
    /// Handles all user-related account operations: registration, login, logout, email confirmation, and password reset.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly GraphEmailService _graphEmailService;

                private readonly AppDbContext _context;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
             GraphEmailService graphEmailService,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
           _graphEmailService = graphEmailService;
              _context = context;
        }

        #region Registration

        /// <summary>
        /// Register a new user and send a confirmation email.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                Role = "User",
                IsActive = true,
                LockoutEnabled = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(errors);
            }

            await _context.UserRoles.AddAsync(new UserRole
            {
                UserId = user.Id,
                Role = "User"
            });
            await _context.SaveChangesAsync();

            var template = await _context.Templates.FirstOrDefaultAsync(t => t.Name == "RegisterConfirmation");
            if (template == null)
                return StatusCode(500, "Email template missing. Contact administrator.");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/account/confirmemail?userId={user.Id}&token={encodedToken}";

            var htmlMessage = template.HtmlContent
                .Replace("{confirmationLink}", confirmationLink)
                .Replace("{year}", DateTime.Now.Year.ToString());

await _graphEmailService.SendEmailAsync(user.Email, "Confirm your email", htmlMessage);

            return Ok("Registration successful. Please check your email to confirm.");
        }

        #endregion

        #region Login & Logout

        /// <summary>
        /// Login an existing user.
        /// </summary>
      [HttpPost("login")]
public async Task<IActionResult> Login([FromForm] LoginModel model)
{
    if (!ModelState.IsValid)
        return BadRequest(CreateErrorHtmlResponse("Invalid input."));

    var user = await _userManager.FindByEmailAsync(model.Email);
    if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        return Content(CreateErrorHtmlResponse("Invalid login credentials or email not confirmed."), "text/html");

    if (!user.IsActive)
        return Content(CreateErrorHtmlResponse("Your account is inactive. Please contact the administrator."), "text/html");

    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
    if (!result.Succeeded)
        return Content(CreateErrorHtmlResponse("Invalid login credentials."), "text/html");

    // Succesvolle login: cookie en redirect
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim("FirstName", user.FirstName ?? ""),
        new Claim("LastName", user.LastName ?? ""),
        new Claim("Address", user.Address ?? ""),
        new Claim(ClaimTypes.Email, user.Email ?? "")
    };
    var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
    var principal = new ClaimsPrincipal(identity);
    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

    return Content("<script>window.location.href='/vacancies'</script>", "text/html");
}

/// <summary>
/// Helper: Professioneel foutbericht voor login met zwarte/donkere achtergrond.
/// </summary>
private string CreateErrorHtmlResponse(string message)
{
    return $@"
    <!DOCTYPE html>
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Login Error</title>
        <style>
            body {{
                margin: 0;
                padding: 0;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                background-color: #121212;
                color: #f8f8f8;
                display: flex;
                justify-content: center;
                align-items: center;
                height: 100vh;
            }}
            .container {{
                text-align: center;
                background-color: #1e1e1e;
                padding: 40px 60px;
                border-radius: 10px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.5);
            }}
            h1 {{
                font-size: 2rem;
                margin-bottom: 20px;
                color: #ff4c4c;
            }}
            p {{
                font-size: 1rem;
                margin-bottom: 30px;
            }}
            a {{
                display: inline-block;
                padding: 12px 24px;
                background-color: #ff4c4c;
                color: white;
                text-decoration: none;
                border-radius: 6px;
                transition: background-color 0.3s ease;
            }}
            a:hover {{
                background-color: #e63946;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h1> Login Failed</h1>
            <p>{message}</p>
            <a href='/login'>Try Again</a>
        </div>
    </body>
    </html>";
}


        /// <summary>
        /// Logout the current user.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete(".AspNetCore.Identity.Application");

            Console.WriteLine("✅ Logout endpoint hit");

            return Ok();
        }

        #endregion

        #region Email Confirmation

        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(CreateHtmlResponse("Invalid request.", false));

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(CreateHtmlResponse("User not found.", false));

            string decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
                return Content(CreateHtmlResponse("✅ Email successfully confirmed! You can now log in.", true), "text/html");

            return BadRequest(CreateHtmlResponse("⚠️ Email confirmation failed. The token may be invalid or expired.", false));
        }

        private string CreateHtmlResponse(string message, bool success)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Email Confirmation</title>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin:0; padding:0; }}
                    .container {{ max-width:600px; margin:40px auto; background-color:#fff; padding:30px 40px; border-radius:8px; box-shadow:0 2px 8px rgba(0,0,0,0.05); color:#333; }}
                    h1 {{ font-size:1.8rem; margin-bottom:20px; font-weight:600; }}
                    p {{ font-size:1rem; line-height:1.6; margin-bottom:25px; }}
                    a {{ display:inline-block; background-color:#555; color:#fff; text-decoration:none; padding:12px 24px; border-radius:5px; font-weight:500; transition: background-color 0.3s ease; }}
                    a:hover {{ background-color:#333; }}
                    .footer {{ font-size:0.85rem; color:#888; margin-top:30px; text-align:center; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h1>{message}</h1>
                    {(success ? "<p>Please click the button below to go to the login page.</p><a href='/login'>Go to Login</a>" : "")}
                    <div class='footer'>&copy; {DateTime.Now.Year} Your Company. All rights reserved.</div>
                </div>
            </body>
            </html>";
        }

        #endregion

        #region Password Reset

        [AllowAnonymous]
        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid email format.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                return Ok("If this email exists, a reset link has been sent.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var encodedEmail = Uri.EscapeDataString(user.Email);

            var resetLink = $"https://localhost:7282/resetpassword?email={encodedEmail}&token={encodedToken}";

            var emailBody = $@"
            <div style=""font-family:Segoe UI, sans-serif; padding:20px;"">
                <h2 style=""color:#2c3e50;"">Password Reset Request</h2>
                <p>Hello {user.FirstName} {user.LastName},</p>
                <p>We received a request to reset your password. If you did not make this request, you can safely ignore this email.</p>
                <p>To reset your password, please click the button below:</p>
                <p style=""margin:20px 0;"">
                    <a href=""{resetLink}"" style=""background-color:#007bff; color:white; padding:10px 20px; text-decoration:none; border-radius:6px;"">Reset Password</a>
                </p>
                <p>If the button doesn't work, copy and paste the following link into your browser:</p>
                <p style=""word-break:break-all; color:#555;"">{resetLink}</p>
                <br/><p>Best regards,<br/>Your Support Team</p>
            </div>";

await _graphEmailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);

            return Ok("If this email exists, a reset link has been sent.");
        }

        [AllowAnonymous]
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid request.");

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                return BadRequest("Invalid token format.");
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok("Password has been reset successfully. You can now log in with your new password.");
        }

        #endregion

        #region Models

        public class RegisterModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [MinLength(6)]
            public string Password { get; set; }

            [Compare("Password")]
            public string ConfirmPassword { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
        }

        public class LoginModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        public class ForgotPasswordModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public class ResetPasswordModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Token { get; set; }

            [Required]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
            public string NewPassword { get; set; }

            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Je moet akkoord gaan met de Privacy Policy.")]
            public bool AgreeToPolicy { get; set; }
        }

        #endregion
    }
}
