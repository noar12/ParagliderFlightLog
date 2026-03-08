using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ParaglidingFlightLogWeb.Components.Account.Pages;
using ParaglidingFlightLogWeb.Components.Account.Pages.Manage;
using ParaglidingFlightLogWeb.Data;
using System.Security.Claims;
using System.Text.Json;
using ParagliderFlightLog.DataAccess;
using System.IO.Compression;

namespace ParaglidingFlightLogWeb.Components.Account
{
    internal static class IdentityComponentsEndpointRouteBuilderExtensions
    {
        // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/Account");

            accountGroup.MapPost("/PerformExternalLogin", (
                HttpContext context,
                [FromServices] SignInManager<ApplicationUser> signInManager,
                [FromForm] string provider,
                [FromForm] string returnUrl) =>
            {
                IEnumerable<KeyValuePair<string, StringValues>> query =
                [
                    new("ReturnUrl", returnUrl),
                    new("Action", ExternalLogin.LoginCallbackAction)
                ];

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/ExternalLogin",
                    QueryString.Create(query));

                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return TypedResults.Challenge(properties, [provider]);
            });

            accountGroup.MapPost("/Logout", async (
                ClaimsPrincipal user,
                SignInManager<ApplicationUser> signInManager,
                [FromForm] string returnUrl) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            var manageGroup = accountGroup.MapGroup("/Manage").RequireAuthorization();

            manageGroup.MapPost("/LinkExternalLogin", async (
                HttpContext context,
                [FromServices] SignInManager<ApplicationUser> signInManager,
                [FromForm] string provider) =>
            {
                // Clear the existing external cookie to ensure a clean login process
                await context.SignOutAsync(IdentityConstants.ExternalScheme);

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/Manage/ExternalLogins",
                    QueryString.Create("Action", ExternalLogins.LinkLoginCallbackAction));

                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl,
                    signInManager.UserManager.GetUserId(context.User));
                return TypedResults.Challenge(properties, [provider]);
            });

            var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");

            manageGroup.MapPost("/DownloadPersonalData", async (
                    HttpContext context,
                    [FromServices] UserManager<ApplicationUser> userManager,
                    [FromServices] AuthenticationStateProvider authenticationStateProvider,
                    [FromServices] FlightLogDB flightLogDb)
                =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user is null)
                {
                    return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
                }

                var userId = await userManager.GetUserIdAsync(user);
                downloadLogger.LogInformation("User with ID '{UserId}' asked for their personal data.", userId);

                // Only include personal data for download
                var personalData = new Dictionary<string, string>();
                var personalDataProps = typeof(ApplicationUser).GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
                }

                var logins = await userManager.GetLoginsAsync(user);
                foreach (var l in logins)
                {
                    personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
                }

                personalData.Add("Authenticator Key", (await userManager.GetAuthenticatorKeyAsync(user))!);
                var fileText = JsonSerializer.Serialize(personalData);

                flightLogDb.Init(userId);
                string backupDbPath = await flightLogDb.BackupDbAsync();
                string? backupPath = Path.GetDirectoryName(backupDbPath);
                if (backupPath is null)
                {
                    return TypedResults.Problem($"Cannot find data");
                }

                string personalDataPath = Path.Combine(backupPath, "PersonalData.json");
                await File.WriteAllTextAsync(personalDataPath, fileText);
                string userDataZipPath = Path.Combine(backupPath, $"{userId}Data.zip");
                if (File.Exists(userDataZipPath)) { File.Delete(userDataZipPath);}

                using (ZipArchive zip = ZipFile.Open(userDataZipPath, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(backupDbPath, Path.GetFileName(backupDbPath));
                    zip.CreateEntryFromFile(personalDataPath, Path.GetFileName(personalDataPath));
                }
                
                context.Response.Headers.TryAdd("Content-Disposition", $"attachment; filename={Path.GetFileName(userDataZipPath)})");
                var fileBytes = await File.ReadAllBytesAsync(userDataZipPath);
                return TypedResults.File(fileBytes, "application/zip", Path.GetFileName(userDataZipPath));
            });

            return accountGroup;
        }
    }
}