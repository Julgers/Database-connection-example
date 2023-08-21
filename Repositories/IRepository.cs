namespace DatabaseExample.Repositories;

/*
 * This repository interface serves as an abstraction of repository objects in some storage system.
 * This way, the application can use a set of model instances without having to be aware of the storage implementation.
 * This allows for a nice separation of concerns and you can change storage systems
 * easily without having to change the main application logic, by just changing IRepository implementations.
 */
public interface IRepository<T, TKey> where T : class
{
    public Task CreateAsync(T item);
    public Task DeleteAsync(T item);
    public Task UpdateAsync(T item);
    public Task<bool> ExistsAsync(TKey key);
    public Task<T?> FindAsync(TKey key);
}