using Microsoft.JSInterop;

namespace ParaglidingFlightLogWeb.Services
{
    public interface ICaptchaManager
    {
        string? SiteKey { get; }

        /// <summary>
        /// Check the captcha response and return true if the captcha is solved correctly, otherwise false.
        /// </summary>
        /// <returns></returns>
        Task<bool> CheckCaptcha(string challengeResponse);
    }
}