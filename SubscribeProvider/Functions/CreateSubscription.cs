using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubscribeProvider.Data.Repositories;
using SubscribeProvider.Factories;
using SubscribeProvider.Helpers.Responses;
using SubscribeProvider.Helpers.Validations;
using SubscribeProvider.Models;
using SubscribeProvider.Services;
using System.Text;

namespace SubscribeProvider.Functions
{
    public class CreateSubscription
    {
        private readonly ILogger<CreateSubscription> _logger;
        private readonly SubscribeService _subscribeService;

        public CreateSubscription(ILogger<CreateSubscription> logger, SubscribeService subscribeService)
        {
            _logger = logger;
            _subscribeService = subscribeService;
        }

        [Function("CreateSubscription")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "subscribe")] HttpRequest req)
        {
            try
            {
                var subscribeRequest = await new StreamReader(req.Body).ReadToEndAsync();
                var model = JsonConvert.DeserializeObject<CreateSubscribeModel>(subscribeRequest);
                if (model == null)
                {
                    return new BadRequestResult();
                }
             
                var modelState = CustomValidation.ValidateModel(model);
                if (!modelState.IsValid)
                {
                    return new BadRequestObjectResult(modelState.ValidationResults);
                }

                var result = await _subscribeService.CreateSubscribeRequestAsync(model);
                return result.StatusCode switch
                {
                    ResultStatus.OK => new CreatedResult(),
                    ResultStatus.EXISTS => new ConflictResult(),
                    ResultStatus.ERROR => new BadRequestResult(),
                    _ => new StatusCodeResult(500)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : CreateSubscription.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}