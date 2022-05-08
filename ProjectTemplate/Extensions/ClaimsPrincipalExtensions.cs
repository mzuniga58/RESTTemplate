using System.Security.Claims;
using System.Text;

namespace $safeprojectname$.Extensions
{
    /// <summary>
    /// Claims Principal Extensions
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns the string representation of the ClaimsPrincipal
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> associated with this call.</param>
        /// <returns></returns>
        public static string ListClaims(this ClaimsPrincipal principal)
        {
            StringBuilder theClaimsList = new();
            bool first = true;
            theClaimsList.Append('[');

            foreach (var claim in principal.Claims)
            {
                if (first)
                    first = false;
                else
                    theClaimsList.Append(',');
                string theClaim = $"\"{claim.Type}\": \"{claim.Value}\"";
                theClaimsList.Append(theClaim);
            }

            theClaimsList.Append(']');

            return theClaimsList.ToString();
        }
    }
}
