using DispatchManager.Application.Features.Orders.Commands.CreateOrder;
using DispatchManager.Application.Features.Orders.Commands.UpdateOrderStatus;
using DispatchManager.Application.Features.Orders.Queries.GetOrderDetail;
using DispatchManager.Application.Features.Orders.Queries.GetOrders;
using DispatchManager.Application.Features.Orders.Queries.GetOrdersByCustomer;
using DispatchManager.Application.Features.Orders.Queries.GetOrdersMetrics;
using DispatchManager.Application.Features.Orders.Queries.GetOrdersReport;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DispatchManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="command">Order creation data</param>
    /// <returns>Created order</returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        _logger.LogInformation("Creating order for customer {CustomerId}", command.CustomerId);

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return CreatedAtAction(
                nameof(GetOrderDetail),
                new { id = result.Data!.Id },
                result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get orders with pagination, filtering and sorting
    /// </summary>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="customerId">Optional customer filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 25)</param>
    /// <param name="sortBy">Sort field (default: CreatedAt)</param>
    /// <param name="sortDirection">Sort direction (default: desc)</param>
    /// <returns>Paginated list of orders</returns>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? searchTerm = null,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] string sortDirection = "desc")
    {
        _logger.LogInformation("Getting orders - Page: {PageNumber}, Size: {PageSize}, Sort: {SortBy} {SortDirection}",
            pageNumber, pageSize, sortBy, sortDirection);

        var query = new GetOrdersQuery
        {
            SearchTerm = searchTerm,
            Status = status,
            CustomerId = customerId,
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var result = await _mediator.Send(query);

        if (result.Success)
        {
            // Add pagination headers for frontend
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
    /// Get order details by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderDetail(Guid id)
    {
        var query = new GetOrderDetailQuery { OrderId = id };
        var result = await _mediator.Send(query);

        if (result.Success)
        {
            return Ok(result);
        }

        return NotFound(result);
    }

    /// <summary>
    /// Get orders by customer ID with pagination
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="sortBy">Sort field (default: CreatedAt)</param>
    /// <param name="sortDescending">Sort descending (default: true)</param>
    /// <returns>Paginated list of orders</returns>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetOrdersByCustomer(
        Guid customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetOrdersByCustomerQuery
        {
            CustomerId = customerId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
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
    /// Update order status
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="command">Status update data</param>
    /// <returns>Success response</returns>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
    {
        if (id != command.OrderId)
        {
            return BadRequest("Order ID in URL does not match command");
        }

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Generate orders report
    /// </summary>
    /// <param name="customerId">Optional customer ID filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="includeGlobalSummary">Include global summary (default: true)</param>
    /// <returns>Orders report</returns>
    [HttpGet("report")]
    public async Task<IActionResult> GetOrdersReport(
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool includeGlobalSummary = true)
    {
        var query = new GetOrdersReportQuery
        {
            CustomerId = customerId,
            StartDate = startDate,
            EndDate = endDate,
            IncludeGlobalSummary = includeGlobalSummary
        };

        var result = await _mediator.Send(query);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Download orders report as Excel
    /// </summary>
    /// <param name="customerId">Optional customer ID filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>Excel file</returns>
    [HttpGet("report/excel")]
    public async Task<IActionResult> DownloadOrdersReportExcel(
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetOrdersReportQuery
        {
            CustomerId = customerId,
            StartDate = startDate,
            EndDate = endDate,
            IncludeGlobalSummary = true
        };

        var reportResult = await _mediator.Send(query);

        if (!reportResult.Success || reportResult.Data == null)
        {
            return BadRequest(reportResult);
        }

        // Generate Excel file using ReportService
        var reportService = HttpContext.RequestServices.GetRequiredService<Application.Contracts.Infrastructure.IReportService>();
        var excelBytes = await reportService.GenerateOrdersReportExcelAsync(reportResult.Data);

        var fileName = $"orders_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

        return File(excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    /// <summary>
    /// Get comprehensive orders metrics for dashboard
    /// </summary>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="includeTrends">Include trend data (default: true)</param>
    /// <returns>Comprehensive orders metrics</returns>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetOrdersMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool includeTrends = true)
    {
        _logger.LogInformation("Getting orders metrics - StartDate: {StartDate}, EndDate: {EndDate}",
            startDate, endDate);

        var query = new GetOrdersMetricsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            IncludeTrends = includeTrends
        };

        var result = await _mediator.Send(query);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}