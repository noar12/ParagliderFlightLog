using global::Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using ParaglidingFlightLogWeb.Data;

namespace ParaglidingFlightLogWeb.Components.Pages
{
    public partial class ImportLogFlyDb
    {
        [Inject] private MainViewModel Mvm { get; set; } = null!;
        [Inject] private ILogger<ImportLogFlyDb>? Logger { get; set; } = null!;
        [Inject] private IWebHostEnvironment? Environment { get; set; } = null!;
        [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
        [Inject] UserManager<ApplicationUser> UserManager { get; set; } = null!;
        private bool _success = false;
        private int _importedFlightCount;
        private int _importedSiteCount;
        private int _importedGliderCount;
        private bool _fail = false;
        private string _failMessage = string.Empty;
        protected override async Task OnInitializedAsync()
        {
            var userClaim = (await AuthenticationStateTask).User;
            if (userClaim.Identity is not null && userClaim.Identity.IsAuthenticated)
            {
                var currentUser = await UserManager.GetUserAsync(userClaim);
                if (currentUser == null) return;
                string userId = currentUser.Id;
                await Mvm.Init(userId);
            }
        }
        private async Task OnLogFlyDbChosen(InputFileChangeEventArgs e)
        {
            _success = false;
            _fail = false;
            if (e.File is not null && e.File.Name.EndsWith(".db"))
            {
                int maxFileSize = 50 * 1024 * 1024;
                var trustedFileNameForFileStorage = Path.GetRandomFileName();
                var path = Path.Combine(Environment!.ContentRootPath, "tmp", trustedFileNameForFileStorage);
                try
                {
                    await using FileStream fs = new(path, FileMode.Create);
                    await e.File.OpenReadStream(maxFileSize).CopyToAsync(fs);
                    Logger!.LogDebug("Copy {providedFile} to {tmpFile}", e.File.Name, path);
                    (_importedSiteCount, _importedGliderCount, _importedFlightCount) = await Mvm!.ImportLogFlyDb(path);
                    _success = true;
                }
                catch (Exception ex)
                {
                    _fail = true;
                    _failMessage = ex.Message;
                }

            }


        }
    }
}