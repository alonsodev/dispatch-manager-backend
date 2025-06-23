using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Application.Models.CacheWrappers
{
    /// <summary>
    /// Wrapper para resultados paginados que puede ser cacheado (tipo referencia)
    /// </summary>
    /// <typeparam name="T">Tipo de entidad paginada</typeparam>
    public sealed class PagedResultWrapper<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int TotalCount { get; set; }

        public PagedResultWrapper() { }

        public PagedResultWrapper(IReadOnlyList<T> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }

        /// <summary>
        /// Conversión implícita desde tuple a wrapper
        /// </summary>
        public static implicit operator PagedResultWrapper<T>((IReadOnlyList<T> Items, int TotalCount) tuple)
            => new(tuple.Items, tuple.TotalCount);

        /// <summary>
        /// Conversión implícita desde wrapper a tuple
        /// </summary>
        public static implicit operator (IReadOnlyList<T> Items, int TotalCount)(PagedResultWrapper<T> wrapper)
            => (wrapper.Items, wrapper.TotalCount);
    }

    /// <summary>
    /// Wrapper para valores int que puede ser cacheado (tipo referencia)
    /// </summary>
    public sealed class CountWrapper
    {
        public int Value { get; set; }

        public CountWrapper() { }

        public CountWrapper(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Conversión implícita desde int a wrapper
        /// </summary>
        public static implicit operator CountWrapper(int value)
            => new(value);

        /// <summary>
        /// Conversión implícita desde wrapper a int
        /// </summary>
        public static implicit operator int(CountWrapper wrapper)
            => wrapper?.Value ?? 0;
    }
}
