using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Xml;

namespace ParaglidingFlightLogWeb.Components.Pages;

public partial class Settings
{
    private List<string>? _adminsId = new();
    private bool _isAdmin;
    private double _maxImportFlightCount;
    [Inject] public IConfiguration Config { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;
    protected override async Task OnInitializedAsync()
    {
        _adminsId = Config.GetSection("Administration:Admins").Get<List<string>>(); ;
        var userClaim = (await AuthenticationStateTask).User;
        if (userClaim?.Identity?.Name is null)
        {
            _isAdmin = false;
        }
        else{
            _isAdmin = _adminsId?.Contains(userClaim.Identity.Name) ?? false;
        }
    }

    private void MaxImportFlightCountChanged(double newValue){
        _maxImportFlightCount = newValue;
        WriteAdminSettings("MaxImportFlightCount", _maxImportFlightCount);
    }

    private void WriteAdminSettings(string v, double maxImportFlightCount)
    {
        
    }
}
