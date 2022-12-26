using Core.Accounts;
using Core.Shared.Contracts;
using Core.Shared.Events;
using MassTransit;

namespace Core.Consumers
{
    public class RollbackConsumer : IConsumer<RollbackDebit>
    {
        private readonly ILogger<RollbackConsumer> _logger;

        public RollbackConsumer(ILogger<RollbackConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RollbackDebit> context)
        {
            try
            {
                _logger.LogInformation($"Rolling back");

                if (context.Message.OrderId[0] == 'E')
                    throw new Exception("Teste");

                var accountService = new AccountService();
                await accountService.Rollback(context.Message.CustomerId);

                _logger.LogInformation($"Debit has been rolled back");
                _logger.LogInformation($"Publishing OnOrderRolledBack");
                await context.Publish<OrderRolledBackEvent>(context.Message);
            }
            catch (Exception)
            {
                _logger.LogInformation($"Publishing OnOrderRolledBackFailed");
                await context.Publish<OrderRolledBackFailedEvent>(context.Message);
            }
        }
    }
}
