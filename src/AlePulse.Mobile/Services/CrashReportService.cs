using System.Collections.Concurrent;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace AlePulse.Mobile.Services;

public static class CrashReportService
{
    public const string SupportEmail = "alberto.emoto@gmail.com";

    private static readonly ConcurrentQueue<string> _breadcrumbs = new();
    private static readonly object _fileLock = new();

    public static string LogsFolder => Path.Combine(FileSystem.AppDataDirectory, "logs");
    public static string ReportedFolder => Path.Combine(LogsFolder, "reported");

    public static void AddBreadcrumb(string message)
    {
        try
        {
            _breadcrumbs.Enqueue($"{DateTime.Now:HH:mm:ss.fff} | {message}");
            while (_breadcrumbs.Count > 40 && _breadcrumbs.TryDequeue(out _)) { }
        }
        catch { }
    }

    public static void LogException(string source, Exception? ex)
    {
        var sb = new StringBuilder();
        try
        {
            AppendHeader(sb, source);
            sb.AppendLine("--- Exceção ---");
            var current = ex;
            var level = 0;
            while (current != null)
            {
                sb.AppendLine($"[{level}] {current.GetType().FullName}: {current.Message}");
                sb.AppendLine(current.StackTrace ?? "(sem stack trace)");
                current = current.InnerException;
                level++;
            }
            if (ex == null) sb.AppendLine("(exceção sem detalhes)");
            WriteLogFile(sb.ToString());
        }
        catch { }
    }

    public static void LogError(string source, string message)
    {
        var sb = new StringBuilder();
        try
        {
            AppendHeader(sb, source);
            sb.AppendLine("--- Erro ---");
            sb.AppendLine(message);
            WriteLogFile(sb.ToString());
        }
        catch { }
    }

    public static bool HasUnreportedCrashes
    {
        get
        {
            try
            {
                if (!Directory.Exists(LogsFolder)) return false;
                return Directory.GetFiles(LogsFolder, "crash-*.log").Length > 0;
            }
            catch { return false; }
        }
    }

    public static List<CrashLogItem> GetUnreportedLogs()
    {
        var list = new List<CrashLogItem>();
        try
        {
            if (!Directory.Exists(LogsFolder)) return list;
            foreach (var file in Directory.GetFiles(LogsFolder, "crash-*.log").OrderByDescending(f => f))
            {
                try
                {
                    var content = File.ReadAllText(file);
                    list.Add(new CrashLogItem
                    {
                        FileName = Path.GetFileName(file),
                        DateText = DateFromFileName(file),
                        Preview = BuildPreview(content),
                        Content = content
                    });
                }
                catch { }
            }
        }
        catch { }
        return list;
    }

    public static string BuildReportBody(string userDescription)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.IsNullOrWhiteSpace(userDescription) ? "(sem descrição do usuário)" : userDescription);
        sb.AppendLine();
        sb.AppendLine($"App: AlePulse v{AppInfo.VersionString} (build {AppInfo.BuildString})");
        sb.AppendLine($"Dispositivo: {DeviceInfo.Manufacturer} {DeviceInfo.Model}");
        sb.AppendLine($"Sistema: {DeviceInfo.Platform} {DeviceInfo.VersionString}");
        sb.AppendLine($"Último erro de API: {ApiService.LastError ?? "nenhum"}");
        sb.AppendLine();
        foreach (var log in GetUnreportedLogs())
        {
            sb.AppendLine($"===== REGISTRO {log.DateText} =====");
            if (log.Content.Length > 12000) sb.AppendLine(log.Content[..12000] + "\n... (conteúdo truncado)");
            else sb.AppendLine(log.Content);
            sb.AppendLine();
        }
        if (sb.Length > 60000) sb.Length = 60000;
        return sb.ToString();
    }

    public static void MarkAllReported()
    {
        lock (_fileLock)
        {
            try
            {
                if (!Directory.Exists(LogsFolder)) return;
                Directory.CreateDirectory(ReportedFolder);
                foreach (var file in Directory.GetFiles(LogsFolder, "crash-*.log"))
                {
                    try
                    {
                        var dest = Path.Combine(ReportedFolder, Path.GetFileName(file));
                        File.Move(file, dest, overwrite: true);
                    }
                    catch { }
                }
                TrimFolder(ReportedFolder, 20);
            }
            catch { }
        }
    }

    private static void AppendHeader(StringBuilder sb, string source)
    {
        sb.AppendLine("===== ALEPULSE CRASH =====");
        sb.AppendLine($"Data/Hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine($"Origem: {source}");
        sb.AppendLine($"App: AlePulse v{AppInfo.VersionString} (build {AppInfo.BuildString})");
        sb.AppendLine($"Dispositivo: {DeviceInfo.Manufacturer} {DeviceInfo.Model}");
        sb.AppendLine($"Sistema: {DeviceInfo.Platform} {DeviceInfo.VersionString}");
        sb.AppendLine();
        sb.AppendLine("--- Últimas ações antes do erro ---");
        foreach (var b in _breadcrumbs) sb.AppendLine(b);
        sb.AppendLine();
        _breadcrumbs.Clear();
    }

    private static void WriteLogFile(string content)
    {
        lock (_fileLock)
        {
            try
            {
                Directory.CreateDirectory(LogsFolder);
                var file = Path.Combine(LogsFolder, $"crash-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..6]}.log");
                File.WriteAllText(file, content);
                TrimFolder(LogsFolder, 15);
            }
            catch { }
        }
    }

    private static void TrimFolder(string folder, int keep)
    {
        try
        {
            var files = Directory.GetFiles(folder, "crash-*.log").OrderByDescending(f => f).ToList();
            for (var i = keep; i < files.Count; i++)
            {
                try { File.Delete(files[i]); } catch { }
            }
        }
        catch { }
    }

    private static string DateFromFileName(string filePath)
    {
        try
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            var parts = name.Split('-');
            if (parts.Length >= 3 && DateTime.TryParseExact(parts[1] + parts[2], "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dt))
                return dt.ToString("dd/MM/yyyy HH:mm:ss");
            return File.GetLastWriteTime(filePath).ToString("dd/MM/yyyy HH:mm:ss");
        }
        catch
        {
            return Path.GetFileName(filePath);
        }
    }

    private static string BuildPreview(string content)
    {
        try
        {
            foreach (var line in content.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("[0]")) return trimmed.Length > 90 ? trimmed[..90] : trimmed;
            }
            foreach (var line in content.Split('\n'))
            {
                if (line.StartsWith("Origem:")) return line.Length > 90 ? line[..90] : line;
            }
            return "(registro de erro)";
        }
        catch
        {
            return "(registro de erro)";
        }
    }
}

public class CrashLogItem
{
    public string FileName { get; set; } = "";
    public string DateText { get; set; } = "";
    public string Preview { get; set; } = "";
    public string Content { get; set; } = "";
}
