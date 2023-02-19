// Build configuration

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient("ApplicationServer1", client =>
{
    client.BaseAddress = new Uri("http://host.docker.internal:5000/");
});
builder.Services.AddHttpClient("ApplicationServer2", client =>
{
    client.BaseAddress = new Uri("http://host.docker.internal:5001/");
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

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
