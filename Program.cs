using ChatInterno.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// SignalR
builder.Services.AddSignalR();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();



var app = builder.Build();

app.UseDefaultFiles(); // permite index.html
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chat");

app.MapPost("/auth/register", async (
    [FromServices] UserManager<IdentityUser> userManager,
    [FromBody] RegisterDto dto) =>
{
    var user = new IdentityUser
    {
        UserName = dto.Email,
        Email = dto.Email
    };

    var result = await userManager.CreateAsync(user, dto.Password);

    if (!result.Succeeded)
        return Results.BadRequest(result.Errors);

    return Results.Ok();
});


app.MapPost("/auth/login", async (
    [FromServices] SignInManager<IdentityUser> signInManager,
    [FromBody] LoginDto dto) =>
{
    var result = await signInManager.PasswordSignInAsync(
        dto.Email,
        dto.Password,
        isPersistent: true,
        lockoutOnFailure: false
    );

    if (!result.Succeeded)
        return Results.Unauthorized();

    return Results.Ok();
});


app.MapGet("/auth/me", (HttpContext http) =>
{
    if (!http.User.Identity?.IsAuthenticated ?? true)
        return Results.Unauthorized();

    return Results.Ok(new
    {
        user = http.User.Identity!.Name
    });
});




app.Run();
