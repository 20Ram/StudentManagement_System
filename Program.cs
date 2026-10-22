using Microsoft.EntityFrameworkCore;
using CrudOperation.Data;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using DotNetEnv;

// Env.Load();
Env.Load(Path.Combine(
    Directory.GetCurrentDirectory(),
    ".env"
));
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    )
);

builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddHttpClient<IChatService, ChatService>();
builder.Services.AddScoped<IChatDataService, ChatDataService>();
builder.Services.AddScoped<IFeePaymentAmountProvider, FeePaymentAmountProvider>();
builder.Services.AddScoped<IFeeService, FeeService>();   
builder.Services.AddHttpClient<IRazorpayPaymentService, RazorpayPaymentService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(20);
});

static string PartitionKey(HttpContext ctx) =>
    ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? ctx.Connection.RemoteIpAddress?.ToString()
    ?? "anonymous";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("create-order", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("verify-payment", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey!)
            )
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();   

app.UseStaticFiles();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
