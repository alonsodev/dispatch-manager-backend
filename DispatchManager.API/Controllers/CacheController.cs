using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace DispatchManager.API.Controllers;

/// <summary>
/// Controller para manejo de operaciones de caché
/// Diseñado siguiendo principios RESTful y Clean Architecture
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(
        ICacheService cacheService,
        ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Invalida toda la caché del sistema
    /// </summary>
    /// <returns>Respuesta indicando el resultado de la operación</returns>
    /// <response code="200">Caché invalidada exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("invalidate-all")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BaseResponse>> InvalidateAllCache(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando invalidación completa de caché");

            // Invalidar toda la caché
            await _cacheService.InvalidateAllAsync(cancellationToken);

            _logger.LogInformation("Caché invalidada exitosamente");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al invalidar toda la caché");

            return StatusCode(
                StatusCodes.Status500InternalServerError
            );
        }
    }
}
