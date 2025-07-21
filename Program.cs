using financesApi.services;

var builder = WebApplication.CreateBuilder(args);
// builder.WebHost.UseUrls("https://localhost:5001");



// Add services to the container. # CORS redirection to use specified domains and HTTPS when domain is set
// Currently CORS allows everything -> FIX LATER
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://192.168.1.252:5173") // your React dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// // Set up Kestrel to use a certificate from a file for HTTPS
// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     serverOptions.ListenLocalhost(5001, listenOptions =>
//     {
//         listenOptions.UseHttps("certificate.pfx", "<secret>");
//     });
// });

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// builder.Services.AddScoped<DataService>();

var app = builder.Build();
app.UseCors(); // <== Add this BEFORE `UseAuthorization()` and `MapControllers()`

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();