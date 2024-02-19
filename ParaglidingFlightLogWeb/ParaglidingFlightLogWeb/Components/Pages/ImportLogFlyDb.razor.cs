using global::Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ParaglidingFlightLogWeb.ViewModels;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using System;

namespace ParaglidingFlightLogWeb.Components.Pages
{
    public partial class ImportLogFlyDb
    {
        [Inject]
        private MainViewModel? Mvm { get; set; }
        [Inject]
        private ILogger<ImportLogFlyDb>? Logger { get; set; }
        [Inject]
        private IWebHostEnvironment? Environment { get; set; }
        private bool _success = false;
        private int _importedFlightCount;
        private int _importedSiteCount;
        private int _importedGliderCount;
        private bool _fail = false;
        private string _failMessage = string.Empty;
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