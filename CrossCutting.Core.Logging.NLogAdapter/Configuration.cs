using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CrossCutting.Core.Logging.NLogAdapter;

public class Configuration
{
    private static string s_configFileName = "config.json";

    public static void ConfigureLogger()
    {
        LoggingConfiguration config = new LoggingConfiguration();
        FileTarget fileLogTarget = new FileTarget();
        fileLogTarget.Layout = GetValue("CrossCutting.Core.Logging.NLogAdapter", "FileLayout", "${longdate}|${level:uppercase=true}|${logger}|${message}");
        fileLogTarget.FileName = GetValue("CrossCutting.Core.Logging.NLogAdapter", "FileName", "logs/iDxLog.log");
        fileLogTarget.ArchiveFileName = GetValue("CrossCutting.Core.Logging.NLogAdapter", "ArchiveFileName", "logs/iDxLog.{#}.log");
        fileLogTarget.ArchiveAboveSize = GetValue<long>("CrossCutting.Core.Logging.NLogAdapter", "ArchiveAboveSize", -1);
        fileLogTarget.MaxArchiveFiles = GetValue("CrossCutting.Core.Logging.NLogAdapter", "MaxArchiveFiles", 0);
        fileLogTarget.KeepFileOpen = GetValue("CrossCutting.Core.Logging.NLogAdapter", "KeepFileOpen", true);
        string encoding = GetValue("CrossCutting.Core.Logging.NLogAdapter", "Encoding", "utf8");
        fileLogTarget.Encoding = StringToEncoding(encoding);
        string archiveNumbering = GetValue("CrossCutting.Core.Logging.NLogAdapter", "ArchiveNumbering", "DateAndSequence");
        fileLogTarget.ArchiveNumbering = StringToArchiveNumberingMode(archiveNumbering);
        fileLogTarget.ArchiveDateFormat = GetValue("CrossCutting.Core.Logging.NLogAdapter", "ArchiveDateFormat", "yyyy-MM-dd");

        ColoredConsoleTarget consoleLogTarget = new ColoredConsoleTarget();
        consoleLogTarget.Layout = GetValue("CrossCutting.Core.Logging.NLogAdapter", "ConsoleLayout", "${longdate}|${level:uppercase=true}|${logger}|${message}");

        string fileLogLevel = GetValue("CrossCutting.Core.Logging.NLogAdapter", "FileLogLevel", "Error");
        string consoleLogLevel = GetValue("CrossCutting.Core.Logging.NLogAdapter", "ConsoleLogLevel", "Error");

        config.AddRule(StringToLogLevel(fileLogLevel), LogLevel.Fatal, fileLogTarget);
        config.AddRule(StringToLogLevel(consoleLogLevel), LogLevel.Fatal, consoleLogTarget);

        LogManager.Configuration = config;
    }

    private static ArchiveNumberingMode StringToArchiveNumberingMode(string value)
    {
        return value.ToLower() switch
        {
            "sequence" => ArchiveNumberingMode.Sequence,
            "rolling" => ArchiveNumberingMode.Rolling,
            "date" => ArchiveNumberingMode.Date,
            "dateandsequence" => ArchiveNumberingMode.DateAndSequence,
            _ => ArchiveNumberingMode.DateAndSequence
        };
    }

    private static LogLevel StringToLogLevel(string value)
    {
        return value.ToLower() switch
        {
            "trace" => LogLevel.Trace,
            "debug" => LogLevel.Debug,
            "info" => LogLevel.Info,
            "warn" => LogLevel.Warn,
            "error" => LogLevel.Error,
            "fatal" => LogLevel.Fatal,
            "off" => LogLevel.Off,
            _ => LogLevel.Error
        };
    }

    private static Encoding StringToEncoding(string value)
    {
        return value.ToLower() switch
        {
            "utf8" => Encoding.UTF8,
            "utf-8" => Encoding.UTF8,
            "unicode" => Encoding.Unicode,
            "bigendianunicode" => Encoding.BigEndianUnicode,
            "utf32" => Encoding.UTF32,
            "utf.32" => Encoding.UTF32,
            "ascii" => Encoding.ASCII,
            "latin1" => Encoding.Latin1,
#pragma warning disable SYSLIB0001 // Typ oder Element ist veraltet
            "utf7" => Encoding.UTF7,
            "utf-7" => Encoding.UTF7,
#pragma warning restore SYSLIB0001 // Typ oder Element ist veraltet
            _ => Encoding.UTF8
        };
    }

    private static T GetValue<T>(string configName, string key, T defaultValue)
    {
        try
        {
            JArray configFile = JArray.Parse(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, s_configFileName)));
            foreach (dynamic configuration in configFile)
            {
                if (!configuration.Name.ToString().Equals(configName, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (dynamic entry in configuration.Entries)
                {
                    if (entry.Key.ToString().Equals(key, StringComparison.OrdinalIgnoreCase))
                        return entry.Value;
                }
            }
        }
        catch
        {
            // ignored
        }

        return defaultValue;
    }
}