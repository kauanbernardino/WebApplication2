// Models/User.cs
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty; // Nome de usuário único
        [Required]
        public string PasswordHash { get; set; } = string.Empty; // Senha com hash (NUNCA armazene senhas em texto plano!)
        public string Email { get; set; } = string.Empty; // Email do usuário
        // Você pode adicionar mais propriedades do usuário aqui, se precisar
    }

    public class UserRegisterDto // DTO para registro de usuário
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty; // Senha em texto plano ao registrar
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class UserLoginDto // DTO para login de usuário
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
