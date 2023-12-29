using Microsoft.AspNetCore.Mvc;
using DomainDefinitions.Data;
using DomainDefinitions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using DomainDefinitions.AuxStaticCode;
using DomainDefinitions.Interfaces;
using DomainDefinitions.Enums;

namespace WebApplication1.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Subscriptions
    {
        private readonly AppDbContext _appDbContext;

        public Subscriptions(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var subscriptions = await _appDbContext.subscriptions.ToListAsync();

            return new OkObjectResult(subscriptions);
        }

        [HttpPut]
        public async Task<IActionResult> AddSubscription(Subscription subscription)
        {
            var existingSubscription = QuerySubscriptionBySourceNameAndPair(subscription.SourceName, subscription.CryptoPair);

            if (existingSubscription != null)
            {
                return new BadRequestObjectResult($"Error duplicate subscription. There exists an active subscription of " +
                    $"SourceName:[{subscription.SourceName}] and CryptoPair:[{subscription.CryptoPair}] with ID:[{subscription.ID}] and Frequency [{subscription.FrequencyMS}]. " +
                    $"Please edit or remove existing subscription.");
            }

            ObjectResult checkValidResult = await CheckExistingSubscription(subscription);

            if (checkValidResult.StatusCode != 200)
            {
                return checkValidResult;
            }

            ICryptoDataSource cryptoDataSource = await AuxStaticCode.GetDataSourceByName(subscription.SourceName);

            if (!cryptoDataSource.listValidCryptoPairs.Contains(subscription.CryptoPair))
            {
                return new BadRequestObjectResult($"Error, Param: CryptoPair - [{subscription.CryptoPair}] is not a valid CryptoPair. Please request the valid params and choose one of them");
            }
                
            _appDbContext.subscriptions.Add(subscription);
            AuxStaticCode.RegisteredIDs.Add(subscription.ID);
            await _appDbContext.SaveChangesAsync();

            await cryptoDataSource.SubscribeCryptoData(subscription.CryptoPair, (EFrequency)subscription.FrequencyMS, AuxStaticCode.cryptoDataSourceDBSubscriber);

            return new OkObjectResult("Successfully added subscription.");
        }

        [HttpPost]
        public async Task<IActionResult> EditSubscription(Subscription subscription)
        {
            ObjectResult checkValidResult = await CheckExistingSubscription(subscription);

            if (checkValidResult.StatusCode != 200)
            {
                return checkValidResult;
            }


            Subscription oldSubscription = QuerySubscriptionByID(subscription.ID);

            if (oldSubscription == null)
            {
                return new NotFoundObjectResult($"Error, No subscription ID:[{subscription.ID}] SourceName:[{subscription.SourceName}] and CryptoPair:[{subscription.CryptoPair}] was found." +
                    $"Please check active subscriptions to edit a valid active subscriptions.");
            }

            oldSubscription.FrequencyMS = subscription.FrequencyMS;
            _appDbContext.subscriptions.Update(oldSubscription);

            await _appDbContext.SaveChangesAsync();

            ICryptoDataSource cryptoDataSource = await AuxStaticCode.GetDataSourceByName(subscription.SourceName);
            await cryptoDataSource.UpdateSubscriptionCryptoData(subscription.CryptoPair, (EFrequency)subscription.FrequencyMS, AuxStaticCode.cryptoDataSourceDBSubscriber);

            return new OkObjectResult("Successfully edited subscription.");
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveSubscription(RemoveSubscription removeSubscription)
        {
            Subscription subscription = QuerySubscriptionByID(removeSubscription.ID);

            if (subscription == null)
            {
                return new NotFoundObjectResult($"Error, No subscription with ID:[{subscription.ID}] was found." +
                    $"Please check active subscriptions to remove a valid active subscriptions.");
            }

            _appDbContext.subscriptions.Remove(subscription);
            await _appDbContext.SaveChangesAsync();

            AuxStaticCode.RegisteredIDs.Remove(subscription.ID);
            ICryptoDataSource cryptoDataSource = await AuxStaticCode.GetDataSourceByName(subscription.SourceName);
            await cryptoDataSource.UnsubscribeCryptoData(subscription.CryptoPair, (EFrequency)subscription.FrequencyMS, AuxStaticCode.cryptoDataSourceDBSubscriber);
            return new OkObjectResult("Successfully removed subscription.");
        }

        private async Task<ObjectResult> CheckExistingSubscription(Subscription subscription)
        {
            if (AuxStaticCode.RegisteredIDs.Contains(subscription.ID))
            {
                return new BadRequestObjectResult($"Error, Param: ID[{subscription.ID}] already exists. Please check existing subscriptions and set a new ID. ID Suggestion: [{AuxStaticCode.RegisteredIDs.Max() + 1}]");
            }

            if (!AuxStaticCode.validExchanges.Contains(subscription.SourceName))
            {
                return new BadRequestObjectResult($"Error, Param: Exchange[{subscription.SourceName}] is not a valid exchange. Please request the valid params and choose one of them");
            }

            if (!Enum.IsDefined(typeof(EFrequency), subscription.FrequencyMS))
            {
                return new BadRequestObjectResult($"Error, Param: FrequencyMS - [{subscription.FrequencyMS}] is not a valid frequency. Please request the valid params and choose one of them");
            }

            return new OkObjectResult("");
        }

        private Subscription QuerySubscriptionByID(int ID)
        {
            var oldSubscriptionResult = from sub in _appDbContext.subscriptions
                                        where sub.ID.Equals(ID)
                                        select sub;

            return oldSubscriptionResult.FirstOrDefault();
        }

        private Subscription QuerySubscriptionBySourceNameAndPair(string sourceName, string cryptoPair)
        {
            var oldSubscriptionResult = from sub in _appDbContext.subscriptions
                                        where (sub.SourceName.Equals(sourceName) && sub.CryptoPair.Equals(cryptoPair))
                                        select sub;

            return oldSubscriptionResult.FirstOrDefault();
        }

    }
}
