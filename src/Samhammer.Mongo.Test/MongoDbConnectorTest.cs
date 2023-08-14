using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Samhammer.Mongo.Test
{
    public class MongoDbConnectorTest
    {
        private readonly MongoDbConnector mongoDbConnector;
        private readonly MongoDbOptions mongoDbOptions;
        
        public MongoDbConnectorTest()
        {
            var connectorLogger = Substitute.For<ILogger<MongoDbConnector>>();
            var conventions = Substitute.For<IMongoConventions>();

            mongoDbOptions = GetMongoDbOptions();
            var mongoDbIOptions = Options.Create(mongoDbOptions);

            mongoDbConnector = new MongoDbConnector(mongoDbIOptions, connectorLogger, conventions);
        }

        [Fact]
        public void GetMongoClient_ReturnsCorrectClient()
        {
            // Arrange
            var credential1 = GetCredential("samhammer-mongo1", "user1", "password");
            var credential2 = GetCredential("samhammer-mongo2", "user2", "password");
            
            // Act
            var client1 = mongoDbConnector.GetMongoClient(credential1);
            var client2 = mongoDbConnector.GetMongoClient(credential2);

            // Assert
            Assert.NotNull(client1);
            Assert.NotNull(client2);
            Assert.Equal(credential1.DatabaseName, client1.Settings.Credential.Source);
            Assert.Equal(credential1.UserName, client1.Settings.Credential.Username);
            Assert.Equal(credential2.DatabaseName, client2.Settings.Credential.Source);
            Assert.Equal(credential2.UserName, client2.Settings.Credential.Username);
        }

        [Fact]
        public void GetMongoDatabase_ReturnsCorrectDatabase()
        {
            // Arrange
            var credential1 = GetCredential("samhammer-mongo1", "user1", "password");
            var credential2 = GetCredential("samhammer-mongo2", "user2", "password");

            // Act
            var database1 = mongoDbConnector.GetMongoDatabase(credential1.DatabaseName);
            var database2 = mongoDbConnector.GetMongoDatabase("samhammer-MONGO2"); // ignore case

            // Assert
            Assert.Equal(credential1.DatabaseName, database1.DatabaseNamespace.DatabaseName);
            Assert.Equal(credential2.DatabaseName, database2.DatabaseNamespace.DatabaseName);
        }

        private MongoDbOptions GetMongoDbOptions()
        {
            var credentials = new List<DatabaseCredential>
            {
                GetCredential("samhammer-mongo1", "user1", "password"),
                GetCredential("samhammer-mongo2", "user2", "password"),
            };
            return new MongoDbOptions
            {
                DatabaseCredentials = credentials,
            };
        }

        private DatabaseCredential GetCredential(string dbName, string user, string password)
        {
            return new DatabaseCredential
            {
                DatabaseHost = "localhost:27017",
                DatabaseName = dbName,
                UserName = user,
                Password = password,
            };
        }
    }
}
