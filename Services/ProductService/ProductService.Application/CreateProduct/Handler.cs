using ECommerce.Contracts.Events;
using ECommerce.Messaging.Abstractions;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using static Confluent.Kafka.ConfigPropertyNames;

namespace ProductService.Application.CreateProducts
{
    public class CreateProductHandler
     : IRequestHandler<CreateProductCommand, Guid> // for MediatR
    {
        //hander -> dbcontext
        private readonly IProductDbContext _context;
        private readonly IEventBus _bus;
        //private readonly ITopicProducer<ProductCreatedEvent> _producer;

        public CreateProductHandler(IProductDbContext context,
            //ITopicProducer<ProductCreatedEvent> producer)
            IEventBus bus)
        {
            _context = context;
           // _producer = producer;
            _bus = bus;
        }
       
        public async Task<Guid> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.Category,
                request.Status);

            _context.Products.Add(product);

            await _context.SaveChangesAsync(cancellationToken);

           //  publish to kafka using IEventBus
            var evt = new ProductCreatedEvent
            {
                ProductId = product.Id,
                Name = request.Name,
                StockQuantity = request.StockQuantity,
                Price = request.Price,
                Description = request.Description,
                Category = request.Category,
                Status = request.Status
            };

            await _bus.PublishAsync("product.created", evt);

            //publish to masstransit
            /*await _producer.Produce(new ProductCreatedEvent
            {
                ProductId = product.Id,
                Name = product.Name,
                StockQuantity = request.StockQuantity
            });*/

            return product.Id;

        }
        /*
        // handler -> repository-> dbcontext
        private readonly IProductRepository _repository;

        public Handler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(
            Command request,
            CancellationToken cancellationToken)// Cancellationtoken for MediatR
        {
            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.StockQuantity);

            await _repository.AddAsync(product);

            return product.Id;
        }
        */
        /*
        //Manual validation without a pipeline [validator.cs] when using MediatR  
        private readonly IValidator<Command> _validator;

        public Handler(IValidator<Command> validator)
        {
            _validator = validator;
        }

        public async Task<Guid> Handle(
            Command request,
            CancellationToken cancellationToken)// Cancellationtoken for MediatR
        {

            var result = await _validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }

            return new Guid();
        }
        */
    }
}
