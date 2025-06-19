using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner.
// Contexto do banco de dados, repositórios, etc. viriam aqui.
// Exemplo: builder.Services.AddDbContext<AppDbContext>(...);
// builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();

builder.Services.AddControllers();

// 1. Configuração da Autenticação JWT
// ===================================================
// Pega a chave secreta do appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new ArgumentNullException("Jwt:Key", "A chave JWT não pode ser nula ou vazia no appsettings.json");
}
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Mude para true em produção
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // Tolera uma pequena diferença de tempo entre servidores
        ClockSkew = TimeSpan.Zero
    };
});

// Adiciona suporte para políticas de autorização
builder.Services.AddAuthorization();


// 2. Configuração do Swagger/OpenAPI para usar JWT
// ===================================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minha API Escolar", Version = "v1" });

    // Define o esquema de segurança (Bearer token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira 'Bearer' [espaço] e então seu token no campo abaixo.\n\nExemplo: \"Bearer 12345abcdef\""
    });

    // Adiciona o requisito de segurança para os endpoints
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


var app = builder.Build();

// Configura o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minha API v1");
        // Opcional: para manter o token após atualizar a página
        c.EnablePersistAuthorization();
    });
}

app.UseHttpsRedirection();

// 3. Habilita a autenticação e autorização
// ===================================================
// É crucial que UseAuthentication venha ANTES de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
