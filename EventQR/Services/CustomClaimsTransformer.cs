using EventQR.EF;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace EventQR.Services
{
    public class CustomClaimsTransformer : IClaimsTransformation
    {
        private AppDbContext _dbcontext;
        public CustomClaimsTransformer(AppDbContext dbContext)
        {
            _dbcontext = dbContext;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {

            var identity = (ClaimsIdentity)principal.Identity;
            if (identity.IsAuthenticated)
            {
                _ = Guid.TryParse(identity.Claims.FirstOrDefault().Value, out Guid OrganizerUserId);
                var _organizer = await _dbcontext.EventOrganizers.Where(o => o.OrganizerUserId.Equals(OrganizerUserId)).FirstOrDefaultAsync();
                identity.AddClaim(new Claim("organizer", JsonConvert.SerializeObject(_organizer)));
            }
            return principal;
        }
    }
}
