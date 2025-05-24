using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Application.Models;
using SearchService.Application.Settings;

namespace SearchService.Persistence.DatabaseContext;

public interface ISearchServiceDatabaseContext
{
    Task InitializeAsync();
    
    Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> filter) where T : Entity;
    
    Task CreateAsync<T>(T entity) where T : Entity;
    
    Task SaveAsync<T>(T entity) where T : Entity;
}

public class SearchServiceDatabaseContext(SearchServiceConfiguration configuration) : ISearchServiceDatabaseContext
{
    public async Task InitializeAsync()
    {
        await DB.InitAsync(configuration.MongoDbSettings.DatabaseName,
            MongoClientSettings.FromConnectionString(
                configuration.MongoDbSettings.ConnectionString));

        await DB.Index<Item>()
            .Key(x => x.Name, KeyType.Text)
            .Key(x => x.Type, KeyType.Text)
            .Key(x => x.AccountId, KeyType.Text)
            .CreateAsync();
    }
    
    public async Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> filter) where T : Entity
    {
        return await DB.Find<T>()
            .Match(filter)
            .ExecuteAsync();
    }

    public async Task CreateAsync<T>(T entity) where T : Entity
    {
        await entity.SaveAsync();
    }
    
    public async Task SaveAsync<T>(T entity) where T : Entity
    {
        await entity.SaveAsync();
    }
}
