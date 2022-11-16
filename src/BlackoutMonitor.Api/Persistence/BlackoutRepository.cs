using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlackoutMonitor.Api.Configuration;
using BlackoutMonitor.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;

namespace BlackoutMonitor.Api.Persistence;

public class BlackoutRepository
{
    private readonly CosmosClient _client;
    private readonly string _dbName;

    public BlackoutRepository(CosmosClient client, IOptions<BlackoutMonitorDbOptions> options)
	{
        _client = client;
        _dbName = options.Value.DbName;
    }

    public async Task<IList<BlackoutDetail>> GetBlackoutsAsync(string beeperId = null)
    {
        var container = GetContainer();

        IQueryable<BlackoutDetail> query = container.GetItemLinqQueryable<BlackoutDetail>();

        if (string.IsNullOrEmpty(beeperId) == false)
        {
            query = query.Where(x => x.BeeperId == beeperId);
        }

        using var feed = query.ToFeedIterator();

        var result = new List<BlackoutDetail>();
        while (feed.HasMoreResults)
        {
            var items = await feed.ReadNextAsync();
            result.AddRange(items);
        }

        return result;
    }

    public async Task<BlackoutDetail> GetLastBlackoutAsync(string beeperId)
    {
        var container = GetContainer();

        using var feed = container
            .GetItemLinqQueryable<BlackoutDetail>()
            .Where(x => x.BeeperId == beeperId)
            .OrderByDescending(x => x.StartTimestamp)
            .Take(1)
            .ToFeedIterator();

        var items = await feed.ReadNextAsync();
        return items.FirstOrDefault();
    }

    public async Task<string> CreateBlackoutAsync(string beeperId, DateTime startTimestamp)
	{
        var container = GetContainer();

        var id = CreateBlackoutId(startTimestamp);
        var blackout = new BlackoutDetail
        {
            Id = id,
            BeeperId = beeperId,
            StartTimestamp = startTimestamp,
            FinishTimestamp = null,
        };

        await container.CreateItemAsync(blackout, new PartitionKey(beeperId));

        return id;
    }

    public async Task UpdateBlackoutAsync(string beeperId, string id, DateTime finishTimestamp)
    {
        var container = GetContainer();

        var patchOperations = new PatchOperation[]
        {
            PatchOperation.Set("/finishTimestamp", finishTimestamp),
        };

        await container.PatchItemAsync<BlackoutDetail>(id, new PartitionKey(beeperId), patchOperations);
    }

    private Container GetContainer() 
    {
        var db = _client.GetDatabase(_dbName);
        var container = db.GetContainer("blackouts");
        return container;
    }

    private string CreateBlackoutId(DateTime startTime)
    {
        return startTime.ToString("yyyyMMddHHmm");
    }
}
