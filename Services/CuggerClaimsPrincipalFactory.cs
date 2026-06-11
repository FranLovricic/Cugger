using System.Security.Claims;
using Cugger.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Cugger.Services
{
    /// <summary>
    /// Lab-5: proširuje standardni Identity claims principal s dodatnim claimovima
    /// (ime, prezime, avatar) koje _Layout i ostali dijelovi aplikacije očekuju.
    /// </summary>
    public class CuggerClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole<int>>
    {
        public CuggerClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty));
            identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty));
            identity.AddClaim(new Claim("AvatarUrl", user.AvatarUrl ?? string.Empty));
            return identity;
        }
    }
}
