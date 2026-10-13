using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Exceptions;
using System.Collections.Concurrent;

namespace PromoCodeFactory.DataAccess.Repositories;


public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ConcurrentDictionary<Guid, T> _data;

    /// <summary>
    /// Инициализирует новый экземпляр репозитория и заполняет его указанным набором сущностей.
    /// </summary>
    /// <param name="data">Начальный набор сущностей для хранения.</param>
    public InMemoryRepository(IEnumerable<T> data)
    {
        _data = new ConcurrentDictionary<Guid, T>(data.Select(e => new KeyValuePair<Guid, T>(e.Id, e)));
    }

    /// <summary>
    /// Асинхронно получает все сущности из репозитория.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Коллекция всех сущностей, доступных только для чтения.</returns>
    public Task<IReadOnlyCollection<T>> GetAll(CancellationToken ct)
    {
        return Task.FromResult((IReadOnlyCollection<T>)_data.Values);
    }

    /// <summary>
    /// Асинхронно получает сущность по её идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор сущности.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Сущность с указанным идентификатором или <c>null</c>, если сущность не найдена.</returns>
    /// <exception cref="InvalidOperationException">Возникает при ошибке во время получения сущности.</exception>
    public Task<T?> GetById(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _data.TryGetValue(id, out var entity);

        return Task.FromResult(entity);
    }

    /// <summary>
    /// Асинхронно добавляет новую сущность в репозиторий.
    /// </summary>
    /// <param name="entity">Добавляемая сущность.</param>
    /// <param name="ct">Токен отмены операции.</param>
    public Task Add(T entity, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ct.ThrowIfCancellationRequested();

        return !_data.TryAdd(entity.Id, entity)
            ? throw new InvalidOperationException($"{typeof(T).Name} with Id '{entity.Id}' already exists.")
            : Task.CompletedTask;
    }

    /// <summary>
    /// Обновляет существующую сущность в репозитории.
    /// </summary>
    /// <param name="entity">Обновляемый объект.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="EntityNotFoundException">Возникает, если сущность с указанным идентификатором не найдена.</exception>
    /// <exception cref="InvalidOperationException">Возникает, если не удалось обновить сущность.</exception>
    public Task Update(T entity, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ct.ThrowIfCancellationRequested();

        if (!_data.TryGetValue(entity.Id, out var existing))
        {
            throw new EntityNotFoundException(typeof(T), entity.Id);
        }

        return !_data.TryUpdate(entity.Id, entity, existing)
            ? throw new InvalidOperationException($"Failed to update {typeof(T).Name} with Id '{entity.Id}'.")
            : Task.CompletedTask;
    }

    /// <summary>
    /// Асинхронно удаляет сущность с указанным идентификатором из хранилища.
    /// </summary>
    /// <param name="id">Идентификатор удаляемой сущности.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <exception cref="EntityNotFoundException">Возникает, если сущность с указанным идентификатором не найдена.</exception>
    public Task Delete(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return !_data.TryRemove(id, out _)
            ? throw new EntityNotFoundException(typeof(T), id)
            : Task.CompletedTask;
    }

}
