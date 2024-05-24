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
    public class CreateSubscription
    {
        private readonly ILogger<CreateSubscription> _logger;
        private readonly SubscribeService _subscribeService;
        private readonly ServiceBusClient _serviceBusClient;

        public CreateSubscription(ILogger<CreateSubscription> logger, ServiceBusClient serviceBusClient, SubscribeService subscribeService)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _subscribeService = subscribeService;
        }

        [Function("CreateSubscription")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route="create")] HttpRequest req)
        {
            try
            {
                var request = await new StreamReader(req.Body).ReadToEndAsync();
                var model = JsonConvert.DeserializeObject<CreateSubscribeModel>(request);
                if (model != null)
                {
                    var modelState = CustomValidation.ValidateModel(model);
                    if (modelState.IsValid)
                    {
                        var createResult = await _subscribeService.CreateSubscribeRequestAsync(model);
                        if (createResult.StatusCode == ResultStatus.OK) 
                        {
                            var emailRequest = _subscribeService.GenerateSubscribeConfirmEmail(model.Email);
                            if (emailRequest != null)
                            {
                                var sender = _serviceBusClient.CreateSender("email_request");
                                await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailRequest))));
                                return new OkResult();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : CreateSubscription.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
            return new BadRequestResult();
        }
    }
}
