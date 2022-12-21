using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MovieAppBlazor.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder()
{
    Host = "localhost",
    Port = 5432,
    Database = "bigDataAppDB",
    Username = "test",
    Password = "12345"
}.ToString());

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(connection), ServiceLifetime.Transient);
builder.Services.AddScoped<DataLoader>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();