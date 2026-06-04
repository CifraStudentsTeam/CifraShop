namespace CifraShop.Client.Services
{
    public class UserState
    {
        private readonly ApiService _api;

        public UserState(ApiService api)
        {
            _api = api;
        }

        public bool IsLoggedIn { get; private set; }
        public uint? StudentId { get; private set; }
        public string? UserName { get; private set; }
        public string? Email { get; private set; }
        public uint Balance { get; private set; }

        public event Action? OnChange;

        public async Task<bool> Login(string login, string password)
        {
            var student = await _api.AuthenticateStudent(login, password);
            if (student == null) return false;

            StudentId = student.Id;
            UserName = student.LoginName;
            Email = student.LoginName;
            Balance = student.Balance;
            IsLoggedIn = true;

            NotifyStateChanged();
            return true;
        }

        public async Task<bool> Register(string login, string password, DateTime dateOfBirth)
        {
            var student = await _api.RegisterStudent(login, password, dateOfBirth);
            if (student == null) return false;

            StudentId = student.Id;
            UserName = student.LoginName;
            Email = student.LoginName;
            Balance = student.Balance;
            IsLoggedIn = true;

            NotifyStateChanged();
            return true;
        }

        public async Task RefreshBalance()
        {
            if (!IsLoggedIn || StudentId == null) return;
            var student = await _api.GetStudentById((int)StudentId.Value);
            if (student != null)
            {
                Balance = student.Balance;
                NotifyStateChanged();
            }
        }

        public async Task TopUpBalance(uint amount)
        {
            if (!IsLoggedIn || StudentId == null) return;
            var newBalance = Balance + amount;
            var student = await _api.UpdateBalance((int)StudentId.Value, newBalance);
            if (student != null)
            {
                Balance = student.Balance;
                NotifyStateChanged();
            }
        }

        public async Task SpendBalance(uint amount)
        {
            if (!IsLoggedIn || StudentId == null) return;
            if (Balance < amount) return;

            var newBalance = Balance - amount;
            var student = await _api.UpdateBalance((int)StudentId.Value, newBalance);
            if (student != null)
            {
                Balance = student.Balance;
                NotifyStateChanged();
            }
        }

        public void Logout()
        {
            IsLoggedIn = false;
            StudentId = null;
            UserName = null;
            Email = null;
            Balance = 0;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}