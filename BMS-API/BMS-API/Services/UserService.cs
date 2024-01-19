using System.Text;
using BMS.Model;
using Microsoft.AspNetCore.Identity;

namespace BMS.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public User Authenticate(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);

            //if (user == null || !VerifyPasswordHash(password, user.Password))
            if (user == null || !password.Equals(user.Password))
                return null;

            return user;
        }

        public User Register(string email, string password)
        {
            if (_context.Users.Any(u => u.Email == email))
                throw new Exception("Email already taken");

            //string passwordHash = CreatePasswordHash(password);

            //var user = new User { Email = email, Password = passwordHash };
            var user = new User { Email = email, Password = password };
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        //private string CreatePasswordHash(string password)
        //{
        //    using var hmac = new System.Security.Cryptography.HMACSHA512();
        //    return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        //}

        //private bool VerifyPasswordHash(string password, string storedHash)
        //{
        //    using var hmac = new System.Security.Cryptography.HMACSHA512();
        //    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        //    return storedHash == Convert.ToBase64String(computedHash);
        //}
    }

}

