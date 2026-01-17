namespace Exercise.Services
{
    public class AuthService
    {
        public bool ValidateUser(string username, string password)
        {
            if (username == "admin" && password == "123456")
            {
                return true;
            }
            return false;
        }
    }
}