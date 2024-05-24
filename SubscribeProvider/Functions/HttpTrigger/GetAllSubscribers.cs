using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SubscribeProvider.Infrastructure.Helpers.Responses;
using SubscribeProvider.Infrastructure.Services;

namespace SubscribeProvider.Functions.HttpTrigger
{
    public class GetAllSubscribers
    {
        private readonly ILogger<GetAllSubscribers> _logger;
        private readonly SubscribeService _subscribeService;

        public GetAllSubscribers(ILogger<GetAllSubscribers> logger, SubscribeService subscribeService)
        {
            _logger = logger;
            _subscribeService = subscribeService;
        }

        [Function("GetAllSubscribers")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route ="subscribers")] HttpRequest req)
        {
            try
            {
                var result = await _subscribeService.GetAllSubscribersAsync();
                return result.StatusCode switch
                {
                    ResultStatus.OK => new OkObjectResult(result.ContentResult),
                    ResultStatus.NOT_FOUND => new NotFoundResult(),
                    _ => new BadRequestResult()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : GetAllSubscribers.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
