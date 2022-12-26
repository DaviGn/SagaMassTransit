using Core.Api.DB;
using Core.Api.Saga;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IConfiguration configuration = builder.Configuration;
string sqlConnection = configuration["SQLConnection"];

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
    //.InMemoryRepository();
    .EntityFrameworkRepository(r =>
    {
        r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

        r.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
        {
            builder.UseSqlServer(sqlConnection);
            builder.EnableSensitiveDataLogging(false);
        });
    });

    x.UsingRabbitMq((context, configurator) =>
    {
        configurator.ReceiveEndpoint("order-saga", e =>
        {
            e.UseMessageRetry(r => r.Immediate(50));
            e.UseInMemoryOutbox();
            e.StateMachineSaga<OrderState>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
