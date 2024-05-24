using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SubscribeProvider.Infrastructure.Data.Repositories;
using SubscribeProvider.Infrastructure.Factories;
using SubscribeProvider.Infrastructure.Helpers.Responses;
using SubscribeProvider.Infrastructure.Models;


namespace SubscribeProvider.Infrastructure.Services;

public class SubscribeService
{
    private readonly ILogger<SubscribeService> _logger;
    private readonly SubscribeRepository _subscribeRepository;
    private readonly ServiceBusClient _serviceBusClient;

    public SubscribeService(SubscribeRepository subscribeRepository, ILogger<SubscribeService> logger, ServiceBusClient serviceBusClient)
    {
        _subscribeRepository = subscribeRepository;
        _logger = logger;
        _serviceBusClient = serviceBusClient;
    }


    public async Task<ResponseResult> GetAllSubscribersAsync()
    {
        var subscribers = await _subscribeRepository.GetAllAsync();
        return subscribers.Any() ? ResponseFactory.Ok(subscribers) : ResponseFactory.NotFound();
    }

    
    public async Task<ResponseResult> UpdateSubscriberAsync(UpdateSubscribeModel model)
    {
        var result = await _subscribeRepository.UpdateSubscriptionAsync(x => x.Id == model.Id, SubscribeFactory.Update(model));
        return result != null ? ResponseFactory.Ok(result) : ResponseFactory.NotFound();
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


    public EmailRequestModel GenerateSubscribeConfirmEmail(string email)
    {
        try
        {
            var confirmEmail = new EmailRequestModel
            {
                To = email,
                Subject = "Confirmation of your subscription",
                HtmlBody = $@"<html lang='en'>
                    <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Newsletter Confirmation</title>
                    </head>
                    <body>
                        <div style='max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff;'>
                            <div style='background-color: #0046ae; color: white; padding: 10px 20px; text-align: center;'>
                                <h1>You've subscribed!</h1>
                            </div>
                            <div style='padding: 20px;'>
                                <p>Hello,</p>
                                <p>Thank you for subscribing: {email} to our newsletter.</p>
                                <p>If you did not subscribe, please contact our support team.</p>
                                <p>Thank you!<br>The Silicon Team</p>
                            </div>
                        </div>
                    </body>
                    </html>",
                PlainText = $"Hello, Thank you for subscribing: {email} to our newsletter. If you did not subscribe, please contact our support team. Thank you! The Silicon Team"
            };
            return confirmEmail;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeService.CreateSubscribeRequestAsync() :: {ex.Message}");
        }
        return null!;
    }


    public async Task<ResponseResult> DeleteAsync(string email)
    {
        try
        {
            if (!await _subscribeRepository.ExistsAsync(x => x.Email == email))
            {
                return ResponseFactory.NotFound();
            }
            var result = await _subscribeRepository.DeleteSubscriptionAsync(x => x.Email == email);
            return result ? ResponseFactory.Ok() : ResponseFactory.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeService.DeleteAsync() :: {ex.Message}");
            return ResponseFactory.ServerError();
        }
    }


    public EmailRequestModel GenerateDeleteConfirmEmail(string email)
    {
        try
        {
            var confirmEmail = new EmailRequestModel
            {
                To = email,
                Subject = "Confirmation of your unsubscription",
                HtmlBody = $@"<html lang='en'>
                    <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Newsletter Unsubscription Confirmation</title>
                    </head>
                    <body>
                        <div style='max-width: 600px; margin: 20px auto; padding: 20px; background-color: #ffffff;'>
                            <div style='background-color: #0046ae; color: white; padding: 10px 20px; text-align: center;'>
                                <h1>You've Unsubscribed</h1>
                            </div>
                            <div style='padding: 20px;'>
                                <p>Hello,</p>
                                <p>We're sorry to see you go. You have successfully unsubscribed: {email}, from our newsletter.</p>
                                <p>If this was a mistake or you change your mind, you can resubscribe at any time by visiting our website.</p>
                                <p>If you did not request this unsubscription, please contact our support team.</p>
                                <p>Thank you for being with us.<br>Best regards,<br>The Silicon Team</p>
                            </div>
                        </div>
                    </body>
                    </html>",
                PlainText = $"Hello, We're sorry to see you go. You have successfully unsubscribed: {email} from our newsletter. If this was a mistake or you change your mind, you can resubscribe at any time by visiting our website. If you did not request this unsubscription, please contact our support team. Thank you for being with us. Best regards, The Silicon Team"
            };
            return confirmEmail;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeService.GenerateDeleteConfirmEmail() :: {ex.Message}");
        }
        return null!;
    }

}
