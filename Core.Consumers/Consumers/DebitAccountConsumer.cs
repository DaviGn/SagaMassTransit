using Core.Accounts;
using Core.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Core.Consumers
{
    public class DebitAccountConsumer : IConsumer<DebitAccount>
    {
        private readonly ILogger<DebitAccountConsumer> _logger;

        public DebitAccountConsumer(ILogger<DebitAccountConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<DebitAccount> context)
        {
            try
            {
                _logger.LogInformation($"Debiting account");
                var accountService = new AccountService();
                await accountService.DebitAsync(context.Message.CustomerId, context.Message.Amount);

                _logger.LogInformation($"Account has been debited");
                _logger.LogInformation($"Publishing OnDebited");
                await context.Publish<CallPartnerEvent>(context.Message);
            }
            catch (Exception)
            {
                _logger.LogInformation($"Publishing OnDebitFailed");
                await context.Publish<DebitFailedEvent>(context.Message);
            }
        }
    }
}
