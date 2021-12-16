# Samhammer.Mongo

## Usage
This package provides access to mongodb over the mongodb driver. It includes basic access functionality to the database and provides a possibility to define models.

#### How to add this to your project:
- reference this package to your main project: https://www.nuget.org/packages/Samhammer.Mongo/
- reference this package to your model project: https://www.nuget.org/packages/Samhammer.Mongo.Abstractions/
- initialize mongodb in Startup.cs
- add the health check to Startup.cs (optional)
- add the mongodb configuration to the appsettings (if the lib is initialized with IConfiguration in Startup.cs)

#### Example Startup.cs:
```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddMongoDb(Configuration); // Init by configuration or action

       services.AddHealthChecks()
                .AddMongoDb();       
   }
```

#### Example appsettings configuration:
```json
  "MongoDbOptions": {
    "UserName": "dbuser",
    "Password": "dbpassword",
    "DatabaseName": "dbname",
    "AuthDatabaseName": "admin", // defaults to the database name
    "DatabaseHost": "dbhost.tld",
    "ConnectionString": "mongodb://dbhost.tld" // alternative to DatabaseHost
  },
```

### Example model

Model for a collection with the name "user".

```csharp
   [MongoCollection]
    public class UserModel : BaseModelMongo
    {
        public string LoginName { get; set; }
    }
```

### Example repository

The base repo provides the following actions:
  * Task<T> GetById(string id);
  *  Task<List<T>> GetAll();
  * Task Save(T model);
  * Task Delete(T model);
  * Task DeleteAll();

Here is an example with the additional method GetByLoginName:

```csharp
    public class UserRepositoryMongo : BaseRepositoryMongo<UserModel>, IUserRepositoryMongo
    {
        public UserRepositoryMongo(ILogger<UserRepositoryMongo> logger, IMongoDbConnector connector)
            : base(logger, connector)
        {
        }

        public async Task<UserModel> GetByLoginName(string loginName)
        {
            var entries = await Collection.FindAsync(i => i.LoginName == loginName);
            return entries.FirstOrDefault();
        }
    }

    public interface IUserRepositoryMongo : IBaseRepositoryMongo<UserModel>
    {
        Task<UserModel> GetByLoginName(string loginName);
    }
```

## Contribute

#### How to publish package
- set package version in Samhammer.Mongo.csproj and Samhammer.Mongo.Abstractions.csproj
- create git tag
- The nuget package will be published automatically by a github action
