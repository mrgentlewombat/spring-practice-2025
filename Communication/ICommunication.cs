using System.Threading.Tasks;

namespace Communication
{
  
    /// interface that defines the communication contract between CentralApp and WorkerNodeApp
    /// contains only the generic HTTP methods that are used for communication
   
    public interface ICommunication
    {
       
        /// sends a GET request to the specified URL
    
        Task<T> GetAsync<T>(string url);
     
        /// sends a POST request with content to the specified URL
      
        Task<T> PostAsync<T>(string url, object content);
        
      
        /// sends a PUT request with content to the specified URL
      
        Task<T> PutAsync<T>(string url, object content);
        
        
        /// sends a DELETE request to the specified URL
   
        Task<T> DeleteAsync<T>(string url);
    }
}