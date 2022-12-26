using Core.Shared.Contracts;
using Core.Shared.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;

namespace Core.Api.Saga
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public State PendingDebit { get; private set; }
        public State AwaitingPartner { get; private set; } // Debitou da conta do cliente
        public State Completed { get; private set; } // Finalizado sucesso
        public State RollingBack { get; private set; } // Fez o rollback
        public State RollbackFailed { get; private set; } // Falha no Rollback
        public State CompletedError { get; private set; } // Finalizado com erro

        public Event<OrderSubmitted> OnOrderSubmitted { get; private set; } // Evento de recepção da solicitação
        public Event<CallPartnerEvent> OnDebited { get; private set; }
        public Event<DebitFailedEvent> OnDebitFailed { get; private set; }
        public Event<OrderConfirmedEvent> OnConfirmed { get; private set; }
        public Event<PartnerFailedEvent> OnPartnerFailed { get; private set; }
        public Event<OrderRolledBackEvent> OnOrderRolledBack { get; private set; }
        public Event<OrderRolledBackFailedEvent> OnOrderRolledFailed { get; private set; }

        public OrderStateMachine(ILogger<OrderStateMachine> logger)
        {
            InstanceState(x => x.CurrentState);
            ConfigureCorrelationIds();

            Initially(
              When(OnOrderSubmitted) // Evento de recepção da solicitação
              .Then(x => logger.LogInformation($"Order submitted"))
              .SendAsync(new Uri("queue:debit-account"), context => context.Init<DebitAccount>(new
              {
                  context.Message.CorrelationId,
                  context.Message.OrderId,
                  context.Message.CustomerId,
                  context.Message.Quantity,
                  context.Message.Amount
              })) // Chama o débito
              .TransitionTo(PendingDebit) // Muda para o status para PendingDebit
              );

            During(PendingDebit, // Enquanto no estado PendingDebit, se...
                When(OnDebited) // Sucesso no débito
                  .Then(x => logger.LogInformation($"Order debited"))
                  .SendAsync(new Uri("queue:call-partner"), context => context.Init<CallPartner>(new
                  {
                      context.Message.CorrelationId,
                      context.Message.OrderId,
                      context.Message.CustomerId,
                      context.Message.Quantity,
                      context.Message.Amount
                  }))  // Chama o parceiro
                  .TransitionTo(AwaitingPartner), // Muda para o status para AwaitingPartner
                When(OnDebitFailed) // Falha do débito
                  .Then(x => logger.LogInformation($"Debit failed"))
                  .TransitionTo(CompletedError) // Finaliza com erro
                );

            During(AwaitingPartner, // Enquanto no estado AwaitingPartner, se...
                  When(OnConfirmed) // Sucesso com o parceiro
                  .Then(x => logger.LogInformation($"Order completed"))
                  .Finalize(),
                  When(OnPartnerFailed) // Erro com o parceiro
                  .Then(x => logger.LogInformation($"Partner failed"))
                  .SendAsync(new Uri("queue:rollback"), context => context.Init<RollbackDebit>(new
                  {
                      context.Message.CorrelationId,
                      context.Message.OrderId,
                      context.Message.CustomerId,
                      context.Message.Quantity,
                      context.Message.Amount
                  })) // Faz rollback do débito
                  .TransitionTo(RollingBack) // Muda para o status para RollingBack
                );

            During(RollingBack, // Enquanto no estado RollingBack, se...
                When(OnOrderRolledBack) // Sucesso com o rollback
                  .Then(x => logger.LogInformation($"Rollback has been succeded"))
                  .TransitionTo(CompletedError), // Finaliza com erro
                When(OnOrderRolledFailed)
                  .Then(x => logger.LogInformation($"Rollback failed"))
                  .TransitionTo(RollbackFailed)
                );

            During(RollbackFailed, // Enquanto no estado RollbackFailed, se...
                When(OnPartnerFailed)
                   .Then(x => logger.LogInformation($"Partner failed"))
                   .SendAsync(new Uri("queue:partner-failed"), context => context.Init<PartnerFailedEvent>(new
                   {
                       context.Message.CorrelationId,
                       context.Message.OrderId,
                       context.Message.CustomerId,
                       context.Message.Quantity,
                       context.Message.Amount
                   })) // Faz rollback do débito
                  .TransitionTo(RollingBack)
                );
        }

        private void ConfigureCorrelationIds()
        {
            Event(() => OnOrderSubmitted, x =>
            {
                x.CorrelateById(x => x.Message.CorrelationId);

                x.InsertOnInitial = true;
                x.SetSagaFactory(c => new OrderState
                {
                    CorrelationId = c.Message.CorrelationId,
                    OrderId = c.Message.OrderId,
                    Amount = c.Message.Amount,
                    Quantity = c.Message.Quantity,
                    CustomerId = c.Message.CustomerId,
                });
            });
            Event(() => OnDebited, x => x.CorrelateById(x => x.Message.CorrelationId));
            Event(() => OnDebitFailed, x => x.CorrelateById(x => x.Message.CorrelationId));
            Event(() => OnConfirmed, x => x.CorrelateById(x => x.Message.CorrelationId));
            Event(() => OnPartnerFailed, x => x.CorrelateById(x => x.Message.CorrelationId));
            Event(() => OnOrderRolledBack, x => x.CorrelateById(x => x.Message.CorrelationId));
            Event(() => OnOrderRolledFailed, x => x.CorrelateById(x => x.Message.CorrelationId));
        }
    }
}
