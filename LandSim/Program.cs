using LandSim.Areas.Agents;
using LandSim.Areas.Generation.Services;
using LandSim.Areas.Map.Services;
using LandSim.Areas.Simulation;
using LandSim.Areas.Simulation.Services;
using LandSim.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MapContext>(ServiceLifetime.Transient);
builder.Services.AddTransient<MapRepository>();
builder.Services.AddHttpClient<AgentOwnerClient>();
builder.Services.AddSingleton<AgentUpdateService>();
builder.Services.AddSingleton<TerrainService>();
builder.Services.AddSingleton<TerrainGenerationService>();
builder.Services.AddHostedService<BackgroundSimulationService>();
builder.Services.AddSingleton<SimulationService>();
builder.Services.AddSingleton<SimulationEventAggregator>();
builder.Services.AddSingleton<DatabaseConnection>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
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
