using Confluent.Kafka;
using ECommerce.Contracts.Commands;
using ECommerce.Contracts.Events;
using ECommerce.Messaging.Abstractions;
using ECommerce.Messaging.Kafka;
using MassTransit;
using MassTransit;
using MassTransit.Transports;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using OrderService.Application;
using OrderService.Application.CreateOrder;
using OrderService.Application.Sagas;
using OrderService.Application.Sagas.Activities;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Messaging.Consumers;
using OrderService.Infrastructure.Messaging.Kafka.Consumers;
using OrderService.Infrastructure.Messaging.Kafka.Producers;
using OrderService.Infrastructure.Persistence;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();


builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));
builder.Services.AddScoped<IEventBus, KafkaEventBus>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration["Redis:ConnectionString"];

    return ConnectionMultiplexer.Connect(configuration);
});

//implementing producer and consumer in same service 
//builder.Services.AddHostedService<KafkaTopicInitializer>(); // setup Kafka
//builder.Services.AddSingleton<OrderCreatedProducer>();
//builder.Services.AddHostedService<OrderProjectionConsumer>();



builder.Services.AddDbContext<OrderDbContext>(opt =>
{
opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<IOrderDbContext>(
provider => provider.GetRequiredService<OrderDbContext>());

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ConfirmOrderConsumer>();
    x.AddConsumer<CancelOrderConsumer>();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<OrderDbContext>();
            r.UseSqlServer();
        })
      .Endpoint(e =>
       {
           e.Name = "order-saga";
       });

    //  REQUIRED base bus
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    // Kafka Rider
    x.AddRider(r =>
    {
        r.AddProducer<OrderCreatedEvent>("order.created");
        r.AddProducer<ReserveInventoryCommand>("reserve.inventory");

        r.AddConsumer<InventoryReservedConsumer>();
        r.AddConsumer<InventoryRejectedConsumer>();
        r.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");

            k.TopicEndpoint<InventoryReservedEvent>(
                "inventory-reserved-event",
                "order.service",
                e =>
                {
                    e.ConfigureConsumer<InventoryReservedConsumer>(context);
                    e.AutoOffsetReset = AutoOffsetReset.Earliest;
                   // e.UseMessageRetry(r => r.Interval(2, TimeSpan.FromSeconds(5)));
                });

            k.TopicEndpoint<InventoryRejectedEvent>(
               "inventory-rejected-event",
               "order.service.rejected",
               e =>
               {
                   e.ConfigureConsumer<InventoryRejectedConsumer>(context);
                   e.AutoOffsetReset = AutoOffsetReset.Earliest;
               });
           
            /*k.TopicEndpoint<OrderCreatedEvent>(
             "order.created",
             "inventory.service",
             e =>
             {
                 e.ConfigureConsumer<OrderCreatedConsumer>(context);
                 e.AutoOffsetReset = AutoOffsetReset.Earliest;
             });*/
        });
    });
});
builder.Services.AddScoped<ReserveInventoryActivity>();


var app = builder.Build();

Console.WriteLine("AFTER BUILD");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();