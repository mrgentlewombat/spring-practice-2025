namespace Communication.Models
{
    // represents a command request sent from CentralApp to WorkerNodeApp
    // command that returns this result: "ping", "start", "stop"
    // method that returns this result: CommandRequest properties
    public class CommandRequest
    {
               public string Command { get; set; }
       
      
        public object Data { get; set; }
    }
}

 // represents a response to a command execution
    // command that returns this result: All commands
    // method that returns this result: CommandResponse properties
    public class CommandResponse
    {
        // indicates if the command was successful
        
        public bool Success { get; set; }
        
        // descriptive message about the command execution
    
        public string Message { get; set; }
        
        // optional result of the command
    
        public string Result { get; set; }
    }

       // represents the status response from a worker node
    // command that returns this result: "status"
    // method that returns this result: StatusResponse properties
    public class StatusResponse
    {
        // indicates if status retrieval was successful
        
        public bool Success { get; set; }
        
        // descriptive message about status
        
        public string Message { get; set; }
        
        // current state of the worker node
       
        public string Status { get; set; }
        
        // progress of current operation
      
        public double Progress { get; set; }
    } 