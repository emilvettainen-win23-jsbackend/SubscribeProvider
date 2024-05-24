using SubscribeProvider.Infrastructure.Data.Entities;
using SubscribeProvider.Infrastructure.Models;


namespace SubscribeProvider.Infrastructure.Factories;

public class SubscribeFactory
{
    public static SubscribeEntity ToEntity(CreateSubscribeModel model)
    {
        try
        {
            return new SubscribeEntity
            {
                Email = model.Email,
                DailyNewsletter = model.DailyNewsletter,
                AdvertisingUpdates = model.AdvertisingUpdates,
                WeenInReview = model.WeenInReview,
                EventUpdates = model.EventUpdates,
                StartupsWeekly = model.StartupsWeekly,
                Podcasts = model.Podcasts,
            };
        }
        catch (Exception)
        {

            return null!;
        }
    }
}
