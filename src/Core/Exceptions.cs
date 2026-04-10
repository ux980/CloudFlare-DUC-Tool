namespace CF_DUC_Tool.src.Core.Exceptions;

public enum ErrorSeverity { Warning, Error, Critical }

public abstract class DdnsBaseException : Exception
{
    public ErrorSeverity Severity { get; }
    protected DdnsBaseException(string message, ErrorSeverity severity) : base(message)
    {
        Severity = severity;
    }
}

public class NetworkException : DdnsBaseException
{
    public NetworkException(string message) : base($"Falha de Rede: {message}", ErrorSeverity.Error) { }
}

public class ValidationException : DdnsBaseException
{
    public ValidationException(string message) : base($"Dados Inválidos: {message}", ErrorSeverity.Warning) { }
}

public class ConfigurationException : DdnsBaseException
{
    public ConfigurationException(string message) : base($"Erro de Configuração: {message}", ErrorSeverity.Critical) { }
}

