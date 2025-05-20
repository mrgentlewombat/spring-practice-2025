using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using SPP.Communication.Models;
using SPP.Communication.Contracts;
using SPP.Communication.Services;
using WorkerNodeApp.Services;
using WorkerNodeApp.Communication;

namespace SPP.Communication.Tests
{
    [TestClass]
    public class CommunicationTests
    {
        #region ICommunication POST Tests

        [TestMethod]
        public async Task ICommunication_PostAsync_ShouldHandleSuccessResponse()
        {
            // Arrange
            var mockCommunication = new Mock<ICommunication>();
            mockCommunication
                .Setup(c => c.PostAsync<CommandResponse>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(new CommandResponse
                {
                    Success = true,
                    Message = "Command executed successfully",
                    Result = "Done"
                });

            var command = new CommandRequest { Command = "start", Data = "test data" };

            // Act
            var result = await mockCommunication.Object.PostAsync<CommandResponse>(
                "http://test-server.com/api/command",
                command);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Command executed successfully", result.Message);
            Assert.AreEqual("Done", result.Result);

            // Verify that the method was called exactly once with the correct parameters
            mockCommunication.Verify(c => c.PostAsync<CommandResponse>(
                "http://test-server.com/api/command",
                command),
                Times.Once);
        }

        [TestMethod]
        public async Task ICommunication_PostAsync_ShouldHandleErrorResponse()
        {
            // Arrange
            var mockCommunication = new Mock<ICommunication>();
            mockCommunication
                .Setup(c => c.PostAsync<CommandResponse>(
                    It.Is<string>(url => url.Contains("error")),
                    It.IsAny<object>()))
                .ReturnsAsync(new CommandResponse
                {
                    Success = false,
                    Message = "Command failed",
                    Result = "Error"
                });

            // Act
            var result = await mockCommunication.Object.PostAsync<CommandResponse>(
                "http://test-server.com/api/error",
                new CommandRequest());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Command failed", result.Message);
        }

        #endregion

        #region Real Communication Implementation Tests

        [TestMethod]
        public void Communication_Constructor_ShouldCreateInstance()
        {
            // Act
            var comService = new Communication.Services.Communication();

            // Assert
            Assert.IsNotNull(comService);
            Assert.IsInstanceOfType(comService, typeof(ICommunication));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task Communication_PostAsync_ShouldThrowForInvalidUrl()
        {
            // Skip this test if no internet connection
            try
            {
                using (var client = new HttpClient())
                {
                    await client.GetAsync("https://www.google.com");
                }
            }
            catch
            {
                Assert.Inconclusive("This test requires internet connection");
                return;
            }

            // Arrange
            var comService = new Communication.Services.Communication();

            // Act - should throw exception for an invalid URL
            await comService.PostAsync<object>("http://invalid-url-that-does-not-exist.xyz", new { });
        }

        #endregion

        #region Model Tests

        [TestMethod]
        public void CommandRequest_Properties_ShouldWork()
        {
            // Arrange
            var request = new CommandRequest();

            // Act
            request.Command = "start";
            request.Data = "testData";

            // Assert
            Assert.AreEqual("start", request.Command);
            Assert.AreEqual("testData", request.Data);
        }

        [TestMethod]
        public void CommandResponse_Properties_ShouldWork()
        {
            // Arrange
            var response = new CommandResponse();

            // Act
            response.Success = true;
            response.Message = "Command executed";
            response.Result = "Success";

            // Assert
            Assert.IsTrue(response.Success);
            Assert.AreEqual("Command executed", response.Message);
            Assert.AreEqual("Success", response.Result);
        }

        #endregion

        #region CommandProcessor Tests

        [TestMethod]
        public async Task ProcessCommandAsync_Ping_ShouldReturnSuccess()
        {
            // Arrange
            var processor = new CommandProcessor();

            // Act
            var result = await processor.ProcessCommandAsync("ping", null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Command 'ping' executed successfully", result.Message);
            Assert.IsTrue(result.Result.Contains("Pong!"));
        }

        [TestMethod]
        public async Task ProcessCommandAsync_Hello_ShouldReturnPersonalizedGreeting()
        {
            // Arrange
            var processor = new CommandProcessor();
            var testName = "TestUser";

            // Act
            var result = await processor.ProcessCommandAsync("hello", testName);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Command 'hello' executed successfully", result.Message);
            Assert.AreEqual($"Hello, {testName}! Greetings from Worker Node.", result.Result);
        }

        [TestMethod]
        public async Task ProcessCommandAsync_Start_ShouldChangeStatusToWorking()
        {
            // Arrange
            var processor = new CommandProcessor();

            // Act
            var startResult = await processor.ProcessCommandAsync("start", "test-data");
            var status = processor.GetStatus();

            // Assert
            Assert.IsNotNull(startResult);
            Assert.IsTrue(startResult.Success);
            Assert.AreEqual("Working", status.Status);
            Assert.AreEqual(0, status.Progress); // At the beginning, progress should be 0

            // Cleanup - stop the task to avoid affecting other tests
            await processor.ProcessCommandAsync("stop", null);
        }

        [TestMethod]
        public async Task ProcessCommandAsync_Stop_ShouldChangeStatusToIdle()
        {
            // Arrange
            var processor = new CommandProcessor();
            await processor.ProcessCommandAsync("start", "test-data");

            // Act
            var stopResult = await processor.ProcessCommandAsync("stop", null);
            var status = processor.GetStatus();

            // Assert
            Assert.IsNotNull(stopResult);
            Assert.IsTrue(stopResult.Success);
            Assert.AreEqual("Idle", status.Status);
        }

        [TestMethod]
        public async Task ProcessCommandAsync_InvalidCommand_ShouldReturnError()
        {
            // Arrange
            var processor = new CommandProcessor();

            // Act
            var result = await processor.ProcessCommandAsync("invalid_command", null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("Unknown command"));
        }

        [TestMethod]
        public async Task ProcessCommandAsync_EmptyCommand_ShouldReturnError()
        {
            // Arrange
            var processor = new CommandProcessor();

            // Act
            var result = await processor.ProcessCommandAsync("", null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Command cannot be empty", result.Message);
        }

        [TestMethod]
        public void GetStatus_InitialState_ShouldReturnIdle()
        {
            // Arrange
            var processor = new CommandProcessor();

            // Act
            var status = processor.GetStatus();

            // Assert
            Assert.IsNotNull(status);
            Assert.IsTrue(status.Success);
            Assert.AreEqual("Idle", status.Status);
            Assert.AreEqual(0, status.Progress);
        }

        #endregion

        #region CommandStorage Tests
        [TestMethod]
        public void CommandStorage_AddCommand_ShouldAddCommandSuccessfully()
        {
            // Arrange
            var storage = new CommandStorage();
            var command = new UnifiedCommand
            {
                CommandId = "cmd-123",
                Type = "test",
                Payload = "test payload"
            };

            // Act
            var result = storage.AddCommand(command);
            bool commandExists = storage.TryGetCommand("cmd-123", out var retrievedCommand);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(commandExists);
            Assert.IsNotNull(retrievedCommand);
            Assert.AreEqual("cmd-123", retrievedCommand!.CommandId);
            Assert.AreEqual(CommandStorage.CommandStatus.Running, retrievedCommand.Status);
            Assert.AreEqual("test", retrievedCommand.Command.Type);
            Assert.AreEqual("test payload", retrievedCommand.Command.Payload);
        }

        [TestMethod]
        public void CommandStorage_AddCommand_ShouldReturnFalseForDuplicateCommandId()
        {
            // Arrange
            var storage = new CommandStorage();
            var command1 = new UnifiedCommand
            {
                CommandId = "cmd-123",
                Type = "test1",
                Payload = "payload1"
            };
            var command2 = new UnifiedCommand
            {
                CommandId = "cmd-123", // Same CommandId
                Type = "test2",
                Payload = "payload2"
            };

            // Act
            var result1 = storage.AddCommand(command1);
            var result2 = storage.AddCommand(command2);

            // Assert
            Assert.IsTrue(result1);
            Assert.IsFalse(result2); // Should fail because CommandId already exists
        }

        [TestMethod]
        public void CommandStorage_UpdateStatus_ShouldUpdateCommandStatus()
        {
            // Arrange
            var storage = new CommandStorage();
            var command = new UnifiedCommand
            {
                CommandId = "cmd-123",
                Type = "test",
                Payload = "test payload"
            };
            storage.AddCommand(command);

            // Act
            var result = storage.UpdateStatus("cmd-123", CommandStorage.CommandStatus.Completed);
            storage.TryGetCommand("cmd-123", out var retrievedCommand);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(CommandStorage.CommandStatus.Completed, retrievedCommand!.Status);
        }

        [TestMethod]
        public void CommandStorage_UpdateStatus_ShouldReturnFalseForNonExistentCommand()
        {
            // Arrange
            var storage = new CommandStorage();

            // Act
            var result = storage.UpdateStatus("non-existent-id", CommandStorage.CommandStatus.Completed);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CommandStorage_CancelCommand_ShouldCancelCommand()
        {
            // Arrange
            var storage = new CommandStorage();
            var command = new UnifiedCommand
            {
                CommandId = "cmd-123",
                Type = "test",
                Payload = "test payload"
            };
            storage.AddCommand(command);

            // Act
            var result = storage.CancelCommand("cmd-123");
            storage.TryGetCommand("cmd-123", out var retrievedCommand);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(CommandStorage.CommandStatus.Cancelled, retrievedCommand!.Status);
            Assert.IsTrue(retrievedCommand.CancellationTokenSource.IsCancellationRequested);
        }

        [TestMethod]
        public void CommandStorage_CancelCommand_ShouldReturnFalseForNonExistentCommand()
        {
            // Arrange
            var storage = new CommandStorage();

            // Act
            var result = storage.CancelCommand("non-existent-id");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CommandStorage_GetActiveCommands_ShouldReturnOnlyRunningCommands()
        {
            // Arrange
            var storage = new CommandStorage();

            // Add running command
            var runningCmd = new UnifiedCommand { CommandId = "running-cmd", Type = "test", Payload = "payload" };
            storage.AddCommand(runningCmd);

            // Add completed command
            var completedCmd = new UnifiedCommand { CommandId = "completed-cmd", Type = "test", Payload = "payload" };
            storage.AddCommand(completedCmd);
            storage.UpdateStatus("completed-cmd", CommandStorage.CommandStatus.Completed);

            // Add failed command
            var failedCmd = new UnifiedCommand { CommandId = "failed-cmd", Type = "test", Payload = "payload" };
            storage.AddCommand(failedCmd);
            storage.UpdateStatus("failed-cmd", CommandStorage.CommandStatus.Failed);

            // Add cancelled command
            var cancelledCmd = new UnifiedCommand { CommandId = "cancelled-cmd", Type = "test", Payload = "payload" };
            storage.AddCommand(cancelledCmd);
            storage.CancelCommand("cancelled-cmd");

            // Act
            var activeCommands = storage.GetActiveCommands().ToList();

            // Assert
            Assert.AreEqual(1, activeCommands.Count);
            Assert.AreEqual("running-cmd", activeCommands[0].CommandId);
            Assert.AreEqual(CommandStorage.CommandStatus.Running, activeCommands[0].Status);
        }

        [TestMethod]
        public void CommandStorage_Clear_ShouldCancelAndRemoveAllCommands()
        {
            // Arrange
            var storage = new CommandStorage();

            // Add multiple commands
            for (int i = 1; i <= 3; i++)
            {
                var command = new UnifiedCommand { CommandId = $"cmd-{i}", Type = "test", Payload = "payload" };
                storage.AddCommand(command);
            }

            // Act
            storage.Clear();
            var hasCommand1 = storage.TryGetCommand("cmd-1", out _);
            var hasCommand2 = storage.TryGetCommand("cmd-2", out _);
            var hasCommand3 = storage.TryGetCommand("cmd-3", out _);
            var activeCommands = storage.GetActiveCommands().ToList();

            // Assert
            Assert.IsFalse(hasCommand1);
            Assert.IsFalse(hasCommand2);
            Assert.IsFalse(hasCommand3);
            Assert.AreEqual(0, activeCommands.Count);
        }

        [TestMethod]
        public void CommandStorage_TryGetCommand_ShouldReturnFalseForNonExistentCommand()
        {
            // Arrange
            var storage = new CommandStorage();

            // Act
            var result = storage.TryGetCommand("non-existent-id", out var retrievedCommand);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(retrievedCommand);
        }

        [TestMethod]
        public void CommandStorage_MultipleConcurrentOperations_ShouldHandleConcurrencyCorrectly()
        {
            // Arrange
            var storage = new CommandStorage();
            var random = new Random();
            var commandIds = new List<string>();

            // Act - Add 100 commands concurrently
            Parallel.For(0, 100, i =>
            {
                var commandId = $"cmd-{i}";
                commandIds.Add(commandId);

                var command = new UnifiedCommand
                {
                    CommandId = commandId,
                    Type = "test",
                    Payload = $"payload-{i}"
                };

                storage.AddCommand(command);
            });

            // Update statuses for half of the commands
            Parallel.ForEach(commandIds.Take(50), commandId =>
            {
                var statusValue = random.Next(1, 4);
                var status = (CommandStorage.CommandStatus)statusValue;
                storage.UpdateStatus(commandId, status);
            });

            // Cancel 25 commands
            Parallel.ForEach(commandIds.Skip(50).Take(25), commandId =>
            {
                storage.CancelCommand(commandId);
            });

            // Assert
            var activeCommands = storage.GetActiveCommands().ToList();
            Assert.AreEqual(25, activeCommands.Count); // Only 25 commands should still be active (100 - 50 updated - 25 cancelled)

            // Verify all commands exist
            foreach (var commandId in commandIds)
            {
                Assert.IsTrue(storage.TryGetCommand(commandId, out _));
            }
        }

        #endregion

    }
}