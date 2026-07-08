using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;
using System.Security.Cryptography;
using System.Text;

namespace BusBookingDemo.Repository
{
    public class UserRepository(ApplicationDbContext context) : Repository<User>(context), IUserRepository
    {
        public string HashPassword(string password)
        {
            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            return hash;
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
