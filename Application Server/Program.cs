using DistrChat.SignalR;
using DistrLB.RedisStatusPoller;
using StackExchange.Redis;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

// Build configuration

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);

builder.Services.AddRazorPages();
builder.Services.AddHostedService<RedisStatusPoller>();

var config = new ConfigurationOptions
{
    AbortOnConnectFail = false
};
config.EndPoints.Add("177.17.0.31:6371"); // Redis node 1
config.EndPoints.Add("177.17.0.32:6372"); // Redis node 2
config.EndPoints.Add("177.17.0.33:6373"); // Redis node 3
config.EndPoints.Add("177.17.0.34:6374"); // Redis node 4
config.EndPoints.Add("177.17.0.35:6375"); // Redis node 5
config.EndPoints.Add("177.17.0.36:6376"); // Redis node 6

var redisConnection = await ConnectionMultiplexer.ConnectAsync(config, null);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

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

                redisConnection.ConnectionFailed += (_, e) =>
                {
                    Console.WriteLine("Connection to Redis failed.");
                };

                if (!redisConnection.IsConnected)
                {
                    Console.WriteLine("Did not connect to Redis.");
                }

                return redisConnection;
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
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
