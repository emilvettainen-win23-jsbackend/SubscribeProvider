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
    public class DeleteSubsciption(ILogger<DeleteSubsciption> logger, SubscribeService subscribeService, ServiceBusClient serviceBusClient)
    {
        private readonly ILogger<DeleteSubsciption> _logger = logger;
        private readonly SubscribeService _subscribeService = subscribeService;
        private readonly ServiceBusClient _serviceBusClient = serviceBusClient;

        [Function("DeleteSubsciption")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route ="delete")] HttpRequest req)
        {
            try
            {
                var model = await ValidateRequestAsync(req);
                if (model == null)
                    return new BadRequestResult();

                var deleteResult = await _subscribeService.DeleteAsync(model.Email);
                return await HandleDeleteResultAsync(deleteResult, model);
            }
            catch (Exception ex)
            {
               _logger.LogError($"ERROR : DeleteSubsciption.Run() :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        private async Task<DeleteSubsciptionModel> ValidateRequestAsync(HttpRequest req)
        {
            using var reader = new StreamReader(req.Body);
            var requestBody = await reader.ReadToEndAsync();
            var model = JsonConvert.DeserializeObject<DeleteSubsciptionModel>(requestBody);
            if (model == null)
            {
                return null!;
            }
            var modelState = CustomValidation.ValidateModel(model);
            return modelState.IsValid ? model : null!;
        }


        private async Task<IActionResult> HandleDeleteResultAsync(ResponseResult result, DeleteSubsciptionModel model)
        {
            return result.StatusCode switch
            {
                ResultStatus.OK => await HandleSuccessfulDeleteAsync(model.Email),
                ResultStatus.NOT_FOUND => new NotFoundResult(),
                _ => new BadRequestResult(),
            };
        }


        private async Task<IActionResult> HandleSuccessfulDeleteAsync(string email)
        {
            var emailRequest = _subscribeService.GenerateDeleteConfirmEmail(email);
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