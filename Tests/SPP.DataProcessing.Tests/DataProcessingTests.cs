using Microsoft.EntityFrameworkCore;
using SPP.DataProcessing.Data;
using SPP.DataProcessing.Models;
using Moq;

namespace SPP.DataProcessing.Tests
{
    [TestClass]
    public class DataProcessingTests
    {
        [TestMethod]
        public void Region_Properties_ShouldWork()
        {
            // Arrange & Act
            var region = new Region
            {
                Id = 1,
                RegionCode = "EUR",
                RegionName = "Europe"
            };

            // Assert
            Assert.AreEqual(1, region.Id);
            Assert.AreEqual("EUR", region.RegionCode);
            Assert.AreEqual("Europe", region.RegionName);
        }

        [TestMethod]
        public void Agent_Properties_ShouldWork()
        {
            // Arrange & Act
            var agent = new Agent
            {
                AgentID = 1,
                Name = "John Doe",
                CodeName = "Eagle",
                Location = "Paris",
                DateOfBirth = new DateOnly(1980, 1, 1),
                Contact = "eagle@agency.com",
                LastMission = "Operation Nightfall",
                Status = "Active",
                ClearenceLevel = "Level 5",
                ArchiveNote = "Special agent"
            };

            // Assert
            Assert.AreEqual(1, agent.AgentID);
            Assert.AreEqual("John Doe", agent.Name);
            Assert.AreEqual("Eagle", agent.CodeName);
        }

        [TestMethod]
        public async Task GenericRepository_GetAllAsync_ShouldReturnEntities()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Region>>();
            var testData = new List<Region>
            {
                new Region { Id = 1, RegionCode = "EUR", RegionName = "Europe" },
                new Region { Id = 2, RegionCode = "NAM", RegionName = "North America" }
            };
            
            // Setup the mock to return our test data
            mockSet.As<IQueryable<Region>>().Setup(m => m.Provider).Returns(testData.AsQueryable().Provider);
            mockSet.As<IQueryable<Region>>().Setup(m => m.Expression).Returns(testData.AsQueryable().Expression);
            mockSet.As<IQueryable<Region>>().Setup(m => m.ElementType).Returns(testData.AsQueryable().ElementType);
            mockSet.As<IQueryable<Region>>().Setup(m => m.GetEnumerator()).Returns(testData.GetEnumerator());
            
            var mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            mockContext.Setup(c => c.Set<Region>()).Returns(mockSet.Object);
            
            var repository = new GenericRepository<Region>(mockContext.Object);
            
            // Act
            var result = await repository.GetAllAsync();
            
            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(r => r.RegionCode == "EUR"));
            Assert.IsTrue(result.Any(r => r.RegionCode == "NAM"));
        }

        [TestMethod]
        public async Task GenericRepository_GetByIdAsync_ShouldFindEntity()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Region>>();
            var region = new Region { Id = 1, RegionCode = "EUR", RegionName = "Europe" };
            
            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(region);
            
            var mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            mockContext.Setup(c => c.Set<Region>()).Returns(mockSet.Object);
            
            var repository = new GenericRepository<Region>(mockContext.Object);
            
            // Act
            var result = await repository.GetByIdAsync(1);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("EUR", result.RegionCode);
            Assert.AreEqual("Europe", result.RegionName);
        }

        [TestMethod]
        public async Task GenericRepository_AddAsync_ShouldCallAddMethod()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Region>>();
            var mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            mockContext.Setup(c => c.Set<Region>()).Returns(mockSet.Object);
            
            var repository = new GenericRepository<Region>(mockContext.Object);
            var region = new Region { Id = 1, RegionCode = "EUR", RegionName = "Europe" };
            
            // Act
            await repository.AddAsync(region);
            await repository.SaveAsync();
            
            // Assert
            mockSet.Verify(m => m.AddAsync(region, default), Times.Once);
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
        }
    }
}