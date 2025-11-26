using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models; // <--- Puxa o seu User.cs e os DTOs
using BCrypt.Net;             // <--- Puxa a criptografia

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        // Se o seu Contexto estiver em outro namespace (ex: WebApplication1.Model), ajuste o using
        private readonly WebApplication1.Model.Contexto _context;

        public AuthController(IConfiguration configuration, WebApplication1.Model.Contexto context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] UserRegisterDto request)
        {
            // Verifica se usuário já existe
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Usuário já existe.");
            }

            // 1. CRIPTOGRAFAR A SENHA
            // Isso transforma "123" em "$2a$11$uK..." (ninguém consegue ler)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash // Salva o hash, não a senha!
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Usuário registrado com sucesso!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized("Usuário não encontrado.");
            }

            // 2. VERIFICAR A SENHA
            // O BCrypt compara a senha digitada com o Hash do banco
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Senha incorreta.");
            }

            string token = CriarToken(user);
            return Ok(new { token = token, username = user.Username });
        }

        private string CriarToken(User user)
        {
            var key = Encoding.ASCII.GetBytes("EstaEUmaChaveSuperSecretaComMaisDe32CaracteresParaNaoDarErro123");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds,
                Issuer = "MinhaEscolaAPI",
                Audience = "MinhaEscolaApp"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}