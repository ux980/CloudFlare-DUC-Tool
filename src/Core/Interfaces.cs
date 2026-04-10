namespace CF_DUC_Tool.src.Core
{

    public interface ILogger
    {
        void LogInfo(string message);
        void LogSuccess(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(Exception ex);
    }

    public interface IConfigService
    {
        AppConfig Load();
    }

    public interface IIpProvider
    {
        Task<string> GetPublicIpAsync(string recordType);
    }

    public interface IDnsProvider
    {
        Task<(string Id, string Content)?> GetRecordInfoAsync(string apiToken, string zoneId, string recordName, string recordType);
        Task<bool> UpdateRecordAsync(string apiToken, string zoneId, string recordId, string recordName, string newIp, string type, int ttl, bool proxied);
    }
}