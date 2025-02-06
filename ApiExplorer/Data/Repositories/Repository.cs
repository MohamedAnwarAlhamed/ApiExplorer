using Dapper;
using System.Data;

namespace ApiExplorer.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        //Task<T?> GetByIdAsync(int id);
        //Task AddAsync(T entity);
        //Task UpdateAsync(T entity);
        //Task DeleteAsync(int id);
    }

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly IDbConnection _connection;
        protected readonly IDbTransaction _transaction;

        public Repository(IDbConnection connection, IDbTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _connection.QueryAsync<T>("SELECT * FROM " + typeof(T).Name, transaction: _transaction);
        }

        //public async Task<T?> GetByIdAsync(int id)
        //{
        //    return await _connection.QueryFirstOrDefaultAsync<T>($"SELECT * FROM {typeof(T).Name} WHERE Id = @Id", new { Id = id }, transaction: _transaction);
        //}

        //public async Task AddAsync(T entity)
        //{
        //    var properties = typeof(T).GetProperties().Where(p => p.Name != "Id").Select(p => p.Name);
        //    var columns = string.Join(", ", properties);
        //    var values = string.Join(", ", properties.Select(p => "@" + p));
        //    var sql = $"INSERT INTO {typeof(T).Name} ({columns}) VALUES ({values})";
        //    await _connection.ExecuteAsync(sql, entity, transaction: _transaction);
        //}

        //public async Task UpdateAsync(T entity)
        //{
        //    var properties = typeof(T).GetProperties().Where(p => p.Name != "Id").Select(p => $"{p.Name} = @{p.Name}");
        //    var setClause = string.Join(", ", properties);
        //    var sql = $"UPDATE {typeof(T).Name} SET {setClause} WHERE Id = @Id";
        //    await _connection.ExecuteAsync(sql, entity, transaction: _transaction);
        //}

        //public async Task DeleteAsync(int id)
        //{
        //    await _connection.ExecuteAsync($"DELETE FROM {typeof(T).Name} WHERE Id = @Id", new { Id = id }, transaction: _transaction);
        //}
    }
}
