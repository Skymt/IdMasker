using IdMasker;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Masker>(services =>
{
    var config = services.GetRequiredService<IConfiguration>();
    return new(config["IdMasker:Alphabet"]!, config["IdMasker:Salt"]!);
});

var app = builder.Build();

app.MapGet("/mask/{idString}", (string idString, Masker masker, [FromQuery] int minLength = 0) =>
{
    var ids = idString.Split(',').Select(ulong.Parse);
    return masker.Mask(ids, minLength);
});

app.MapGet("/unmask/{mask}", (string mask, Masker masker) =>
{
    return string.Join(",", masker.Unmask(mask));
});

app.Run();