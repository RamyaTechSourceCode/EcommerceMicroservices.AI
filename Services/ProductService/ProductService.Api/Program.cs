using ECommerce.Contracts.Events;
using ECommerce.Messaging.Abstractions;
using ECommerce.Messaging.Kafka;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using ProductService.Application.CreateProducts;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;
using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// JWT Auth setup
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    // policy based authorization
    options.AddPolicy("AccessAsUserAndAdmin", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var scp = context.User.Claims
                      .FirstOrDefault(c =>
                          c.Type == "scp" ||
                          c.Type.EndsWith("/scope") ||
                          c.Type.EndsWith("identity/claims/scope"))
                      ?.Value;

            var hasScope = !string.IsNullOrWhiteSpace(scp)
                && scp.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                      .Contains("access_as_user");

            var isAdmin = context.User.IsInRole("Admin");

            return  hasScope && isAdmin;
        });
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IProductDbContext>(
    provider => provider.GetRequiredService<ProductDbContext>());

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(CreateProductHandler).Assembly));// registers assembly for all handlers (gets the DLL/assembly containing that type)

builder.Services.AddSingleton<IEventBus, KafkaEventBus>();

/*builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => { });

    x.AddRider(rider =>
    {
        rider.AddProducer<ProductCreatedEvent>("product.created");

        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");
        });
    });
});*/
/*
//FluentValidation works without a MediatR pipeline if no MediatR used
builder.Services.AddFluentValidationAutoValidation();
*/

builder.Services.AddValidatorsFromAssemblyContaining<Validator>();

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

var app = builder.Build();

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

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features
            .Get<IExceptionHandlerFeature>()?
            .Error;

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = 400;

            var errors = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage));

            await context.Response.WriteAsJsonAsync(new
            {
                Errors = errors
            });
        }
    });
});

app.MapControllers();



app.Run();
