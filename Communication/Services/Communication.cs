using System.Threading.Tasks;
using Communication.Contracts;
using Communication.Http;

namespace Communication.Services
{
    // implementation of ICommunication interface
  
    public class Communication : ICommunication
    {
        // HTTP request helper for sending requests
      
  
        private readonly HttpRequestHelper _httpHelper;

        // constructor with dependency injection
     
        public Communication(HttpRequestHelper httpHelper)
        {
            _httpHelper = httpHelper;
        }

        // sends a GET request
        // command that returns this result: "status"
       
        public Task<T> GetAsync<T>(string url)
        {
            return _httpHelper.GetAsync<T>(url);
        }

        // sends a POST request
        // command that returns this result: "ping", "start", "stop"
       
        public Task<T> PostAsync<T>(string url, object content)
        {
            return _httpHelper.PostAsync<T>(url, content);
        }

        // sends a PUT request
        // command that returns this result: Configuration updates
    
        public Task<T> PutAsync<T>(string url, object content)
        {
            return _httpHelper.PutAsync<T>(url, content);
        }

        // Sends a DELETE request
       
        public Task<T> DeleteAsync<T>(string url)
        {
            return _httpHelper.DeleteAsync<T>(url);
        }
    }
}