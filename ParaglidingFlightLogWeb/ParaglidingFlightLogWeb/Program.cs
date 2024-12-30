using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Configuration;
using ParagliderFlightLog.DataAccess;
using ParagliderFlightLog.Services;
using ParaglidingFlightLogWeb.Client.Pages;
using ParaglidingFlightLogWeb.Components;
using ParaglidingFlightLogWeb.Components.Account;
using ParaglidingFlightLogWeb.Data;
using ParaglidingFlightLogWeb.Services;
using Radzen;
using Serilog;
using System.Globalization;


try
{
    
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog();
    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityUserAccessor>();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();


    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
        .AddIdentityCookies();

    var connectionString = builder.Configuration.GetConnectionString("UserDataSqlite") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

    builder.Services.AddRadzenComponents();
    builder.Services.AddHttpClient(); 
    
    // Register our own injectables
    builder.Services.AddScoped<CoreService>();
    builder.Services.AddScoped<FlightStatisticService>();
    builder.Services.AddScoped<FlightLogDB>();
    builder.Services.AddScoped<LogFlyDB>();
    builder.Services.AddScoped<SqliteDataAccess>();
    builder.Services.AddSingleton<XcScoreManagerData>();
    builder.Services.AddHostedService<XcScoreManager>();
    builder.Services.AddTransient<PhotosService>();
    var app = builder.Build();
    // Set the culture for something globally accepted... TODO Customize for each client
    var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
    customCulture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
    customCulture.DateTimeFormat.LongDatePattern = "dd MMMM yyyy";
    CultureInfo.DefaultThreadCurrentCulture = customCulture;
    CultureInfo.DefaultThreadCurrentUICulture = customCulture;
    
    var config = app.Services.GetService<IConfiguration>()!;
    Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();
    Log.Information("Application starting up");

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(ParaglidingFlightLogWeb.Client._Imports).Assembly);

    // Add additional endpoints required by the Identity /Account Razor components.
    app.MapAdditionalIdentityEndpoints();

    //app.UseSerilogRequestLogging(); // log all the request
    await app.RunAsync();
    Log.Information("Application stopped");
}
catch (Exception ex)
{
    Log.Fatal(ex, "The application failed to start");
}
finally
{
    await Log.CloseAndFlushAsync();
}

