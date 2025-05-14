using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using SPP.Communication.Models;
using SPP.Communication.Contracts;
using SPP.Communication.Services;
using WorkerNodeApp.Services;

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
    }
}