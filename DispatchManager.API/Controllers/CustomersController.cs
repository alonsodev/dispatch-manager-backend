using DispatchManager.Application.Features.Customers.Commands.CreateCustomer;
using DispatchManager.Application.Features.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DispatchManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    /// <param name="command">Customer creation data</param>
    /// <returns>Created customer</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        _logger.LogInformation("Creating customer with email {Email}", command.Email);

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return CreatedAtAction(
                nameof(GetCustomers),
                new { },
                result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get customers with optional search and pagination
    /// </summary>
    /// <param name="searchTerm">Optional search term for customer name</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of customers</returns>
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetCustomersQuery
        {
            SearchTerm = searchTerm,
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
    /// Get customer list for dropdowns (simplified data)
    /// </summary>
    /// <returns>List of customers with ID and Name only</returns>
    [HttpGet("list")]
    public async Task<IActionResult> GetCustomerList()
    {
        var query = new GetCustomersQuery { PageSize = 1000 }; // Get all for dropdown
        var result = await _mediator.Send(query);

        if (result.Success)
        {
            var customerList = result.Data?.Select(c => new { c.Id, c.Name, c.Email }).ToList();
            return Ok(new { success = true, data = customerList });
        }

        return BadRequest(result);
    }
}