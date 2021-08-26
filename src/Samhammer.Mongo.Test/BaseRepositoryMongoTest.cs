using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Samhammer.Mongo.Test
{
    public class BaseRepositoryMongoTest : IDisposable
    {
        private readonly BaseRepositoryMongo<TestUserModel> repository;
        private readonly MongoDbConnector mongoDbConnector;
        private readonly MongoDbOptions mongoDbOptions;

        public BaseRepositoryMongoTest()
        {
            var connectorLogger = Substitute.For<ILogger<MongoDbConnector>>();
            var repoLogger = Substitute.For<ILogger<BaseRepositoryMongo<TestUserModel>>>();
            var conventions = Substitute.For<IMongoConventions>();

            mongoDbOptions = GetMongoDbOptions();
            var mongoDbIOptions = Microsoft.Extensions.Options.Options.Create(mongoDbOptions);

            mongoDbConnector = new MongoDbConnector(mongoDbIOptions, connectorLogger, conventions);
            repository = new BaseRepositoryMongo<TestUserModel>(repoLogger, mongoDbConnector);
        }

        [SkippableFact]
        public async Task RepositoryActions()
        {
            // Change the connection settings in GetMongoDbOptions() before debugging
            Skip.IfNot(Debugger.IsAttached, "Only for debugging");

            var user1 = new TestUserModel
            {
                FirstName = "Amy",
                LastName = "White",
            };

            var user2 = new TestUserModel
            {
                FirstName = "David",
                LastName = "Jones",
            };

            var user3 = new TestUserModel
            {
                FirstName = "Denise",
                LastName = "Graves",
            };

            var users = new List<TestUserModel> { user1, user2, user3 };

            await SaveNew(user1);
            await SaveNew(user2);
            await SaveNew(user3);
            await SaveUpdate(user1);
            await SaveWithInvalidId();

            await GetAll(users);

            await Delete(user1);
            await DeleteAll();
        }

        private async Task SaveNew(TestUserModel user)
        {
            await repository.Save(user);
            user.Id.Should().NotBeNullOrEmpty();

            var savedUser = await repository.GetById(user.Id);
            savedUser.Should().BeEquivalentTo(user);
        }

        private async Task SaveUpdate(TestUserModel user)
        {
            user.LastName = "White";

            await repository.Save(user);
            var savedUser = await repository.GetById(user.Id);

            savedUser.Should().BeEquivalentTo(user);
        }

        private async Task SaveWithInvalidId()
        {
            var user = new TestUserModel { Id = "1b2750bd666ed759583681ea" };
            await Assert.ThrowsAnyAsync<MongoRepositoryException>(() => repository.Save(user));
        }

        private async Task GetAll(List<TestUserModel> users)
        {
            var savedUsers = await repository.GetAll();
            savedUsers.Should().BeEquivalentTo(users);
        }

        private async Task Delete(TestUserModel user)
        {
            await repository.Delete(user);
            var savedUser = await repository.GetById(user.Id);

            savedUser.Should().BeNull();
        }

        private async Task DeleteAll()
        {
            await repository.DeleteAll();
            var users = await repository.GetAll();

            users.Should().BeEmpty();
        }

        private MongoDbOptions GetMongoDbOptions()
        {
            var guid = Guid.NewGuid().ToString();

            return new MongoDbOptions
            {
                DatabaseHost = "mymongodb.mydomain.tld:27017",
                DatabaseName = $"mydatabase-{guid}",
                AuthDatabaseName = "admin",
                UserName = $"admin",
                Password = "test",
            };
        }

        private async Task CleanDb()
        {
            var adminClient = mongoDbConnector.GetMongoClient();
            await adminClient.DropDatabaseAsync(mongoDbOptions.DatabaseName);
        }

        public void Dispose()
        {
            if (!Debugger.IsAttached)
            {
                return;
            }

            CleanDb().Wait();
        }
    }
}
