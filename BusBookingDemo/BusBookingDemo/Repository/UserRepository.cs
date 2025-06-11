using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BusBookingDemo.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                return hash;
            }
        }

        //public User GetByUsername(string username)
        //{
        //    return Items.FirstOrDefault(u => u.Username == username);
        //}

        public void Update(User user)
        {
            Items.Update(user);
        }

        public bool VerifyPassword(string inputPassword, string storedHash)
        {
            var hashedInputPassword = HashPassword(inputPassword);

            // Hash'lenmiş kullanıcı şifresi ile veritabanındaki hash'lenmiş şifreyi karşılaştırın
            return hashedInputPassword == storedHash;
        }
    }

}
