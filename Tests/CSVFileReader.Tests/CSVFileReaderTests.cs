using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPP.DataProcessing.Models;

namespace CSVFileReader.Tests
{
    [TestClass]
    public class CSVFileReaderTests
    {
        private string _testFilePath;

        [TestInitialize]
        public void Setup()
        {
            // Create a temporary test file
            _testFilePath = Path.GetTempFileName();

            // Write test CSV data
            File.WriteAllText(_testFilePath, @"AgentID,Name,CodeName,Location,DateOfBirth,Contact,LastMission,Status,ClearenceLevel,ArchiveNote
1,James Bond,007,London,1968-04-13,bond@mi6.gov.uk,Skyfall,Active,Level 5,Experienced agent
2,Jason Bourne,Delta,Paris,1970-09-13,bourne@cia.gov,Treadstone,Inactive,Level 4,Memory issues
3,Ethan Hunt,Ghost,Washington,1975-08-18,hunt@imf.gov,Fallout,Active,Level 5,Team leader");
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Delete the temporary test file
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [TestMethod]
        public void ReadAgents_ValidCsv_ReturnsCorrectAgentsList()
        {
            // Arrange
            var reader = new CsvAgentReader(); // Now uses parameterless constructor

            // Act
            var agents = reader.ReadAgents(_testFilePath);

            // Assert
            Assert.AreEqual(3, agents.Count);
            Assert.AreEqual(0, reader.Errors.Count);

            // Verify first agent
            var firstAgent = agents[0];
            Assert.AreEqual(1, firstAgent.AgentID);
            Assert.AreEqual("James Bond", firstAgent.Name);
            Assert.AreEqual("007", firstAgent.CodeName);
            Assert.AreEqual("London", firstAgent.Location);
            Assert.AreEqual(new DateOnly(1968, 4, 13), firstAgent.DateOfBirth);
            Assert.AreEqual("bond@mi6.gov.uk", firstAgent.Contact);
            Assert.AreEqual("Skyfall", firstAgent.LastMission);
            Assert.AreEqual("Active", firstAgent.Status);
            Assert.AreEqual("Level 5", firstAgent.ClearenceLevel);
            Assert.AreEqual("Experienced agent", firstAgent.ArchiveNote);
        }

        [TestMethod]
        public void ReadAgents_InvalidEmail_AddsErrorAndSkipsAgent()
        {
            // Arrange
            var reader = new CsvAgentReader();
            var invalidEmailCsv = @"AgentID,Name,CodeName,Location,DateOfBirth,Contact,LastMission,Status,ClearenceLevel,ArchiveNote
1,James Bond,007,London,1968-04-13,invalid-email,Skyfall,Active,Level 5,Experienced agent
2,Jason Bourne,Delta,Paris,1970-09-13,bourne@cia.gov,Treadstone,Inactive,Level 4,Memory issues";
            
            var invalidFilePath = Path.GetTempFileName();
            File.WriteAllText(invalidFilePath, invalidEmailCsv);

            try
            {
                // Act
                var agents = reader.ReadAgents(invalidFilePath);

                // Assert
                Assert.AreEqual(1, agents.Count); // Only one valid agent
                Assert.AreEqual(1, reader.Errors.Count); // One error for invalid email
                Assert.IsTrue(reader.Errors[0].Contains("Invalid email"));
            }
            finally
            {
                // Cleanup
                if (File.Exists(invalidFilePath))
                {
                    File.Delete(invalidFilePath);
                }
            }
        }

        [TestMethod]
        public void ReadAgents_InvalidDate_AddsErrorAndSkipsRecord()
        {
            // Arrange
            var reader = new CsvAgentReader();
            var invalidDateCsv = @"AgentID,Name,CodeName,Location,DateOfBirth,Contact,LastMission,Status,ClearenceLevel,ArchiveNote
1,James Bond,007,London,invalid-date,bond@mi6.gov.uk,Skyfall,Active,Level 5,Experienced agent
2,Jason Bourne,Delta,Paris,1970-09-13,bourne@cia.gov,Treadstone,Inactive,Level 4,Memory issues";
            
            var invalidFilePath = Path.GetTempFileName();
            File.WriteAllText(invalidFilePath, invalidDateCsv);

            try
            {
                // Act
                var agents = reader.ReadAgents(invalidFilePath);

                // Assert
                Assert.AreEqual(1, agents.Count); // Only one valid agent
                Assert.AreEqual(1, reader.Errors.Count); // One error for invalid date
                Assert.IsTrue(reader.Errors[0].Contains("parsing error"));
            }
            finally
            {
                // Cleanup
                if (File.Exists(invalidFilePath))
                {
                    File.Delete(invalidFilePath);
                }
            }
        }

        [TestMethod]
        public void ReadAgents_FileNotFound_ThrowsException()
        {
            // Arrange
            var reader = new CsvAgentReader();
            var nonExistentFilePath = "non_existent_file.csv";

            // Act & Assert
            var ex = Assert.ThrowsException<FileNotFoundException>(() => reader.ReadAgents(nonExistentFilePath));
        }

        [TestMethod]
        public void ReadAgents_EmptyCsv_ReturnsEmptyList()
        {
            // Arrange
            var reader = new CsvAgentReader();
            var emptyFilePath = Path.GetTempFileName();
            File.WriteAllText(emptyFilePath, "AgentID,Name,CodeName,Location,DateOfBirth,Contact,LastMission,Status,ClearenceLevel,ArchiveNote");

            try
            {
                // Act
                var agents = reader.ReadAgents(emptyFilePath);

                // Assert
                Assert.AreEqual(0, agents.Count);
                Assert.AreEqual(0, reader.Errors.Count);
            }
            finally
            {
                // Cleanup
                if (File.Exists(emptyFilePath))
                {
                    File.Delete(emptyFilePath);
                }
            }
        }

        [TestMethod]
        public void DateOnlyConverter_ValidDate_ConvertsCorrectly()
        {
            // Arrange
            var converter = new DateOnlyConverter();
            var mockReaderRow = new Mock<CsvHelper.IReaderRow>();
            var mockMemberMapData = new Mock<CsvHelper.Configuration.MemberMapData>(null);

            // Act
            var result = converter.ConvertFromString("2022-05-15", mockReaderRow.Object, mockMemberMapData.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(DateOnly));
            Assert.AreEqual(new DateOnly(2022, 5, 15), result);
        }

        [TestMethod]
        public void DateOnlyConverter_InvalidDate_ThrowsException()
        {
            // Arrange
            var converter = new DateOnlyConverter();
            var mockReaderRow = new Mock<CsvHelper.IReaderRow>();
            var mockContext = new Mock<CsvHelper.CsvContext>();
            mockReaderRow.Setup(r => r.Context).Returns(mockContext.Object);
            var mockMemberMapData = new Mock<CsvHelper.Configuration.MemberMapData>(null);

            // Act & Assert
            Assert.ThrowsException<CsvHelper.TypeConversion.TypeConverterException>(() => 
                converter.ConvertFromString("not-a-date", mockReaderRow.Object, mockMemberMapData.Object));
        }

        [TestMethod]
        public void AgentMap_MapsCorrectIndices()
        {
            // This is a reflection-based test to verify the mapping configuration
            var map = new AgentMap();
            var mappings = map.GetType()
                .BaseType
                .GetField("_mappings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(map) as System.Collections.IEnumerable;

            var mappingsList = new List<object>();
            foreach (var mapping in mappings)
            {
                mappingsList.Add(mapping);
            }

            // Assert that we have 10 mappings (one for each field)
            Assert.AreEqual(10, mappingsList.Count);
        }
    }
}