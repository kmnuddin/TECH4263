using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudentAPI.Data;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace StudentAPI.Auth
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AppDbContext _context;

        public BasicAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, AppDbContext context) : base(options, logger, encoder)
        {
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }
            try 
            {
                var authHeader = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = System.Text.Encoding.UTF8.GetString(credentialBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];
                
                var hashedPassword = Helpers.PasswordHasher.Hash(password);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword);
                if (user == null)
                {
                    return AuthenticateResult.Fail("Invalid Username or Password");
                }

                var claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header Format");

            }
        }
    }
}
