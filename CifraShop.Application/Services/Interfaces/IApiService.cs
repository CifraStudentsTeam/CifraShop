using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IApiService
    {
        public Task<T> GetAsync<T>(string uri);
        public Task<T> PostAsync<T>(string uri, object data);
        public Task PostAsync(string uri, object data);
        public Task PutAsync(string uri, object data);
        public Task DeleteAsync(string uri);
    }
}
