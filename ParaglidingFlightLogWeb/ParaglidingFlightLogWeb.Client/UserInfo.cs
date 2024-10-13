namespace ParaglidingFlightLogWeb.Client;

// Add properties to this class and update the server and client AuthenticationStateProviders
// to expose more information about the authenticated user to the client.
/// <summary>
/// User Info
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User Id
    /// </summary>
    public required string UserId { get; set; }
    /// <summary>
    /// Email
    /// </summary>
    public required string Email { get; set; }
}
