using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SubscribeProvider.Infrastructure.Helpers.Responses;
using SubscribeProvider.Infrastructure.Helpers.Validations;
using SubscribeProvider.Infrastructure.Models;
using SubscribeProvider.Infrastructure.Services;
using System.Text;

namespace SubscribeProvider.Functions.HttpTrigger
{
    public class CreateSubscription(ILogger<CreateSubscription> logger, ServiceBusClient serviceBusClient, SubscribeService subscribeService)
    {
        private readonly ILogger<CreateSubscription> _logger = logger;
        private readonly SubscribeService _subscribeService = subscribeService;
        private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

        [Function("CreateSubscription")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route="create")] HttpRequest req)
        {
            try
            {
                var model = await ValidateRequestAsync(req);
                if (model == null)
                    return new BadRequestResult();

                var createResult = await _subscribeService.CreateSubscribeRequestAsync(model);
                return await HandleCreateResultAsync(createResult, model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : CreateSubscription.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        private async Task<CreateSubscribeModel> ValidateRequestAsync(HttpRequest req)
        {
            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync();
            var model = JsonConvert.DeserializeObject<CreateSubscribeModel>(requestBody);
            if (model == null)
            {
                return null!;
            }
            var modelState = CustomValidation.ValidateModel(model);
            return modelState.IsValid ? model : null!;
        }


        private async Task<IActionResult> HandleCreateResultAsync(ResponseResult result, CreateSubscribeModel model)
        {
            return result.StatusCode switch
            {
                ResultStatus.OK => await HandleSuccessfulCreateAsync(model.Email),
                ResultStatus.EXISTS => new ConflictResult(),
                _ => new BadRequestResult(),
            };
        }


        private async Task<IActionResult> HandleSuccessfulCreateAsync(string email)
        {
            var emailRequest = _subscribeService.GenerateSubscribeConfirmEmail(email);
            if (emailRequest == null) 
            {
                return new BadRequestResult();
            }
            var sender = _serviceBusClient.CreateSender("email_request");
            await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailRequest))));
            return new OkResult();
        }
    }
}
