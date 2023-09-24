using Agent.Boar;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BoarService>();
builder.Services.AddHttpClient<SimulationClient>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
