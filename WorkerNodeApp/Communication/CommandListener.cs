using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Communication.Models;
using Microsoft.Extensions.Logging;
using WorkerNodeApp.Services;

namespace WorkerNodeApp.Communication
{
    // listens for HTTP commands from CentralApp and processes them
    
    public class CommandListener : IDisposable
    {
        // HTTP listener for receiving requests
        // command that returns this result: All HTTP commands
        // method that returns this result: Start, ProcessRequestsAsync
        private readonly HttpListener _listener;
        
        // URL for listening
       
        private readonly string _url;
        
        // running state of listener
        // command that returns this result: All commands affecting listener state
        // method that returns this result: Start, Stop
        private bool _isRunning;
        
        // command processor for handling requests
        // command that returns this result: All registered commands
        //method that returns this result: HandleCommandAsync
        private readonly CommandProcessor _processor;
        
        // logger for tracking events
    
        private readonly ILogger<CommandListener> _logger;

       
        public CommandListener(
            string ipAddress, 
            int port, 
            CommandProcessor processor,
            ILogger<CommandListener> logger = null)
        {
            _url = $"http://{ipAddress}:{port}/";
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _processor = processor;
            _logger = logger;
        }

        // starts listening for incoming commands
        // command that returns this result: Listener startup
     
        public void Start()
        {
            if (_isRunning) return;

            _listener.Start();
            _isRunning = true;
            _logger?.LogInformation($"Command listener started on {_url}");
            
            // start listening for requests in a background task
            // command that returns this result: Background request processing
            // method that returns this result: ProcessRequestsAsync
            Task.Run(ProcessRequestsAsync);
        }

        // stops listening for commands
        // command that returns this result: Listener shutdown
       
        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            _logger?.LogInformation("Command listener stopped");
        }

        // main loop for processing incoming HTTP requests
 
        private async Task ProcessRequestsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    // get next HTTP context
                    // command that returns this result: Incoming HTTP requests
                    // method that returns this result: GetContextAsync
                    var context = await _listener.GetContextAsync();
                    
                    // process the request in another task to keep listener responsive
                    // command that returns this result: Request processing
                    //method that returns this result: HandleRequestAsync
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
                        _logger?.LogError(ex, "Error processing request");
                    }
                }
            }
        }

        // handles an individual HTTP request
      
     
        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;
                
                response.ContentType = "application/json";

                // route the request based on path and method
                // command that returns this result: Specific HTTP endpoints
                // method that returns this result: HandleRequestAsync
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
                _logger?.LogError(ex, "Error handling request");
                SendErrorResponse(context.Response, HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                context.Response.Close();
            }
        }

        // handles a command request
        // command that returns this result: "ping", "start", "stop"
       
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

                _logger?.LogInformation($"Received command: {commandRequest.Command}");
                
                var result = await _processor.ProcessCommandAsync(commandRequest.Command, commandRequest.Data);
                
                SendJsonResponse(response, HttpStatusCode.OK, result);
            }
            catch (JsonException)
            {
                SendErrorResponse(response, HttpStatusCode.BadRequest, "Invalid JSON in request");
            }
        }

        // handles a status request
        // command that returns this result: "status"
    
        private async Task HandleStatusAsync(HttpListenerResponse response)
        {
            var status = _processor.GetStatus();
            SendJsonResponse(response, HttpStatusCode.OK, status);
        }

        // sends a JSON response
        // command that returns this result: Various commands
        // method that returns this result: SendJsonResponse
        private void SendJsonResponse(HttpListenerResponse response, HttpStatusCode statusCode, object data)
        {
            response.StatusCode = (int)statusCode;
            
            var json = JsonSerializer.Serialize(data, 
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var buffer = Encoding.UTF8.GetBytes(json);
            
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

       
        private void SendErrorResponse(HttpListenerResponse response, HttpStatusCode statusCode, string message)
        {
            response.StatusCode = (int)statusCode;
            
            var error = new { error = true, message };
            var json = JsonSerializer.Serialize(error);
            var buffer = Encoding.UTF8.GetBytes(json);
            
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        // disposes resources used by the listener
     
        public void Dispose()
        {
            Stop();
            _listener.Close();
        }
    }
}