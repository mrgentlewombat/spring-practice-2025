namespace Communication
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
        
        /// indicates if the status request was successful
      
        public bool Success { get; set; }
        

        /// description message about the status
    
        public string Message { get; set; }
        
        
        /// current state of the worker node (e.g., "Idle", "Working")
      
        public string Status { get; set; }
        
    
        /// progress of the current task as a percentage (0-100)
       
        public double Progress { get; set; }
    }
}