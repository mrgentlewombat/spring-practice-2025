using System.Threading.Tasks;

namespace Communication.Contracts
{
    // communication contract for HTTP requests between applications
    public interface ICommunication
    {
        // sends a GET request to the specified URL
        // command that returns this result: "status"
        Task<T> GetAsync<T>(string url);
        
        // sends a POST request with content
        // commands that return this result: "ping", "start", "stop"
        Task<T> PostAsync<T>(string url, object content);
        
        // sends a PUT request to update an existing resource
        // command that returns this result: configuration updates
        Task<T> PutAsync<T>(string url, object content);
        
        // sends a DELETE request to remove a resource
        // command that returns this result: task cancellation
        Task<T> DeleteAsync<T>(string url);
    }
}