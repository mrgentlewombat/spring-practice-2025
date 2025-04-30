using System.Threading.Tasks;
using Communication.Contracts;
using Communication.Http;

namespace Communication.Services
{
   
    /// implementation of the ICommunication interface
    /// this class provides the actual implementation of the generic HTTP methods using the HttpRequestHelper class
   
    public class Communication : ICommunication
    {
        private readonly HttpRequestHelper _httpHelper;

     
        
       
        public Communication()
        {
            _httpHelper = new HttpRequestHelper();
        }

        /// sends a GET request to the specified URL
        public Task<T> GetAsync<T>(string url)
        {
            return _httpHelper.GetAsync<T>(url);
        }

        
        /// sends a POST request with content to the specified URL
        public Task<T> PostAsync<T>(string url, object content)
        {
            return _httpHelper.PostAsync<T>(url, content);
        }

        
        /// sends a PUT request with content to the specified URL
        public Task<T> PutAsync<T>(string url, object content)
        {
            return _httpHelper.PutAsync<T>(url, content);
        }

      
        /// sends a DELETE request to the specified URL
        public Task<T> DeleteAsync<T>(string url)
        {
            return _httpHelper.DeleteAsync<T>(url);
        }
    }
}