using System.Text.Json.Serialization;
// ReSharper disable All

namespace ParaglidingFlightLogWeb.Data;

/// <summary>
/// Data Model for the response of the friendly captcha API.
/// </summary>
public class FriendlyCaptchaApiResponse
{
    /// <summary>
    /// True if the CAPTCHA challenge was successfully completed and the response is valid; otherwise, false. This field indicates the overall success of the CAPTCHA verification process.
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// Data in case of a successful CAPTCHA verification.
    /// </summary>
    public FriendlyCaptchaData? Data { get; set; }
    /// <summary>
    /// Data in case of an error during the CAPTCHA verification process.
    /// </summary>
    public FriendlyCaptchaError? Error { get; set; }
}
/// <summary>
/// Data Model for the data part of the response of the friendly captcha API.
/// </summary>
public class FriendlyCaptchaData
{
    /// <summary>
    /// Id
    /// </summary>
    public string? Event_Id { get; set; }
    /// <summary>
    /// Challenge data
    /// </summary>
    public CaptchaChallenge? Challenge { get; set; }
}
/// <summary>
/// Data model for the challenge part of the response of the friendly captcha API.
/// </summary>
public class CaptchaChallenge
{
    /// <summary>
    /// Timestamp of the challenge
    /// </summary>
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// Origin of the challenge.
    /// </summary>
    public string? Origin { get; set; }
}
/// <summary>
/// Data model for the error part of the response of the friendly captcha API.
/// </summary>
public class FriendlyCaptchaError
{
    /// <summary>
    /// Error code indicating the reason for the failure of the CAPTCHA verification process.
    /// </summary>
    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }
    /// <summary>
    /// Error detail providing additional information about the error that occurred during the CAPTCHA verification process.
    /// </summary>
    public string? Detail { get; set; }
}