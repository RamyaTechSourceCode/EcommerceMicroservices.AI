using CatalogService.Application.CreateCatalog;
using CatalogService.Application.Interfaces;
using CatalogService.Infrastructure.Messaging.Consumers;
using CatalogService.Infrastructure.Persistence;
using Confluent.Kafka;
using ECommerce.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateCatalogCommand).Assembly));


// Add services to the container.
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<ICatalogDbContext>(
    provider => provider.GetRequiredService<CatalogDbContext>());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{

    // REQUIRED: main bus (fixes IBus error)
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);

    });

    // Kafka Rider
    x.AddRider(rider =>
    {
        rider.AddConsumer<CatalogProjectionConsumer>();
       
        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");

            k.TopicEndpoint<ProductCreatedEvent>(
                "product.created",
                "catalog.service",
                e =>
                {
                    e.ConfigureConsumer<CatalogProjectionConsumer>(context);
                    e.AutoOffsetReset = AutoOffsetReset.Earliest;
                });
            k.TopicEndpoint<ProductDeletedEvent>(
               "product.deleted",
               "catalog.service",
               e =>
               {
                   e.ConfigureConsumer<CatalogProjectionConsumer>(context);
                   e.AutoOffsetReset = AutoOffsetReset.Earliest;
               });
            k.TopicEndpoint<ProductUpdatedEvent>(
               "product.updated",
               "catalog.service",
               e =>
               {
                   e.ConfigureConsumer<CatalogProjectionConsumer>(context);
                   e.AutoOffsetReset = AutoOffsetReset.Earliest;
               });
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
