using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Communication
{
    
    /// generic class that prepares HTTP requests based on method, URL, and content
    /// part of the communication layer between CentralApp and WorkerNodeApp
  
    public class HttpRequestHelper
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        
        /// initializes a new instance of the HttpRequestHelper
       
        public HttpRequestHelper()
        {
            _httpClient = new HttpClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

      
        /// sends an HTTP request with the specified parameters
      
     
        
        public async Task<T> SendRequestAsync<T>(HttpMethod method, string url, object content = null)
        {
            // create the request
            var request = new HttpRequestMessage(method, url);

            // add content if provided
            if (content != null)
            {
                var json = JsonSerializer.Serialize(content, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            try
            {
                // send the request and get response
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // read and deserialize the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseContent))
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HTTP request: {ex.Message}");
                throw;
            }
        }

      
        /// sends a GET request
      
        public Task<T> GetAsync<T>(string url)
        {
            return SendRequestAsync<T>(HttpMethod.Get, url);
        }

       
        /// sends a POST request
      
        public Task<T> PostAsync<T>(string url, object content)
        {
            return SendRequestAsync<T>(HttpMethod.Post, url, content);
        }

      
        /// sends a PUT request
                public Task<T> PutAsync<T>(string url, object content)
        {
            return SendRequestAsync<T>(HttpMethod.Put, url, content);
        }

        
        /// sends a DELETE request
     
        public Task<T> DeleteAsync<T>(string url)
        {
            return SendRequestAsync<T>(HttpMethod.Delete, url);
        }
    }
}