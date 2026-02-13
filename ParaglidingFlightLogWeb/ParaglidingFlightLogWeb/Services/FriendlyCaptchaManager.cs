using System.Collections.Concurrent;
using ParaglidingFlightLogWeb.Data;

namespace ParaglidingFlightLogWeb.Services;

/// <inheritdoc/>
public class FriendlyCaptchaManager : ICaptchaManager
{
    private readonly string? _apiKey;
    private readonly string? _apiUrl;
    private readonly ILogger<FriendlyCaptchaManager> _logger;
    private readonly ConcurrentDictionary<string, (DateTime, int)> _validTokens = [];
    private readonly TimeSpan _tokenValidity = TimeSpan.FromMinutes(10);
    private readonly int _maxTokenUsage = 2; // login page are reload when the user submit the form so we have to allow multiple usage of the same token, but we want to limit it to prevent abuse.
    /// <inheritdoc/>
    public string? SiteKey { get; private init; }
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    public FriendlyCaptchaManager(IConfiguration configuration, ILogger<FriendlyCaptchaManager> logger)
    {
        _apiKey = configuration["FriendlyCaptcha:ApiKey"];
        SiteKey = configuration["FriendlyCaptcha:SiteKey"];
        _apiUrl = configuration["FriendlyCaptcha:ApiUrl"];
        _logger = logger;
    }

    private async Task<FriendlyCaptchaApiResponse?> VerifyChallengeAsync(string response)
    {
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(SiteKey))
        {
            throw new InvalidOperationException("API key or Site key is not configured.");
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-API-Key", _apiKey);

        var serviceAnswer = await client.PostAsJsonAsync(_apiUrl, new
        {
            sitekey = SiteKey,
            response
        });

        serviceAnswer.EnsureSuccessStatusCode();
        return await serviceAnswer.Content.ReadFromJsonAsync<FriendlyCaptchaApiResponse>();
    }
    /// <inheritdoc/>
    public async Task<string> CheckCaptcha(string challengeResponse)
    {
        string token = Guid.NewGuid().ToString(); // The only difference between valid token and unvalid token is the fact that they are added to the dictionary. This way we don't expose the fact that the token is valid by its format.
        try
        {
            var result = await VerifyChallengeAsync(challengeResponse);

            if (result?.Success == true)
            {
                _logger.LogInformation($"Captcha verified successfully!");
                if (!_validTokens.TryAdd(token, (DateTime.UtcNow + _tokenValidity, _maxTokenUsage)))
                {
                    _logger.LogError("The token {Token} already exists in the valid tokens dictionary", token);
                }
            }
            else
            {
                _logger.LogError("Captcha verification failed: {Code}, {Detail}", result?.Error?.ErrorCode, result?.Error?.Detail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying captcha");
        }
        return token;
    }
    /// <inheritdoc/>
    public bool IsCaptchaValid(string token)
    {
        bool output = false;
        CleanupExpiredTokens();
        if (_validTokens.TryGetValue(token, out var value))
        {
            output = true;
            if (value.Item2 > 1)
            {
                _validTokens[token] = (value.Item1, value.Item2 - 1);
            }
            else
            {
                _validTokens.TryRemove(token, out _);
            }
        }
        return output;
    }
    private void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        foreach (var token in _validTokens.Where(kvp => kvp.Value.Item1 < now || kvp.Value.Item2 <= 0).Select(kvp => kvp.Key))
        {
            _validTokens.TryRemove(token, out _);
        }
    }
}

