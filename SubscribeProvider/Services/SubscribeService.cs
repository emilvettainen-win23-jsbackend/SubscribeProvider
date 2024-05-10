using Microsoft.Extensions.Logging;
using SubscribeProvider.Data.Repositories;
using SubscribeProvider.Factories;
using SubscribeProvider.Helpers.Responses;
using SubscribeProvider.Models;

namespace SubscribeProvider.Services;

public class SubscribeService
{
    private readonly ILogger<SubscribeService> _logger;
    private readonly SubscribeRepository _subscribeRepository;

    public SubscribeService(SubscribeRepository subscribeRepository, ILogger<SubscribeService> logger)
    {
        _subscribeRepository = subscribeRepository;
        _logger = logger;
    }

    public async Task<ResponseResult> CreateSubscribeRequestAsync(CreateSubscribeModel model)
    {
        try
        {
            if (await _subscribeRepository.ExistsAsync(x => x.Email == model.Email))
            {
                return ResponseFactory.Exists();
            }
            var createRequest = await _subscribeRepository.CreateAsync(SubscribeFactory.ToEntity(model));
            return createRequest != null ? ResponseFactory.Ok() : ResponseFactory.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeService.CreateSubscribeRequestAsync() :: {ex.Message}");
            return ResponseFactory.ServerError();
        }
    }
}
