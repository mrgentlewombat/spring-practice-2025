using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SPP.Communication.Models;
using SPP.WorkerNode.Services;

namespace SPP.WorkerNodeApp.Communication
namespace SPP.WorkerNode.Communication
{
    /// <summary>
    /// Listens for HTTP commands from CentralApp and processes them.
    /// This class allows WorkerNodeApp to receive HTTP requests.
    /// </summary>
    public class CommandListener : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly string _baseUrl;
        private bool _isRunning;
        private readonly CommandProcessor _processor;
        private readonly CommandStorage _commandStorage;
        private readonly ILogger<CommandListener> _logger;

        public CommandListener(string baseUrl, int port, CommandProcessor processor, ILogger<CommandListener> logger)
        {
            _baseUrl = $"http://{baseUrl.TrimEnd('/')}:{port}/";
            _listener = new HttpListener();
            _listener.Prefixes.Add($"{_baseUrl}api/");
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _commandStorage = new CommandStorage();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start()
        {
            if (_isRunning) return;

            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"Command listener started on {_baseUrl}");
            Console.WriteLine($"Command listener started on {_baseUrl}");

            Task.Run(ProcessRequestsAsync);
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            Console.WriteLine("Command listener stopped");
        }

        private async Task ProcessRequestsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
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
                        _logger.LogError(ex, "Error processing request");
                    }
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;
                response.ContentType = "application/json";

                if (request.HttpMethod != "POST")
                {
                    SendErrorResponse(response, HttpStatusCode.MethodNotAllowed, "Only POST method is supported");
                    return;
                }

                await HandleUnifiedCommandAsync(request, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling request");
                SendErrorResponse(context.Response, HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                context.Response.Close();
            }
        }

        private async Task HandleUnifiedCommandAsync(HttpListenerRequest request, HttpListenerResponse response)
        {
            string requestBody;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            try
            {
                var command = JsonSerializer.Deserialize<UnifiedCommand>(requestBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (command == null)
                {
                    _logger.LogWarning("Deserialized command is null.");
                    SendErrorResponse(response, HttpStatusCode.BadRequest, "Invalid command");
                    return;
                }

                if (string.IsNullOrEmpty(command.CommandId))
                {
                    command.CommandId = Guid.NewGuid().ToString();
                }

                _commandStorage.AddCommand(command);

                var commandResult = command.Type?.ToLower() switch
                {
                    "status" => await HandleStatusCommand(command),
                    "cancel" => await HandleCancelCommand(command),
                    "startprocessing" => await HandleStartProcessingCommand(command),
                    "sendstatistics" => await HandleSendStatisticsCommand(command),
                    "lockfiles" => await HandleLockFilesCommand(command),
                    _ => new UnifiedCommandResponse
                    {
                        Success = false,
                        Status = "Unknown command",
                        CommandId = command.CommandId
                    }
                };

                if (_commandStorage.TryGetCommand(command.CommandId, out var cmdInfo))
                {
                    _commandStorage.UpdateStatus(command.CommandId,
                        commandResult.Success ? CommandStorage.CommandStatus.Completed : CommandStorage.CommandStatus.Failed);
                }

                SendJsonResponse(response, HttpStatusCode.OK, commandResult);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "Invalid JSON received");
                SendErrorResponse(response, HttpStatusCode.BadRequest, "Invalid JSON in request");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling command");
                SendErrorResponse(response, HttpStatusCode.InternalServerError, "Internal server error");
            }
        }

        private Task<UnifiedCommandResponse> HandleStatusCommand(UnifiedCommand command)
        {
            if (_commandStorage.TryGetCommand(command.CommandId, out var cmdInfo))
            {
                return Task.FromResult(new UnifiedCommandResponse
                {
                    Success = true,
                    Status = cmdInfo.Status.ToString(),
                    CommandId = command.CommandId
                });
            }

            return Task.FromResult(new UnifiedCommandResponse
            {
                Success = false,
                Status = "Command not found",
                CommandId = command.CommandId
            });
        }

        private Task<UnifiedCommandResponse> HandleCancelCommand(UnifiedCommand command)
        {
            var success = _commandStorage.CancelCommand(command.CommandId);
            return Task.FromResult(new UnifiedCommandResponse
            {
                Success = success,
                Status = success ? "Command cancelled" : "Command not found",
                CommandId = command.CommandId
            });
        }

        private async Task<UnifiedCommandResponse> HandleStartProcessingCommand(UnifiedCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Payload?.ToString()))
            {
                _logger.LogWarning("Received 'startprocessing' command with null or empty payload.");
                return new UnifiedCommandResponse
                {
                    Success = false,
                    Status = "Payload is empty",
                    CommandId = command.CommandId
                };
            }

            _commandStorage.AddCommand(command);
            await _processor.ProcessCommandAsync(command.Type, command.Payload);

            return new UnifiedCommandResponse
            {
                Success = true,
                Status = "Processing started",
                CommandId = command.CommandId
            };
        }

        private Task<UnifiedCommandResponse> HandleSendStatisticsCommand(UnifiedCommand command)
        {
            return Task.FromResult(new UnifiedCommandResponse
            {
                Success = true,
                Status = "Statistics sent",
                CommandId = command.CommandId
            });
        }

        private Task<UnifiedCommandResponse> HandleLockFilesCommand(UnifiedCommand command)
        {
            return Task.FromResult(new UnifiedCommandResponse
            {
                Success = true,
                Status = "Files locked",
                CommandId = command.CommandId
            });
        }

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

        public void Dispose()
        {
            Stop();
            _listener.Close();
        }
    }
}
