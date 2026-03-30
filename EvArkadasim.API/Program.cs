// Program.cs
using MediatR;
using FluentValidation;
using Application.Common.Behaviors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;
using Persistence;
using Core.Security.JWT;
using Application.Features.Auths.Commands.SendVerificationCode;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;
using Microsoft.OpenApi.Models;
using Application.Services.Repositories;
using Persistence.Repositories;
using System.Text.Json.Serialization;
using EvArkadasim.API.Services.Receipts;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
}

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 1) Persistence (DbContext, Repos, MailService, JwtHelper, vs.)
builder.Services.AddPersistenceServices(builder.Configuration);

// --- Repositories ---
builder.Services.AddScoped<IExpenseRepository, EfExpenseRepository>();
builder.Services.AddScoped<IPersonalExpenseRepository, EfPersonalExpenseRepository>();
builder.Services.AddScoped<IShareRepository, EfShareRepository>();
builder.Services.AddScoped<IPaymentRepository, EfPaymentRepository>();
builder.Services.AddScoped<IHouseMemberRepository, EfHouseMemberRepository>();
builder.Services.AddScoped<IHouseNoteRepository, EfHouseNoteRepository>();
builder.Services.AddScoped<IInvitationRepository, EfInvitationRepository>();
builder.Services.AddScoped<ILedgerLineRepository, EfLedgerLineRepository>();
builder.Services.AddScoped<IRecurringChargeRepository, EfRecurringChargeRepository>();
builder.Services.AddScoped<IChargeCycleRepository, EfChargeCycleRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IReceiptOcrService, ReceiptOcrService>();

// 2) MediatR — Application assembly’sini tara (V11 uyumlu kayıt)
builder.Services.AddMediatR(typeof(Application.Features.Auths.Commands.SendVerificationCode.SendVerificationCodeCommand).Assembly);

// 3) AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 4) FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(SendVerificationCodeCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// 5) JWT
var tokenOptions = builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey))
        };
    });

// 6) MVC + JSON + Swagger + CORS
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EvArkadasim API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token'ınızı 'Bearer {token}' şeklinde girin."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            Array.Empty<string>()
        }
    });

    c.CustomSchemaIds(t => t.FullName);
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// ---- CORS: Mobil/uzak test icin gelistirme ortaminda esnek tutuyoruz ----
const string RemoteTestCorsPolicy = "RemoteTestCorsPolicy";
var configuredCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(p =>
{
    p.AddPolicy(RemoteTestCorsPolicy, x =>
    {
        if (builder.Environment.IsDevelopment())
        {
            x
                .SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        if (configuredCorsOrigins.Length > 0)
        {
            x
                .WithOrigins(configuredCorsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        x
            .WithOrigins("https://evarkadasim.co", "https://www.evarkadasim.co")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
// ------------------------------------------------------------

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 7) Kestrel: HTTP 5118 + HTTPS 7118
builder.WebHost.ConfigureKestrel(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.ListenAnyIP(5118); // Local/mobile development
    }
    else
    {
        options.Listen(IPAddress.Loopback, 5118); // Production: only local reverse proxy can reach API
    }

    if (builder.Environment.IsDevelopment())
    {
        options.ListenAnyIP(7118, listen => listen.UseHttps()); // Local development HTTPS
    }
});

var app = builder.Build();

// 8) Otomatik migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    EnsureReceiptTables(db);
}

// 9) Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        context.Response.ContentType = "application/json";

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Gonderilen veri dogrulamadan gecemedi.",
                errors = validationException.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    message = error.ErrorMessage
                })
            });
            return;
        }

        if (exception is InvalidOperationException invalidOperationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { message = invalidOperationException.Message });
            return;
        }

        if (exception is UnauthorizedAccessException unauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = unauthorizedAccessException.Message });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { message = "Beklenmeyen bir sunucu hatasi olustu." });
    });
});

app.UseForwardedHeaders();

