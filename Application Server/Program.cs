using DistrChat.SignalR;


// Build configuration

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);

builder.Services.AddRazorPages();

// Redis backplane address when running outside Docker: "127.0.0.1:6379",
// Redis backplane address when running in Docker: "177.17.0.255:6379",
builder.Services.AddSignalR()
    .AddStackExchangeRedis(
        "177.17.0.255:6379",
        options =>
        {
            options.Configuration.ChannelPrefix = "DistrChat";
        }
    );

builder.Services.AddSingleton<SignalrHub>();

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
    builder =>
    {
        // Allow CORS
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed((host) => true)
               .AllowCredentials();
    }));

var app = builder.Build();


// After build configuration

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<SignalrHub>("/SignalrHub");

app.Run();
