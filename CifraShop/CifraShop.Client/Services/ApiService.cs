using System.Net.Http.Json;
using CifraShop.Dto;

namespace CifraShop.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        // ===== STUDENTS =====

        public async Task<StudentResponse?> AuthenticateStudent(string login, string password)
        {
            var request = new AuthenticationStudentRequest { LoginName = login, Password = password };
            var response = await _http.PostAsJsonAsync("api/Student/authenticate", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<StudentResponse>();
        }

        public async Task<StudentResponse?> RegisterStudent(string login, string password, DateTime dateOfBirth)
        {
            var request = new RegisterStudentRequest
            {
                LoginName = login,
                Password = password,
                DateOfBirth = dateOfBirth
            };
            var response = await _http.PostAsJsonAsync("api/Student/register", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<StudentResponse>();
        }

        public async Task<StudentResponse?> GetStudentById(int id)
        {
            return await _http.GetFromJsonAsync<StudentResponse>($"api/Student/{id}");
        }

        public async Task<StudentResponse?> GetStudentByLogin(string login)
        {
            return await _http.GetFromJsonAsync<StudentResponse>($"api/Student/by-login/{login}");
        }

        public async Task<StudentResponse?> UpdateBalance(int studentId, uint newBalance)
        {
            var request = new UpdateBalanceRequest { NewBalance = newBalance };
            var response = await _http.PutAsJsonAsync($"api/Student/{studentId}/balance", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<StudentResponse>();
        }

        // ===== PRODUCTS =====

        public async Task<List<ProductResponce>?> GetAllProducts()
        {
            return await _http.GetFromJsonAsync<List<ProductResponce>>("api/Product/all");
        }

        public async Task<ProductResponce?> GetProductById(int id)
        {
            return await _http.GetFromJsonAsync<ProductResponce>($"api/Product/{id}");
        }

        // ===== ORDERS =====

        public async Task<OrderResponce?> CreateOrder(string customerLogin, uint sum, string status = "PaidFor")
        {
            var request = new CreateOrderRequest
            {
                CustomerLogin = customerLogin,
                Sum = sum,
                Status = status
            };
            var response = await _http.PostAsJsonAsync("api/Order", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<OrderResponce>();
        }

        public async Task<List<OrderResponce>?> GetOrdersByLogin(string login)
        {
            return await _http.GetFromJsonAsync<List<OrderResponce>>($"api/Order/by-login/{login}");
        }
    }
}
