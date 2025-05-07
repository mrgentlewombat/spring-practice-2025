namespace Communication.Models
{
    
    /// represents a command request sent from CentralApp to WorkerNodeApp
    public class CommandRequest
    {
        
        /// the command to execute (e.g., "start", "stop", "ping")
        public string Command { get; set; }
       
        /// optional data associated with the command
        public object Data { get; set; }
    }

   
    /// represents a response to a command execution
    public class CommandResponse
    {
        
    
        public bool Success { get; set; }
        
      
      
        public string Message { get; set; }
        
        
        public string Result { get; set; }
    }

   
    /// represents the status response from a worker node
    public class StatusResponse
    {
       
        public bool Success { get; set; }
        
     
        public string Message { get; set; }
        
      
        public string Status { get; set; }
        
       
        public double Progress { get; set; }
    }
}