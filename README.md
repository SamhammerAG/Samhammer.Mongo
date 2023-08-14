# Samhammer.Mongo

## Usage
This package provides access to mongodb over the mongodb driver. It includes basic access functionality to the database and provides a possibility to define models.
The package also supports multiple connections from different servers.

#### How to add this to your project:
- reference this package to your main project: https://www.nuget.org/packages/Samhammer.Mongo/
- reference this package to your model project: https://www.nuget.org/packages/Samhammer.Mongo.Abstractions/
- initialize mongodb in Program.cs
- add the health check to Program.cs (optional)
- add the mongodb configuration to the appsettings (if the lib is initialized with IConfiguration in Program.cs)

#### Example Program.cs:
```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddMongoDb(Configuration); // Init by configuration or action

       services.AddHealthChecks()
                .AddMongoDb();       
   }
```

##### For multiple credentials:
```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddMongoDb(Configuration); // Init by configuration or action

       services.AddHealthChecks()
                .AddMongoDb(credentials);       
   }
```

#### Example appsettings configuration:
```json
  "MongoDbOptions": {
    "DatabaseCredentials": [
      {
        "UserName": "dbuser",
        "Password": "dbpassword",
        "DatabaseName": "dbname",
        "AuthDatabaseName": "admin", // defaults to the database name
        "DatabaseHost": "dbhost.tld",
        "ConnectionString": "mongodb://dbhost.tld" // alternative to DatabaseHost
      },
      {
        "UserName": "dbuser2",
        "Password": "dbpassword2",
        "DatabaseName": "dbname2",
        "AuthDatabaseName": "admin", // defaults to the database name
        "DatabaseHost": "dbhost2.tld",
        "ConnectionString": "mongodb://dbhost2.tld" // alternative to DatabaseHost
      }
    ]
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

Here is an example with the additional method GetByLoginName. If the database name is not provided, the library will use the first set of DatabaseCredentials in appsetting:

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

To specify which credential should be used:
```csharp
    public class UserRepositoryMongo : BaseRepositoryMongo<UserModel>, IUserRepositoryMongo
    {
        public UserRepositoryMongo(ILogger<UserRepositoryMongo> logger, IMongoDbConnector connector)
            : base(logger, connector, "dbName")
        {
        }
    }
```

### Query Logging

It is possible to log the plain queries, by enabling trace logging for Samhammer.Mongo.
Here is an example with Serilog configured in appsettings.json

```json
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Samhammer.Mongo":  "Verbose"
      }
    }
  },
```

Here is an example how the log message will look like:

```
mongodb command: {
    "find" : "label",
    "filter" : { "ProjectId" : ObjectId("63bfb836e9103fd33216f6b8") },
    "$db" : "mytestdb",
    "lsid" : { "id" : CSUUID("c95ac0da-18d3-4fda-ac27-fe8e9eb0e034") }
}
```

## Contribute

#### How to publish package
- create git tag
- The nuget package will be published automatically by a github action
