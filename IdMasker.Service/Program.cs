using IdMasker;
using IdMasker.Service;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<Masker, ConfigurableMasker>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/mask/{idString}", (string idString, Masker masker, [FromQuery]int minLength = 0) =>
{
    var ids = idString.Split(',').Select(ulong.Parse);
    return masker.Mask(ids, minLength);
});

app.MapGet("/unmask/{mask}", (string mask, Masker masker) =>
{
    return string.Join(",", masker.Unmask(mask));
}); 

app.Run();