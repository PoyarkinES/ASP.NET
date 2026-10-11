using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Domain.Administration;
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
    public async Task<T?> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                return _data.FirstOrDefault(f => f.Key == id).Value;
            },
            ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Асинхронно добавляет новую сущность в репозиторий.
    /// </summary>
    /// <param name="entity">Добавляемая сущность.</param>
    /// <param name="ct">Токен отмены операции.</param>
    public async Task Add(T entity, CancellationToken ct)
    {
        await Task.Run(() =>
        {
            _data.TryAdd(entity.Id, entity);
        }, ct);
    }

    /// <summary>
    /// Обновляет существующую сущность в репозитории. Если сущность не существует, она добавляется.
    /// </summary>
    /// <param name="entity">Обновляемый объект.</param>
    /// <param name="ct">Токен отмены операции.</param>
    public async Task Update(T entity, CancellationToken ct)
    {
        await Task.Run(() =>
        {
            var existing = _data.FirstOrDefault(f => f.Key == entity.Id).Value;

            if (existing is null)
            {
                _data.TryAdd(entity.Id, entity);
            }
            else
            {
                _data.TryUpdate(entity.Id, entity, existing);
            }
        }, ct);
    }

    /// <summary>
    /// Асинхронно удаляет сущность с указанным идентификатором из хранилища.
    /// </summary>
    /// <param name="id">Идентификатор удаляемой сущности.</param>
    /// <param name="ct">Токен отмены операции.</param>
    public async Task Delete(Guid id, CancellationToken ct)
    {
        await Task.Run(() =>
        {
            if (_data.TryRemove(id, out _))
            {
                return;
            }
            else
            {
                throw new EntityNotFoundException(typeof(Employee), id);
            }
        }, ct);
    }

}
