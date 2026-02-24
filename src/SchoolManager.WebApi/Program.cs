using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SchoolManager.Application.Commands.Alunos.CriarAluno;
using SchoolManager.Application.Commands.Alunos.LancarNota;
using SchoolManager.Application.Commands.Mensagens.EnviarMensagem;
using SchoolManager.Application.Commands.Professores.CriarProfessor;
using SchoolManager.Application.Commands.Turmas.CriarTurma;
using SchoolManager.Application.Queries.Turmas.ListarTurmas;
using SchoolManager.Infrastructure.DependencyInjection;
using SchoolManager.Infrastructure.Persistence.Contexts;
using SchoolManager.WebApi.Middleware;

// ── Serilog bootstrap ─────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "SchoolManager")
            .WriteTo.Console();

        // Em produção, adicionar MongoDB sink via appsettings
    });

    // ── Infrastructure (DbContexts, Repos, DomainEvent Handlers) ─────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Identity ──────────────────────────────────────────────────────────────
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
    {
        opts.Password.RequiredLength         = 8;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireUppercase       = true;
        opts.Password.RequireDigit           = true;
    })
    .AddEntityFrameworkStores<MasterDbContext>()
    .AddDefaultTokenProviders();

    // ── JWT ───────────────────────────────────────────────────────────────────
    var jwtSection  = builder.Configuration.GetSection("Jwt");
    var secretKey   = jwtSection["SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey não configurado.");

    builder.Services.AddAuthentication(opts =>
    {
        opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opts.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSection["Issuer"],
            ValidAudience            = jwtSection["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew                = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // ── Rate Limiting ─────────────────────────────────────────────────────────
    var rateLimitSection = builder.Configuration.GetSection("RateLimit");
    builder.Services.AddRateLimiter(opts =>
    {
        opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit       = rateLimitSection.GetValue<int>("PermitLimit", 100),
                    Window            = TimeSpan.FromMinutes(rateLimitSection.GetValue<int>("WindowMinutes", 1)),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit        = 0
                }));

        opts.OnRejected = async (ctx, ct) =>
        {
            ctx.HttpContext.Response.StatusCode = 429;
            await ctx.HttpContext.Response.WriteAsync("Taxa de requisições excedida. Tente novamente em breve.", ct);
        };
    });

    // ── API Versioning ────────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(opts =>
    {
        opts.DefaultApiVersion                = new ApiVersion(1, 0);
        opts.AssumeDefaultVersionWhenUnspecified = true;
        opts.ReportApiVersions                = true;
    });

    // ── Application Handlers ──────────────────────────────────────────────────
    // Commands
    builder.Services.AddScoped<CriarTurmaCommandHandler>();
    builder.Services.AddScoped<CriarProfessorCommandHandler>();
    builder.Services.AddScoped<CriarAlunoCommandHandler>();
    builder.Services.AddScoped<LancarNotaCommandHandler>();
    builder.Services.AddScoped<EnviarMensagemCommandHandler>();

    // Queries
    builder.Services.AddScoped<ListarTurmasQueryHandler>();

    // Validators (FluentValidation)
    builder.Services.AddValidatorsFromAssemblyContaining<CriarTurmaRequestValidator>();

    // ── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<MasterDbContext>("master-db")
        .AddDbContextCheck<EscolaDbContext>("escola-db");

    // ── Controllers + Swagger ─────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opts =>
    {
        opts.SwaggerDoc("v1", new() { Title = "SchoolManager API", Version = "v1" });
        opts.AddSecurityDefinition("Bearer", new()
        {
            Name         = "Authorization",
            Type         = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme       = "Bearer",
            BearerFormat = "JWT",
            In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description  = "Insira: Bearer {seu_token}"
        });
        opts.AddSecurityRequirement(new()
        {
            {
                new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });
    });

    // ── Build ─────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Pipeline ──────────────────────────────────────────────────────────────
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSerilogRequestLogging(opts =>
    {
        opts.EnrichDiagnosticContext = (diag, ctx) =>
        {
            diag.Set("UserId",   ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            diag.Set("EscolaId", ctx.User.FindFirst("EscolaId")?.Value);
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health").AllowAnonymous();

    Log.Information("SchoolManager iniciando...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha fatal ao iniciar a aplicação.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
