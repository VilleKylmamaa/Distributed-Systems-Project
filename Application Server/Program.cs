using DistrChat.SignalR;
using StackExchange.Redis;


// Build configuration

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);

builder.Services.AddRazorPages();

builder.Services.AddSignalR()
    .AddStackExchangeRedis(
        options =>
        {
            options.Configuration.ChannelPrefix = "DistrChat";
            options.ConnectionFactory = async writer =>
            {
                var config = new ConfigurationOptions
                {
                    AbortOnConnectFail = false
                };

                config.EndPoints.Add("177.17.0.31:6371"); // Cluster node 1
                config.EndPoints.Add("177.17.0.32:6372"); // Cluster node 2
                config.EndPoints.Add("177.17.0.33:6373"); // Cluster node 3
                config.EndPoints.Add("177.17.0.34:6374"); // Cluster node 4
                config.EndPoints.Add("177.17.0.36:6375"); // Cluster node 5
                config.EndPoints.Add("177.17.0.36:6376"); // Cluster node 6

                var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
                connection.ConnectionFailed += (_, e) =>
                {
                    Console.WriteLine("Connection to Redis failed.");
                };

                if (!connection.IsConnected)
                {
                    Console.WriteLine("Did not connect to Redis.");
                }

                return connection;
            };
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
    })
);

var app = builder.Build();


// After build configuration

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Connections}/{action=Connections}"
);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<SignalrHub>("/SignalrHub");

app.Run();
