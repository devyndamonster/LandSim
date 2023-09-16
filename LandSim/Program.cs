using LandSim.Areas.Map;
using LandSim.Areas.Map.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<MapContext>(ServiceLifetime.Transient);
builder.Services.AddTransient<MapGenerationRepository>();
builder.Services.AddSingleton<TerrainService>();
builder.Services.AddHostedService<BackgroundSimulationService>();
builder.Services.AddSingleton<SimulationEventAggregator>();
builder.Services.AddSingleton<DatabaseConnection>();
builder.Services.AddSingleton<SimulationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
