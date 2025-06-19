// Services/UserService.cs
using WebApplication1.Models;
using BCrypt.Net; // Para hash de senhas

namespace WebApplication1.Services
{
    public interface IUserService
    {
        User? GetByUsername(string username);
        User Register(UserRegisterDto userDto);
        bool VerifyPassword(string hashedPassword, string password);
    }

    // Para fins de DEMONSTRAÇÃO, usaremos uma lista em memória.
    // Em uma aplicação real, aqui você usaria um banco de dados (EF Core com SQL Server, PostgreSQL, etc.).
    public class InMemoryUserService : IUserService
    {
        private readonly List<User> _users = new List<User>();
        private int _nextId = 1;

        public InMemoryUserService()
        {
            // Adicionar um usuário de teste inicial (opcional)
            // Username: testuser, Senha: password123, Email: test@example.com
            _users.Add(new User { Id = _nextId++, Username = "testuser", Email = "test@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123") });
        }

        public User? GetByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public User Register(UserRegisterDto userDto)
        {
            if (GetByUsername(userDto.Username) != null)
            {
                throw new InvalidOperationException("Nome de usuário já existe.");
            }

            var newUser = new User
            {
                Id = _nextId++,
                Username = userDto.Username,
                Email = userDto.Email,
                // Faça o hash da senha antes de armazenar!
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
            };
            _users.Add(newUser);
            return newUser;
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            // Verifica a senha fornecida com o hash armazenado
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
