using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Communication.Models;
using WorkerNodeApp.Services;

namespace WorkerNodeApp.Communication
{
   
    /// listens for HTTP commands from CentralApp and processes them
    /// this class allows WorkerNodeApp to receive HTTP requests
   
    public class CommandListener : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly string _url;
        private bool _isRunning;
        private readonly CommandProcessor _processor;

        
       
        
        public CommandListener(string ipAddress, int port, CommandProcessor processor)
        {
            _url = $"http://{ipAddress}:{port}/";
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _processor = processor;
        }

       
        /// starts listening for incoming commands
        public void Start()
        {
            if (_isRunning) return;

            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"Command listener started on {_url}");
            
            // start listening for requests in a background task
            Task.Run(ProcessRequestsAsync);
        }

        /// stops listening for commands
        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            Console.WriteLine("Command listener stopped");
        }

       
        /// main loop for processing incoming HTTP requests
        private async Task ProcessRequestsAsync()
        {
            while (_isRunning)
            {
                try
                {
                  
                    var context = await _listener.GetContextAsync();
                    
                    // process the request in another task to keep the listener responsive
                    _ = Task.Run(() => HandleRequestAsync(context));
                }
                catch (HttpListenerException)
                {
                    
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Console.WriteLine($"Error processing request: {ex.Message}");
                    }
                }
            }
        }

       
        /// handles an individual HTTP request by routing it to the appropriate handler
        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;
                
               
                response.ContentType = "application/json";

                // route the request based on path and method
                if (request.Url.AbsolutePath == "/api/command" && request.HttpMethod == "POST")
                {
                    await HandleCommandAsync(request, response);
                }
                else if (request.Url.AbsolutePath == "/api/status" && request.HttpMethod == "GET")
                {
                    await HandleStatusAsync(response);
                }
                else
                {
                    
                    SendErrorResponse(response, HttpStatusCode.NotFound, "Endpoint not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling request: {ex.Message}");
                SendErrorResponse(context.Response, HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                
                context.Response.Close();
            }
        }

        
        /// handles a command request by deserializing and processing it
        private async Task HandleCommandAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
           
            string requestBody;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            try
            {
               
                var commandRequest = JsonSerializer.Deserialize<CommandRequest>(requestBody, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (commandRequest == null)
                {
                    SendErrorResponse(response, HttpStatusCode.BadRequest, "Invalid command request");
                    return;
                }

                Console.WriteLine($"Received command: {commandRequest.Command}");
                
              
                var result = await _processor.ProcessCommandAsync(commandRequest.Command, commandRequest.Data);
                
               
                SendJsonResponse(response, HttpStatusCode.OK, result);
            }
            catch (JsonException)
            {
                SendErrorResponse(response, HttpStatusCode.BadRequest, "Invalid JSON in request");
            }
        }

       
        /// handles a status request by getting the current status
        private async Task HandleStatusAsync(HttpListenerResponse response)
        {
            var status = _processor.GetStatus();
            SendJsonResponse(response, HttpStatusCode.OK, status);
        }

       
        /// sends a JSON response with the specified status code and data
        private void SendJsonResponse(HttpListenerResponse response, HttpStatusCode statusCode, object data)
        {
            response.StatusCode = (int)statusCode;
            
          
            var json = JsonSerializer.Serialize(data, 
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var buffer = Encoding.UTF8.GetBytes(json);
            
        
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        
        /// sends an error response with the specified status code and message
        private void SendErrorResponse(HttpListenerResponse response, HttpStatusCode statusCode, string message)
        {
            response.StatusCode = (int)statusCode;
            
          
            var error = new { error = true, message };
            var json = JsonSerializer.Serialize(error);
            var buffer = Encoding.UTF8.GetBytes(json);
            
          
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

       
        /// disposes resources used by the listener
        public void Dispose()
        {
            Stop();
            _listener.Close();
        }
    }
}