using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DictionaryService.Infrastructure.BackgroundServices;

public class RecordCleanupBackgroundService : BackgroundService
{
    private readonly ILogger<RecordCleanupBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly RecordCleanupOptions _options;

    public RecordCleanupBackgroundService(
        ILogger<RecordCleanupBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<RecordCleanupOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RecordCleanupBackgroundService запущен");
        _logger.LogInformation(
            "Интервал выполнения: {Interval}, Срок хранения: {RetentionDays}, Размер батча: {BatchSize}",
            _options.CleanupInterval,
            _options.RetentionDays,
            _options.BatchSize);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldRecords(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке старых записей");
            }

            await Task.Delay(_options.CleanupInterval, cancellationToken);
        }

        _logger.LogInformation("RecordCleanupBackgroundService остановлен");
    }

    private async Task CleanupOldRecords(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DictionaryServiceDbContext>();

        var cutoffDate = DateTime.UtcNow.AddDays(-_options.RetentionDays);
        _logger.LogInformation("Начало очистки записей, деактивированных старее {CutoffDate}", cutoffDate);

        int totalDeleted = 0;

        totalDeleted += await DeleteRecordsInBatches(
            dbContext.Departments
                .IgnoreQueryFilters()
                .Where(d => !d.IsActive && d.DeletedAt < cutoffDate),
            dbContext,
            cancellationToken,
            "отделов");

        totalDeleted += await DeleteRecordsInBatches(
            dbContext.Locations
                .IgnoreQueryFilters()
                .Where(l => !l.IsActive && l.DeletedAt < cutoffDate),
            dbContext,
            cancellationToken,
            "локаций");

        totalDeleted += await DeleteRecordsInBatches(
            dbContext.Positions
                .IgnoreQueryFilters()
                .Where(p => !p.IsActive && p.DeletedAt < cutoffDate),
            dbContext,
            cancellationToken,
            "должностей");

        if (totalDeleted > 0)
        {
            _logger.LogInformation("Успешно удалено {TotalCount} записей", totalDeleted);
        }
        else
        {
            _logger.LogInformation("Записей для удаления не найдено");
        }
    }

    private async Task<int> DeleteRecordsInBatches<T>(
        IQueryable<T> query,
        DictionaryServiceDbContext dbContext,
        CancellationToken cancellationToken,
        string entityName)
        where T : class
    {
        int deletedCount = 0;
        bool hasMore = true;

        while (hasMore && !cancellationToken.IsCancellationRequested)
        {
            var batch = await query
                .Take(_options.BatchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count != 0)
            {
                dbContext.RemoveRange(batch);
                await dbContext.SaveChangesAsync(cancellationToken);
                deletedCount += batch.Count;
                _logger.LogInformation(
                    "Удалено {Count} {EntityName} (всего: {Total})",
                    batch.Count, entityName, deletedCount);
            }
            else
            {
                hasMore = false;
            }
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation("Всего удалено {Count} {EntityName}", deletedCount, entityName);
        }

        return deletedCount;
    }
}