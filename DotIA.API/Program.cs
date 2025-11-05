using Microsoft.EntityFrameworkCore;
using DotIA.API.Data;
using DotIA.API.Services;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ═══════════════════════════════════════════════════════════════════
// CONFIGURAR SERVICES
// ═══════════════════════════════════════════════════════════════════

// Configurar o DbContext com PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConexaoDotIA")));

// Adicionar Controllers
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar HttpClient e OpenAI Service
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

// Configurar CORS para permitir acesso do frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ═══════════════════════════════════════════════════════════════════
// BUILD APP
// ═══════════════════════════════════════════════════════════════════

var app = builder.Build();

// ═══════════════════════════════════════════════════════════════════
// VERIFICAR/CRIAR BANCO DE DADOS
// ═══════════════════════════════════════════════════════════════════

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Verificar se o banco existe e criar se necessário
        if (context.Database.CanConnect())
        {
            Console.WriteLine("✅ Conexão com banco de dados estabelecida!");
        }
        else
        {
            Console.WriteLine("⚠️  Criando banco de dados...");
            context.Database.EnsureCreated();
            Console.WriteLine("✅ Banco de dados criado com sucesso!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao conectar com o banco: {ex.Message}");
        Console.WriteLine("Verifique:");
        Console.WriteLine("1. PostgreSQL está rodando?");
        Console.WriteLine("2. String de conexão está correta no appsettings.json?");
        Console.WriteLine("3. Banco 'dotia' existe?");
    }
}

// ═══════════════════════════════════════════════════════════════════
// CONFIGURAR MIDDLEWARE
// ═══════════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ═══════════════════════════════════════════════════════════════════
// INICIAR APLICAÇÃO
// ═══════════════════════════════════════════════════════════════════

Console.WriteLine("🚀 DotIA API iniciada!");
Console.WriteLine($"📍 Swagger UI: http://localhost:5100/swagger");
Console.WriteLine($"📍 API Base: http://localhost:5100/api");

await app.RunAsync();
