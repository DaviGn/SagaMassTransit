using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Api.Saga;

namespace Core.Api.DB
{
    public class OrderStateMap : SagaClassMap<OrderState>
    {
        protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
        {
            entity.HasKey(x => x.CorrelationId);

            entity.Property(x => x.OrderId);
            entity.Property(x => x.CustomerId);
            entity.Property(x => x.Amount);
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.RowVersion).IsRowVersion();
        }
    }
}
