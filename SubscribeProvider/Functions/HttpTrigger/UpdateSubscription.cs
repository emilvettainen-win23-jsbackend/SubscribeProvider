using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubscribeProvider.Infrastructure.Helpers.Responses;
using SubscribeProvider.Infrastructure.Helpers.Validations;
using SubscribeProvider.Infrastructure.Models;
using SubscribeProvider.Infrastructure.Services;

namespace SubscribeProvider.Functions.HttpTrigger
{
    public class UpdateSubscription(ILogger<UpdateSubscription> logger, SubscribeService subscribeService)
    {
        private readonly ILogger<UpdateSubscription> _logger = logger;
        private readonly SubscribeService _subscribeService = subscribeService;

        [Function("UpdateSubscription")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                var model = await ValidateRequestAsync(req);
                if (model == null)
                    return new BadRequestResult();

                var updateResult = await _subscribeService.UpdateSubscriberAsync(model);
                return updateResult.StatusCode switch
                {
                    ResultStatus.OK => new OkObjectResult(updateResult.ContentResult),
                    ResultStatus.NOT_FOUND => new NotFoundResult(),
                    _ => new BadRequestResult()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : CreateSubscription.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        private async Task<UpdateSubscribeModel> ValidateRequestAsync(HttpRequest req)
        {
            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync();
            var model = JsonConvert.DeserializeObject<UpdateSubscribeModel>(requestBody);
            if (model == null)
            {
                return null!;
            }
            var modelState = CustomValidation.ValidateModel(model);
            return modelState.IsValid ? model : null!;
        }
    }
}