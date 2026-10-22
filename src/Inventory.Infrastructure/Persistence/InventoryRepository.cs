using Inventory.Application.Abstractions;
using Inventory.Application.Contracts;
using Inventory.Application.Errors;
using Inventory.Domain.Movements;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Inventory.Infrastructure.Persistence;

public sealed class InventoryRepository(InventoryDbContext database) : IInventoryRepository
{
    public async Task RegisterAsync(Guid productId, Guid userId, MovementType type, int quantity, string? note, CancellationToken cancellationToken)
    {
        var procedure = type == MovementType.Entry
            ? "CALL register_stock_entry({0}, {1}, {2}, {3})"
            : "CALL register_stock_exit({0}, {1}, {2}, {3})";
        try
        {
            // El tipo elige el procedimiento; los valores se envían parametrizados.
            await database.Database.ExecuteSqlRawAsync(procedure,
                new object[] { productId, quantity, note ?? (object)DBNull.Value, userId }, cancellationToken);
        }
        catch (PostgresException exception) when (exception.MessageText == "INSUFFICIENT_STOCK")
        {
            throw new BusinessRuleException("La salida supera las existencias disponibles.");
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new NotFoundException("El producto o usuario solicitado no existe.");
        }
    }

    public async Task<IReadOnlyList<InventoryItemDto>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)database.Database.GetDbConnection();
        var openedHere = connection.State != System.Data.ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            // El cursor debe consultarse dentro de la misma transacción.
            await using (var call = new NpgsqlCommand("CALL get_current_inventory(@cursor)", connection, transaction))
            {
                call.Parameters.Add(new NpgsqlParameter("cursor", NpgsqlDbType.Refcursor)
                {
                    Direction = System.Data.ParameterDirection.InputOutput,
                    Value = "inventory_cursor"
                });
                await call.ExecuteNonQueryAsync(cancellationToken);
            }

            var items = new List<InventoryItemDto>();
            await using (var fetch = new NpgsqlCommand("FETCH ALL IN \"inventory_cursor\"", connection, transaction))
            await using (var reader = await fetch.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    items.Add(new InventoryItemDto(
                        reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetDecimal(3),
                        reader.GetInt32(4), reader.GetInt32(5), reader.GetString(6)));
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return items;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyList<MovementDto>> GetRecentAsync(int take, CancellationToken cancellationToken) =>
        await database.Movements.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new MovementDto(x.Id, x.ProductId, x.Product.Sku, x.Product.Name,
                x.Type == MovementType.Entry ? "Entrada" : "Salida", x.Quantity, x.Note, x.User.Email, x.CreatedAt))
            .ToListAsync(cancellationToken);
}
