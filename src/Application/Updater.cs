using CF_DUC_Tool.src.Core;

namespace CF_DUC_Tool.src.Application;

public class DnsUpdaterService
{
    private readonly ILogger _logger;
    private readonly IIpProvider _ipProvider;
    private readonly IDnsProvider _dnsProvider;
    private readonly AppConfig _config;

    public DnsUpdaterService(ILogger logger, IIpProvider ipProvider, IDnsProvider dnsProvider, AppConfig config)
    {
        _logger = logger;
        _ipProvider = ipProvider;
        _dnsProvider = dnsProvider;
        _config = config;
    }

    public async Task ExecuteAsync()
    {
        string? cachedV4 = null, cachedV6 = null;

        foreach (var record in _config.Records)
        {
            try
            {
                _logger.LogInfo($"[{record.ProfileName}] Processando {record.RecordName} ({record.RecordType})...");

                string currentIp;
                if (record.RecordType == "AAAA")
                {
                    if (cachedV6 == null) cachedV6 = await _ipProvider.GetPublicIpAsync("AAAA");
                    currentIp = cachedV6;
                }
                else
                {
                    if (cachedV4 == null) cachedV4 = await _ipProvider.GetPublicIpAsync("A");
                    currentIp = cachedV4;
                }

                var info = await _dnsProvider.GetRecordInfoAsync(record.ApiToken, record.ZoneId, record.RecordName, record.RecordType);

                if (info == null)
                {
                    _logger.LogWarning($"[{record.ProfileName}] Registro não encontrado na Cloudflare.");
                    continue;
                }

                if (info.Value.Content == currentIp)
                {
                    _logger.LogSuccess($"[{record.ProfileName}] Sincronizado ({currentIp}).");
                    continue;
                }

                _logger.LogWarning($"[{record.ProfileName}] MUDANÇA: {info.Value.Content} -> {currentIp}");

                var success = await _dnsProvider.UpdateRecordAsync(record.ApiToken, record.ZoneId, info.Value.Id, record.RecordName, currentIp, record.RecordType, record.Ttl, record.Proxied);

                if (success) _logger.LogSuccess($"[{record.ProfileName}] Atualizado com sucesso!");
                else _logger.LogError($"[{record.ProfileName}] Falha na atualização.");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
    }
}