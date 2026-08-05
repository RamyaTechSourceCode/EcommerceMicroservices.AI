using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;
using ProductService.Api.Requests;
using ProductService.Application.CreateProducts;
using ProductService.Application.DeleteProduct;
using ProductService.Application.GetProductById;
using ProductService.Application.GetProducts;
using ProductService.Application.Interfaces;
using ProductService.Application.UpdateProducts;
using ProductService.Domain.Entities;

namespace ProductService.Api.Controllers
{
    [ApiController]
   // [Authorize]
    [Route("api/products")]
   
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //Implementing CQRS [Command Query Responsibility Segregation] 
        // with mediator
        [HttpPost]
       // [Authorize(Policy = "AccessAsUserAndAdmin")]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var command = new CreateProductCommand(
               request.Name,
               request.Description,
               request.Price,
               request.StockQuantity,
               request.Category,
               request.Status);

            var id = await _mediator.Send(command);

            return Ok(id);
        }
       
       
        [HttpGet("{id}")]
        [RequiredScope("Product.Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Console.WriteLine(Request.Headers.Authorization);

            var product = await _mediator.Send(
                new GetProductByIdQuery(id));

            if (product is null)
                return NotFound();

            return Ok(product);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id)
                return BadRequest();

            var result = await _mediator.Send(command);

            return result ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
       // [Authorize(Roles = "Admin")]
       // [RequiredScope("access_as_user")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(
                new DeleteProductCommand(id));

            return result ? Ok() : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Console.WriteLine(Request.Headers.Authorization);

            var product = await _mediator.Send(
                new GetProductsQuery());

            if (product is null)
                return NotFound();

            return Ok(product);
        }
        /*
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        //Implementing CQRS [Command Query Responsibility Segregation] 
        // without mediator

        [HttpPost]
         public async Task<IActionResult> Create(
         Command command)
         {
            
            var handler = new Handler(_repository);
            var id = await handler.Handle(command);

            return Ok(id);

         }
       
        //No CQRS Implemented

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.StockQuantity);

            await _repository.AddAsync(product);

            await _repository.SaveChangesAsync();

            return Ok();
        }*/

    }
}
