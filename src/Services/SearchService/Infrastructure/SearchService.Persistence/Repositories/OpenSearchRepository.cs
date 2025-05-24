using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using SearchService.Application.Contracts.Persistence;
using SearchService.Application.Features.SearchResult;
using SearchService.Application.Models;
using Services.Core.Models;

namespace SearchService.Persistence.Repositories;

public class OpenSearchRepository : ISearchRepository
{
    private readonly IOpenSearchClient _client;
    private readonly ILogger<OpenSearchRepository> _logger;
    private const string INDEX_NAME = "marketplace";

    public OpenSearchRepository(IOpenSearchClient client, ILogger<OpenSearchRepository> logger)
    {
        _client = client;
        _logger = logger;
        
        CreateIndexIfNotExists().GetAwaiter().GetResult();
    }
    
    private async Task CreateIndexIfNotExists()
    {
        var indexExists = await _client.Indices.ExistsAsync(INDEX_NAME);
        
        if (!indexExists.Exists)
        {
            var createIndexResponse = await _client.Indices.CreateAsync(INDEX_NAME, c => c
                .Settings(s => s
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Standard("standard_analyzer", sa => sa
                                .StopWords("_english_")
                            )
                        )
                    )
                )
                .Map<Item>(m => m
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Id)
                            .Analyzer("standard_analyzer")
                        )
                        .Text(t => t
                            .Name(n => n.Name)
                            .Analyzer("standard_analyzer")
                            .Fields(f => f
                                .Keyword(k => k
                                    .Name("keyword")
                                    .IgnoreAbove(256)
                                )
                            )
                        )
                        .Text(t => t
                            .Name(n => n.Description)
                            .Analyzer("standard_analyzer")
                        )
                        .Keyword(k => k
                            .Name(n => n.Type)
                        )
                        .Keyword(k => k
                            .Name(n => n.AccountId)
                        )
                        .Keyword(k => k
                            .Name(n => n.ResourceUrl)
                        )
                        .Number(n => n
                            .Name(n => n.Status)
                            .Type(NumberType.Integer)
                        )
                        .Date(d => d
                            .Name(n => n.CreatedAt)
                        )
                        .Date(d => d
                            .Name(n => n.UpdatedAt)
                        )
                        .Object<Dictionary<string, string>>(o => o
                            .Name(n => n.Metadata)
                            .Properties(mp => mp
                                .Text(t => t
                                    .Name("*")
                                    .Fields(f => f
                                        .Keyword(k => k
                                            .Name("keyword")
                                            .IgnoreAbove(256)
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            );
            
            if (!createIndexResponse.IsValid)
            {
                _logger.LogError("Failed to create index: {Error}", createIndexResponse.DebugInformation);
                throw new Exception($"Failed to create index: {createIndexResponse.DebugInformation}");
            }
        }
    }

    public async Task<BasePagedResult<SearchResultDto>> SearchAsync(PagedRequestQuery pagedRequestQuery)
    {
        var from = (pagedRequestQuery.PageNumber - 1) * pagedRequestQuery.PageSize;
        
        var searchResponse = await _client.SearchAsync<Item>(s => s
            .Index(INDEX_NAME)
            .From(from)
            .Size(pagedRequestQuery.PageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(f => f
                        .Field(p => p.Name, 2.0)
                        .Field(p => p.Description)
                        .Field("metadata.*")
                    )
                    .Query(pagedRequestQuery.SearchBy ?? string.Empty)
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(Fuzziness.Auto)
                )
            )
            .Sort(sort => sort
                .Field(f => f
                    .Field(i => i.CreatedAt)
                    .Order(SortOrder.Descending)
                )
            )
        );
        
        if (!searchResponse.IsValid)
        {
            _logger.LogError("Failed to execute search: {Error}", searchResponse.DebugInformation);
            throw new Exception($"Failed to execute search: {searchResponse.DebugInformation}");
        }
        
        var totalCount = searchResponse.Total;
        
        var results = searchResponse.Documents.Select(item => new SearchResultDto
        {
            Identifier = item.Identifier,
            Name = item.Name,
            Type = item.Type,
            AccountId = item.AccountId,
            ResourceUrl = item.ResourceUrl,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        }).ToList();
        
        return new BasePagedResult<SearchResultDto>
        {
            Data = results,
            TotalRecords = (int)totalCount,
            PageNumber = pagedRequestQuery.PageNumber,
            PageSize = pagedRequestQuery.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pagedRequestQuery.PageSize)
        };
    }

    public async Task SaveAsync(Item item)
    {
        var indexResponse = await _client.IndexAsync(item, i => i
            .Index(INDEX_NAME)
            .Id(item.Id)
            .Refresh(OpenSearch.Net.Refresh.WaitFor)
        );
        
        if (!indexResponse.IsValid)
        {
            _logger.LogError("Failed to index document: {Error}", indexResponse.DebugInformation);
            throw new Exception($"Failed to index document: {indexResponse.DebugInformation}");
        }
    }

    public async Task DeleteAsync(string id)
    {
        var deleteResponse = await _client.DeleteAsync(new DeleteRequest(INDEX_NAME, id)
        {
            Refresh = OpenSearch.Net.Refresh.WaitFor
        });
        
        if (!deleteResponse.IsValid && deleteResponse.Result != Result.NotFound)
        {
            _logger.LogError("Failed to delete document: {Error}", deleteResponse.DebugInformation);
            throw new Exception($"Failed to delete document: {deleteResponse.DebugInformation}");
        }
    }
}