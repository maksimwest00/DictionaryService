namespace DictionaryService.Infrastructure.BackgroundServices;

public class RecordCleanupOptions
{
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(24);

    public int RetentionDays { get; set; } = 30;

    public int BatchSize { get; set; } = 100;
}