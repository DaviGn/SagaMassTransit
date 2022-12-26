using Core.Api.DB;
using Core.Shared.Events;
using Core.Api.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Core.Shared.Contracts;

namespace Core.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Order order)
        {
            Guid correlationID = Guid.NewGuid();
            await _publishEndpoint.Publish<OrderSubmitted>(new
            {
                CorrelationID = correlationID,
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                Quantity = order.Quantity,
                Amount = order.Amount,
            });
            return Ok();
        }
    }
}
