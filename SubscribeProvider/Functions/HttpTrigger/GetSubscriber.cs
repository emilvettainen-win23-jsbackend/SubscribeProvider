using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SubscribeProvider.Infrastructure.Helpers.Responses;
using SubscribeProvider.Infrastructure.Services;

namespace SubscribeProvider.Functions.HttpTrigger
{
    public class GetSubscriber(ILogger<GetSubscriber> logger, SubscribeService subscribeService)
    {
        private readonly ILogger<GetSubscriber> _logger = logger;
        private readonly SubscribeService _subscribeService = subscribeService;

        [Function("GetSubscriber")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route ="subscriber/{id}")] string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return new BadRequestResult();

                var subscriberResult = await _subscribeService.GetOneSubsciberAsync(id);
                return subscriberResult.StatusCode switch
                {
                    ResultStatus.OK => new OkObjectResult(subscriberResult.ContentResult),
                    ResultStatus.NOT_FOUND => new NotFoundResult(),
                    _ => new BadRequestResult()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : GetSubscriber.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
