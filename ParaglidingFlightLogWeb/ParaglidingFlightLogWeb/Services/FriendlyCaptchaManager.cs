namespace ParaglidingFlightLogWeb.Services;

public class FriendlyCaptchaManager : ICaptchaManager
{
    private readonly string? _apiKey;
    
    private readonly string? _apiUrl;
    private readonly ILogger<FriendlyCaptchaManager> _logger;
    public string? SiteKey { get; private init; }

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
            response = response
        });

        serviceAnswer.EnsureSuccessStatusCode();
        return await serviceAnswer.Content.ReadFromJsonAsync<FriendlyCaptchaApiResponse>();
    }

    public async Task<bool> CheckCaptcha(string challengeResponse)
    {
        bool isSuccess = false;
        try
        {
            var result = await VerifyChallengeAsync(challengeResponse);

            if (result?.Success == true)
            {
                isSuccess = true;
                _logger.LogInformation($"Captcha verified successfully!");
            }
            else
            {
                _logger.LogError("Captcha verification failed: {Code}, {Detail}", result?.Error?.Error_Code, result?.Error?.Detail);
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying captcha");
        }
        return isSuccess;
    }
}

public class FriendlyCaptchaApiResponse
{
    public bool Success { get; set; }
    public FriendlyCaptchaData? Data { get; set; }
    public FriendlyCaptchaError? Error { get; set; }
}

public class FriendlyCaptchaData
{
    public string? Event_Id { get; set; }
    public CaptchaChallenge? Challenge { get; set; }
}

public class CaptchaChallenge
{
    public DateTime Timestamp { get; set; }
    public string? Origin { get; set; }
}

public class FriendlyCaptchaError
{
    public string? Error_Code { get; set; }
    public string? Detail { get; set; }
}
