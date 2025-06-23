using DispatchManager.Application.Features.Products.Commands.CreateProduct;
using DispatchManager.Application.Features.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DispatchManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="command">Product creation data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        _logger.LogInformation("Creating product with name {Name}", command.Name);

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return CreatedAtAction(
                nameof(GetProducts),
                new { },
                result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get products with optional filters and pagination
    /// </summary>
    /// <param name="searchTerm">Optional search term for product name</param>
    /// <param name="minPrice">Optional minimum price filter</param>
    /// <param name="maxPrice">Optional maximum price filter</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? searchTerm = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsQuery
        {
            SearchTerm = searchTerm,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (result.Success)
        {
            // Add pagination headers
            Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
            {
                result.TotalCount,
                result.PageSize,
                result.PageNumber,
                result.TotalPages,
                result.HasPreviousPage,
                result.HasNextPage
            }));

            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get product list for dropdowns (simplified data)
    /// </summary>
    /// <returns>List of products with ID, Name and Price</returns>
    [HttpGet("list")]
    public async Task<IActionResult> GetProductList()
    {
        var query = new GetProductsQuery { PageSize = 100 }; // Get all for dropdown
        var result = await _mediator.Send(query);

        if (result.Success)
        {
            var productList = result.Data?.Select(p => new {
                p.Id,
                p.Name,
                p.UnitPrice,
                p.Unit
            }).ToList();
            return Ok(new { success = true, data = productList });
        }

        return BadRequest(result);
    }
}