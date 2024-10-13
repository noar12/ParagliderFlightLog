using Microsoft.AspNetCore.Identity;

namespace ParaglidingFlightLogWeb.Data
{
    /// <summary>
    /// Add profile data for application users by adding properties to the ApplicationUser class
    /// </summary>
#pragma warning disable S2094 // Classes should not be empty
    public class ApplicationUser : IdentityUser
#pragma warning restore S2094 // Classes should not be empty
    {
    }

}
