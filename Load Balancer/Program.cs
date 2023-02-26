using DistrLB.ServerStatusPoller;
using DistrLB.SignalR;


// Build configuration

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddHostedService<ServerStatusPoller>();

builder.Services.AddHttpClient("ApplicationServer1", client =>
{
    client.BaseAddress = new Uri("http://host.docker.internal:5001/");
});
builder.Services.AddHttpClient("ApplicationServer2", client =>
{
    client.BaseAddress = new Uri("http://host.docker.internal:5002/");
});
builder.Services.AddHttpClient("ApplicationServer3", client =>
{
    client.BaseAddress = new Uri("http://host.docker.internal:5003/");
});


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
    pattern: "{controller=LoadBalancer}/{action=GetApplicationServerHost}"
);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<SignalrHub>("/SignalrHub");

app.Run();
