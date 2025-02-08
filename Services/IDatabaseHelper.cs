using System.Data;
using ApiOnLamda.Model;
namespace ApiOnLamda.Data
{
    public interface IDatabaseHelper
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
        // Additional methods as needed (e.g., UpdateUser, DeleteUser, etc.)
    }
}
