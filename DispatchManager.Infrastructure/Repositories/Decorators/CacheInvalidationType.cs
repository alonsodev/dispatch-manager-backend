namespace DispatchManager.Infrastructure.Repositories.Decorators;

/// <summary>
/// Define los tipos de operaciones que requieren invalidación de cache
/// </summary>
public enum CacheInvalidationType
{
    /// <summary>
    /// Operación de creación - invalida listas y contadores
    /// </summary>
    Create,

    /// <summary>
    /// Operación de actualización - invalida entidad específica y listas
    /// </summary>
    Update,

    /// <summary>
    /// Operación de eliminación - invalida todo el cache relacionado
    /// </summary>
    Delete
}