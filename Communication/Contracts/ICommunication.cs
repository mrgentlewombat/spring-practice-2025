/// <summary>
/// provides a generic contract for HTTP-based communication between applications
/// defines methods for different types of HTTP requests with flexible response handling
/// </summary>
public interface ICommunication
{
    /// <summary>
    /// sends a GET request to retrieve data from a specified URL
    /// typically used for fetching status or retrieving information about a resource
    /// </summary>
 
    Task<T> GetAsync<T>(string url);

    /// <summary>
    /// sends a POST request to submit data or command to a specified URL
    /// used for sending commands like start, stop, or ping to a server
    /// </summary>
   
    Task<T> PostAsync<T>(string url, object content);

    /// <summary>
    /// sends a PUT request to update an existing resource at the specified URL
    /// used for modifying server-side resources or configurations
    /// </summary>
  
    Task<T> PutAsync<T>(string url, object content);

    /// <summary>
    /// sends a DELETE request to remove a resource at the specified URL
    /// used for deleting or canceling resources on the server
    /// </summary>

    Task<T> DeleteAsync<T>(string url);
}