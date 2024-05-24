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
    public class DeleteSubsciption
    {
        private readonly ILogger<DeleteSubsciption> _logger;
        private readonly SubscribeService _subscribeService;
        private readonly ServiceBusClient _serviceBusClient;

        public DeleteSubsciption(ILogger<DeleteSubsciption> logger, SubscribeService subscribeService, ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _subscribeService = subscribeService;
            _serviceBusClient = serviceBusClient;
        }

        [Function("DeleteSubsciption")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route ="delete")] HttpRequest req)
        {
            try
            {
                var request = await new StreamReader(req.Body).ReadToEndAsync();
                var model = JsonConvert.DeserializeObject<DeleteSubsciptionModel>(request);
                if (model != null)
                {
                    var modelState = CustomValidation.ValidateModel(model);
                    if (modelState.IsValid)
                    {
                        var deleteResult = await _subscribeService.DeleteAsync(model.Email);
                        switch (deleteResult.StatusCode)
                        {
                            case ResultStatus.OK:
                                var emailRequest = _subscribeService.GenerateDeleteConfirmEmail(model.Email);
                                if (emailRequest != null)
                                {
                                    var sender = _serviceBusClient.CreateSender("email_request");
                                    await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailRequest))));
                                    return new OkResult();
                                }
                                break;

                            case ResultStatus.NOT_FOUND:
                                return new NotFoundResult();

                            default:
                                return new StatusCodeResult(500);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
               _logger.LogError($"ERROR : DeleteSubsciption.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
            return new BadRequestResult();
        }
    }
}