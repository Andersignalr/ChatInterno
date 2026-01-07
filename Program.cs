using ChatInterno.Hubs;

var builder = WebApplication.CreateBuilder(args);

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

app.UseDefaultFiles(); // permite index.html
app.UseStaticFiles();

app.MapHub<ChatHub>("/chat");

app.Run();
