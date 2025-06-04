using BlockedCountries.BL.Managers;
using BlockedCountries.BL.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Default
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod();
    });
});

#endregion

#region Service

builder.Services.AddSingleton<BlockedCountryService>();
builder.Services.AddSingleton<GeoLocationService>();
builder.Services.AddSingleton<LogService>();
builder.Services.AddHttpClient<GeoLocationService>();

#endregion

//builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
