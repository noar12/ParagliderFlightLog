using Microsoft.JSInterop;

namespace ParaglidingFlightLogWeb.Services
{
    public interface ICaptchaManager
    {
        string? SiteKey { get; }
        bool? IsHuman { get; }

        /// <summary>
        /// Check the captcha response and return true if the captcha is solved correctly, otherwise false.
        /// </summary>
        /// <returns></returns>
        Task CheckCaptcha(string challengeResponse);
    }
}