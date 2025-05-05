using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Communication.Http
{
    // generic class for preparing HTTP requests
    // method that returns this result: SendRequestAsync
    public class HttpRequestHelper
    {
        // HTTP client for network requests
        // command that returns this result: Used in all HTTP methods
       
        private readonly HttpClient _httpClient;

        // JSON serialization options
        // command that returns this result: Used in serializing "status", "ping", "start", "stop"
       
        private readonly JsonSerializerOptions _jsonOptions;

        // logger for error tracking
        // command that returns this result: error logging for all commands
      
        private readonly ILogger<HttpRequestHelper> _logger;

        public HttpRequestHelper(
            HttpClient httpClient, 
            ILogger<HttpRequestHelper> logger = null)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _logger = logger;
        }

        // sends a generic HTTP request
      
        public async Task<T> SendRequestAsync<T>(
            HttpMethod method, 
            string url, 
            object content = null)
        {
            // create HTTP request message
      
            var request = new HttpRequestMessage(method, url);

            // serialize request content
        
            if (content != null)
            {
                var json = JsonSerializer.Serialize(content, _jsonOptions);
                request.Content = new StringContent(
                    json, 
                    Encoding.UTF8, 
                    "application/json"
                );
            }

            try 
            {
                // send HTTP request
              
              
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // read response content
               
            
                var responseContent = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseContent))
                {
                    return default;
                }
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
              
                _logger?.LogError(ex, "Error in HTTP request");
                throw;
            }
        }

        // sends a GET request
        // command that returns this result: "status"
       
        public Task<T> GetAsync<T>(string url)
        {
            return SendRequestAsync<T>(HttpMethod.Get, url);
        }

        // sends a POST request
        // command that returns this result: "ping", "start", "stop"
       
        public Task<T> PostAsync<T>(string url, object content)
        {
            return SendRequestAsync<T>(HttpMethod.Post, url, content);
        }

        // sends a PUT request
        // command that returns this result: Configuration updates
       
        public Task<T> PutAsync<T>(string url, object content)
        {
            return SendRequestAsync<T>(HttpMethod.Put, url, content);
        }

        // sends a DELETE request
        // command that returns this result: Resource deletion
     
        public Task<T> DeleteAsync<T>(string url)
        {
            return SendRequestAsync<T>(HttpMethod.Delete, url);
        }
    }

    // exception for communication errors
   
    public class CommunicationException : Exception
    {
       
        public CommunicationException(string message) : base(message) { }
        
  
        public CommunicationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}