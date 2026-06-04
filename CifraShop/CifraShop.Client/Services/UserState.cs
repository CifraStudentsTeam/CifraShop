namespace CifraShop.Client.Services
{
    public class UserState
    {
        public bool IsLoggedIn { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public int Balance { get; set; } = 1500;

        public void Login(string name, string email)
        {
            UserName = name;
            Email = email;
            IsLoggedIn = true;
        }

        public void Logout()
        {
            UserName = null;
            Email = null;
            IsLoggedIn = false;
            Balance = 1500; // сброс баланса для демо
        }
    }
}