// Dikkat: HTTPS yönlendirmesi SADECE prod'da Nginx tarafından yapıldığı için 
// uygulama içinde devre dışı bırakıyoruz. Aksi takdirde CORS preflight (OPTIONS)
// istekleri yönlendirme nedeniyle başarısız oluyor.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();
// }

app.UseStaticFiles();

app.UseCors(RemoteTestCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void EnsureReceiptTables(AppDbContext db)
{
    const string sql = """
IF OBJECT_ID(N'[Receipts]', N'U') IS NULL
BEGIN
    CREATE TABLE [Receipts] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [HouseId] INT NOT NULL,
        [UploadedByUserId] INT NOT NULL,
        [ImageUrl] NVARCHAR(1024) NOT NULL,
        [RawOcrText] NVARCHAR(MAX) NULL,
        [StoreName] NVARCHAR(256) NULL,
        [ReceiptDate] DATETIME2 NULL,
        [DetectedTotalAmount] DECIMAL(18,2) NULL,
        [Status] INT NOT NULL DEFAULT(0),
        [ConvertedExpenseId] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [FK_Receipts_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Receipts_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users]([Id])
    );

    CREATE INDEX [IX_Receipts_HouseId] ON [Receipts]([HouseId]);
    CREATE INDEX [IX_Receipts_UploadedByUserId] ON [Receipts]([UploadedByUserId]);
    CREATE INDEX [IX_Receipts_CreatedAt] ON [Receipts]([CreatedAt]);
    CREATE INDEX [IX_Receipts_HouseId_UploadedByUserId_CreatedAt] ON [Receipts]([HouseId], [UploadedByUserId], [CreatedAt]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receipts_CreatedAt' AND object_id = OBJECT_ID(N'[Receipts]'))
    CREATE INDEX [IX_Receipts_CreatedAt] ON [Receipts]([CreatedAt]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receipts_HouseId_UploadedByUserId_CreatedAt' AND object_id = OBJECT_ID(N'[Receipts]'))
    CREATE INDEX [IX_Receipts_HouseId_UploadedByUserId_CreatedAt] ON [Receipts]([HouseId], [UploadedByUserId], [CreatedAt]);

IF OBJECT_ID(N'[ReceiptItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [ReceiptItems] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ReceiptId] INT NOT NULL,
        [Name] NVARCHAR(256) NOT NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [Quantity] DECIMAL(18,2) NOT NULL,
        [LineTotal] DECIMAL(18,2) NOT NULL,
        [BoxLeft] INT NULL,
        [BoxTop] INT NULL,
        [BoxWidth] INT NULL,
        [BoxHeight] INT NULL,
        [IsAssigned] BIT NOT NULL DEFAULT(0),
        [IsShared] BIT NOT NULL DEFAULT(1),
        [PersonalUserId] INT NULL,
        [SortOrder] INT NOT NULL DEFAULT(0),
        CONSTRAINT [FK_ReceiptItems_Receipts_ReceiptId] FOREIGN KEY ([ReceiptId]) REFERENCES [Receipts]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ReceiptItems_ReceiptId] ON [ReceiptItems]([ReceiptId]);
END;

IF COL_LENGTH('ReceiptItems', 'BoxLeft') IS NULL
    ALTER TABLE [ReceiptItems] ADD [BoxLeft] INT NULL;
IF COL_LENGTH('ReceiptItems', 'BoxTop') IS NULL
    ALTER TABLE [ReceiptItems] ADD [BoxTop] INT NULL;
IF COL_LENGTH('ReceiptItems', 'BoxWidth') IS NULL
    ALTER TABLE [ReceiptItems] ADD [BoxWidth] INT NULL;
IF COL_LENGTH('ReceiptItems', 'BoxHeight') IS NULL
    ALTER TABLE [ReceiptItems] ADD [BoxHeight] INT NULL;
IF COL_LENGTH('ReceiptItems', 'IsAssigned') IS NULL
    ALTER TABLE [ReceiptItems] ADD [IsAssigned] BIT NOT NULL CONSTRAINT [DF_ReceiptItems_IsAssigned] DEFAULT(0);
""";

    db.Database.ExecuteSqlRaw(sql);
}
