using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Model;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURAÇÃO DOS SERVIÇOS (Tudo antes do Build)
// ==========================================

// Configura o CORS (Permite que seu Front-End acesse a API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configura o Banco de Dados SQLite
builder.Services.AddDbContext<Contexto>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("conexao")));

// Configura os Controllers
builder.Services.AddControllers();

// Configura a Autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    // Se não tiver chave no appsettings, cria uma provisória para não travar (apenas dev)
    jwtKey = "UmaChaveMuitoSecretaQueDeveTerPeloMenos32Caracteres!";
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Simplificado para facilitar o teste
        ValidateAudience = false, // Simplificado para facilitar o teste
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configura o Swagger com suporte a cadeado (JWT)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minha API Escolar", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira a palavra 'Bearer' [espaço] e seu token.\nExemplo: Bearer 12345abcdef"
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
                }
            },
            new string[] {}
        }
    });
});

// ==========================================
// 2. CONSTRUÇÃO DO APP (UMA VEZ SÓ)
// ==========================================
var app = builder.Build();

// ==========================================
// 3. PIPELINE DE EXECUÇÃO (Como o app se comporta)
// ==========================================

// Configura o Swagger

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseHttpsRedirection();

// APLICA O CORS (Isso é obrigatório vir antes da Autenticação)
app.UseCors("PermitirTudo");

// Autenticação e Autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ==========================================
// 4. PREPARAÇÃO DO BANCO (AUTO-MIGRATION)
// ==========================================
// Isso garante que o banco seja criado sozinho (Local e Docker)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Contexto>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro ao criar/migrar o banco de dados: " + ex.Message);
    }
}

app.Run();