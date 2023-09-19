using LandSim.Areas.Map;
using LandSim.Areas.Map.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<MapContext>(ServiceLifetime.Transient);
builder.Services.AddTransient<MapRepository>();
builder.Services.AddSingleton<TerrainService>();
builder.Services.AddHostedService<BackgroundSimulationService>();
builder.Services.AddSingleton<SimulationEventAggregator>();
builder.Services.AddSingleton<DatabaseConnection>();
builder.Services.AddSingleton<SimulationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntDesign();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyBlazor v1"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
