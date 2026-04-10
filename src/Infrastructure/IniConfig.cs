using CF_DUC_Tool.src.Core;
using CF_DUC_Tool.src.Core.Exceptions;

namespace CF_DUC_Tool.src.Infrastructure;

public class IniConfigService : IConfigService
{
    private readonly ILogger _logger;
    public IniConfigService(ILogger logger) => _logger = logger;

    public AppConfig Load()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
        if (!File.Exists(path)) throw new ConfigurationException($"Arquivo '{path}' não encontrado no diretório da aplicação.");

        var appConfig = new AppConfig();
        DnsRecordConfig? currentRecord = null;

        foreach (var line in File.ReadAllLines(path))
        {
            var cleanLine = line.Trim();
            if (string.IsNullOrWhiteSpace(cleanLine) || cleanLine.StartsWith(";")) continue;

            if (cleanLine.StartsWith("[") && cleanLine.EndsWith("]"))
            {
                var sectionName = cleanLine[1..^1];
                if (sectionName.ToLower() == "general")
                {
                    currentRecord = null;
                }
                else
                {
                    currentRecord = new DnsRecordConfig { ProfileName = sectionName };
                    appConfig.Records.Add(currentRecord);
                }
                continue;
            }

            var parts = cleanLine.Split('=', 2);
            if (parts.Length != 2) continue;
            var key = parts[0].Trim();
            var value = parts[1].Trim();

            if (currentRecord == null)
            {
                if (key == "IntervalMinutes" && int.TryParse(value, out int min)) appConfig.IntervalMinutes = min;
            }
            else
            {
                switch (key)
                {
                    case "ApiToken": currentRecord.ApiToken = value; break;
                    case "ZoneId": currentRecord.ZoneId = value; break;
                    case "RecordName": currentRecord.RecordName = value; break;
                    case "RecordType": currentRecord.RecordType = value.ToUpper(); break;
                    case "TTL": if (int.TryParse(value, out int t)) currentRecord.Ttl = t; break;
                    case "Proxied": currentRecord.Proxied = ParseBool(value); break;
                }
            }
        }

        if (appConfig.Records.Count == 0) throw new ConfigurationException("Nenhum perfil de configuração válido foi encontrado no arquivo config.ini.");
        return appConfig;
    }

    private bool ParseBool(string val) =>
        bool.TryParse(val, out var b) ? b : (val == "1" || val.ToLower() == "on" || val.ToLower() == "true");
}
