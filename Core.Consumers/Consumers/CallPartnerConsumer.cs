using Core.Accounts;
using Core.Shared.Events;
using Core.External;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Core.Shared.Contracts;

namespace Core.Consumers
{
    public class CallPartnerConsumer : IConsumer<CallPartner>
    {
        private readonly ILogger<CallPartnerConsumer> _logger;

        public CallPartnerConsumer(ILogger<CallPartnerConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CallPartner> context)
        {
            try
            {
                _logger.LogInformation($"Calling partner");

                if (context.Message.OrderId[0] != '1')
                    throw new Exception("Teste");

                var partnerApi = new PartnerApi();
                await partnerApi.SendRequest(context.Message.CustomerId, context.Message.Amount);

                _logger.LogInformation($"Partner has been succeeded");
                _logger.LogInformation($"Publishing OnConfirmed");
                await context.Publish<OrderConfirmedEvent>(context.Message);
            }
            catch (Exception)
            {
                _logger.LogInformation($"Publishing OnPartnerFailed");
                await context.Publish<PartnerFailedEvent>(context.Message);
            }
        }
    }
}
