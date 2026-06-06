using CifraShop.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CifraShop.Application.Services.Implementations
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var response = await  _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions);
        }
        
        public async Task<T> PostAsync<T>(string uri, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, data, _jsonSerializerOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions);
        }

        public async Task PostAsync(string uri, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, data, _jsonSerializerOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task PutAsync(string uri, object data)
        {
            var responce = await _httpClient.PutAsJsonAsync(uri, data, _jsonSerializerOptions);
            responce.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string uri)
        {
            var responce = await _httpClient.DeleteAsync(uri);
            responce.EnsureSuccessStatusCode();
        }
    }
}
