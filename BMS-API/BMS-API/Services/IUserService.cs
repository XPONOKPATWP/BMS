using BMS.Model;

namespace BMS.Services
{
    public interface IUserService
    {
        User Authenticate(string name, string password);
        User Register(string name, string password);

    }
}
