using ApiOnLamda;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using ApiOnLamda.Services;
using ApiOnLamda.Data;
using ApiOnLamda.Model;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Set up Google OAuth authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    options.Scope.Add("email");
    options.SaveTokens = true;
});

// Register custom Google authentication service
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.Configure<GoogleAuthConfig>(builder.Configuration.GetSection("Google"));

// Add controllers
builder.Services.AddControllers();

// Register DatabaseHelper for ADO.NET operations (no EF required)
builder.Services.AddScoped<DatabaseHelper>();

// Swagger/OpenAPI configuration
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SwaggerFileOperationFilter>();
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseStaticFiles(); // Serve files from wwwroot

// Optional: Explicitly serve only the "upload" folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload")),
    RequestPath = "/upload"
});

// Use the configured CORS policy
app.UseCors("AllowSpecificOrigin");

// Enable Swagger for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Enable controllers
app.MapControllers();

// Run the application
app.Run();







//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//builder.Services.AddScoped<ApiOnLamda.Data.DatabaseHelper>();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
////builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
//// Configure CORS to allow requests from your specific S3 bucket
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontendBucket",
//        policy =>
//        {
//            policy.WithOrigins("http://localhost:3000")
//                  .AllowAnyMethod()
//                  .AllowAnyHeader()
//                  .AllowCredentials(); // Include if using authentication cookies
//        });
//});

//var app = builder.Build();

//app.UseCors("AllowSpecificOrigin");
//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();
//app.MapGet("/users", () => "Hello User");
//app.MapControllers();

//app.Run();
