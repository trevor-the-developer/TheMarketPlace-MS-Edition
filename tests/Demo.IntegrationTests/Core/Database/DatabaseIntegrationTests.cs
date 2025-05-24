using Demo.IntegrationTests.Core.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Npgsql;
using Xunit;
using Minio;
using Minio.DataModel.Args;

namespace Demo.IntegrationTests.Core.Database;

public class DatabaseIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task PostgreSQL_Connection_Should_Be_Established_Successfully()
    {
        // Arrange & Act
        await using var connection = new NpgsqlConnection(PostgreSqlConnectionString);
        await connection.OpenAsync();

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    [Fact]
    public async Task PostgreSQL_Should_Support_Basic_CRUD_Operations()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(PostgreSqlConnectionString);
        await connection.OpenAsync();

        // Create table
        await using var createCmd = new NpgsqlCommand(
            "CREATE TABLE test_table (id SERIAL PRIMARY KEY, name VARCHAR(100), created_at TIMESTAMP)", 
            connection);
        await createCmd.ExecuteNonQueryAsync();

        // Act - Insert
        await using var insertCmd = new NpgsqlCommand(
            "INSERT INTO test_table (name, created_at) VALUES (@name, @created_at) RETURNING id", 
            connection);
        insertCmd.Parameters.AddWithValue("name", "Test Item");
        insertCmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);
        
        var insertedId = await insertCmd.ExecuteScalarAsync();

        // Act - Select
        await using var selectCmd = new NpgsqlCommand(
            "SELECT name FROM test_table WHERE id = @id", 
            connection);
        selectCmd.Parameters.AddWithValue("id", insertedId);
        
        var retrievedName = await selectCmd.ExecuteScalarAsync();

        // Assert
        insertedId.Should().NotBeNull();
        retrievedName.Should().Be("Test Item");
    }

    [Fact]
    public async Task MongoDB_Connection_Should_Be_Established_Successfully()
    {
        // Arrange
        var client = new MongoClient(MongoDbConnectionString);
        var database = client.GetDatabase("test_db");

        // Act
        var collections = await database.ListCollectionNamesAsync();
        var collectionList = await collections.ToListAsync();

        // Assert
        collectionList.Should().NotBeNull();
    }

    [Fact]
    public async Task MongoDB_Should_Support_Document_Operations()
    {
        // Arrange
        var client = new MongoClient(MongoDbConnectionString);
        var database = client.GetDatabase("test_db");
        var collection = database.GetCollection<TestDocument>("test_collection");

        var testDocument = new TestDocument
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Document",
            CreatedAt = DateTime.UtcNow
        };

        // Act - Insert
        await collection.InsertOneAsync(testDocument);

        // Act - Find
        var retrievedDocument = await collection
            .Find(d => d.Id == testDocument.Id)
            .FirstOrDefaultAsync();

        // Assert
        retrievedDocument.Should().NotBeNull();
        retrievedDocument!.Name.Should().Be("Test Document");
        retrievedDocument.Id.Should().Be(testDocument.Id);
    }

    [Fact]
    public async Task MinIO_Connection_Should_Be_Established_Successfully()
    {
        // Arrange
        var minioClient = new MinioClient()
            .WithEndpoint(MinioEndpoint)
            .WithCredentials("minioadmin", "minioadmin")
            .WithSSL(false)
            .Build();

        // Act
        var buckets = await minioClient.ListBucketsAsync();

        // Assert
        buckets.Should().NotBeNull();
        buckets.Buckets.Should().NotBeNull();
    }

    [Fact]
    public async Task MinIO_Should_Support_File_Operations()
    {
        // Arrange
        var minioClient = new MinioClient()
            .WithEndpoint(MinioEndpoint)
            .WithCredentials("minioadmin", "minioadmin")
            .WithSSL(false)
            .Build();

        const string bucketName = "test-bucket";
        const string objectName = "test-file.txt";
        const string fileContent = "This is a test file for integration testing";

        // Create bucket
        var bucketExists = await minioClient.BucketExistsAsync(new BucketExistsArgs()
            .WithBucket(bucketName));
        
        if (!bucketExists)
        {
            await minioClient.MakeBucketAsync(new MakeBucketArgs()
                .WithBucket(bucketName));
        }

        // Act - Upload file
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)))
        {
            await minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("text/plain"));
        }

        // Act - Download file
        string downloadedContent = "";
        await minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                using var reader = new StreamReader(stream);
                downloadedContent = reader.ReadToEnd();
            }));

        // Assert
        downloadedContent.Should().Be(fileContent);

        // Cleanup
        await minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName));
    }

    [Fact]
    public async Task All_Infrastructure_Services_Should_Be_Running_Concurrently()
    {
        // Arrange & Act
        var tasks = new[]
        {
            TestPostgreSQLConnection(),
            TestMongoDBConnection(),
            TestMinIOConnection()
        };

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeEquivalentTo(true);
    }

    private async Task<bool> TestPostgreSQLConnection()
    {
        try
        {
            await using var connection = new NpgsqlConnection(PostgreSqlConnectionString);
            await connection.OpenAsync();
            return connection.State == System.Data.ConnectionState.Open;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> TestMongoDBConnection()
    {
        try
        {
            var client = new MongoClient(MongoDbConnectionString);
            var database = client.GetDatabase("test_db");
            await database.ListCollectionNamesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> TestMinIOConnection()
    {
        try
        {
            var minioClient = new MinioClient()
                .WithEndpoint(MinioEndpoint)
                .WithCredentials("minioadmin", "minioadmin")
                .WithSSL(false)
                .Build();
            
            await minioClient.ListBucketsAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class TestDocument
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}