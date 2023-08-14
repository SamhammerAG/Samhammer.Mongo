using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NSubstitute;
using Samhammer.Mongo.Abstractions;
using Samhammer.Mongo.Utils;
using Xunit;

namespace Samhammer.Mongo.Test
{
    public class MongoDbUtilsTest
    {
        private readonly MongoDbOptions mongoDbOptions;

        public MongoDbUtilsTest()
        {
            mongoDbOptions = GetMongoDbOptions();
            var mongoDbIOptions = Microsoft.Extensions.Options.Options.Create(mongoDbOptions);
        }

        [Theory]
        [InlineData(typeof(TestDataModel), "testData")]
        [InlineData(typeof(TestDataModelWithCustomName), "TestDataModel")]
        private void GetCollectionName(Type modelType, string collectionName)
        {
            var name = MongoDbUtils.GetCollectionName(modelType);
            name.Should().Be(collectionName);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("1234", false)]
        [InlineData("abcd", false)]
        [InlineData("1a3d", false)]
        [InlineData("5c9e1ae71d07dc6a14617513", true)]
        private void IsValidObjectId(string id, bool expected)
        {
            var result = MongoDbUtils.IsValidObjectId(id);
            result.Should().Be(expected);
        }

        [Fact]
        public void GetMongoClientSettings_ReturnsSettingsWithCredential()
        {
            // Act
            var result = MongoDbUtils.GetMongoClientSettings(mongoDbOptions.DatabaseCredentials[0]);

            // Assert
            Assert.Equal(mongoDbOptions.DatabaseCredentials[0].UserName, result.Credential.Username);
            Assert.Equal(mongoDbOptions.DatabaseCredentials[0].AuthDatabaseName, result.Credential.Source);

            var evidence = result.Credential.Evidence as PasswordEvidence;
            Assert.NotNull(evidence);

            var password = new PasswordEvidence(mongoDbOptions.DatabaseCredentials[0].Password);
            Assert.Equal(password, evidence);
        }

        [Fact]
        public void GetMongoUrl_ReturnsCorrectUrl()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            
            var credential = GetCredential();
            var expectedUrl = "mongodb://testuser:123@localhost/?authSource=admin";

            // Act
            var result = MongoDbUtils.GetMongoUrl(configuration, credential);

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void GetMongoUrl_ReturnsTruncateUrl()
        {
            // Arrange
            var configuration = Substitute.For<IConfiguration>();
            
            var credential = GetCredential();
            credential.AuthDatabaseName = null;
            credential.DatabaseName = "ThisIsVeryVeryVeryVeryVeryVeryLongDatabaseNameAndExceededTheMaxValueOfDatabase";
            var expectedUrl = "mongodb://testuser:123@localhost/?authSource=thisisveryveryveryveryveryverylongdatabasenameandexceededthemax";

            // Act
            var result = MongoDbUtils.GetMongoUrl(configuration, credential);

            // Assert
            Assert.Equal(expectedUrl, result);
        }

        private MongoDbOptions GetMongoDbOptions()
        {
            var credentials = new List<DatabaseCredential>
            {
                GetCredential(),
            };
            return new MongoDbOptions
            {
                DatabaseCredentials = credentials,
            };
        }

        private DatabaseCredential GetCredential()
        {
            return new DatabaseCredential
            {
                DatabaseName = "samhammer-mongo",
                UserName = "testuser",
                Password = "123",
                AuthDatabaseName = "admin",
                DatabaseHost = "localhost:27017",
            };
        }

        [MongoCollection]
        private class TestDataModel
        {
        }

        [MongoCollection("TestDataModel")]
        private class TestDataModelWithCustomName
        {
        }
    }
}
