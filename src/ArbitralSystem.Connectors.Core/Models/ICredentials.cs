namespace ArbitralSystem.Connectors.Core.Models
{
    public interface ICredentials
    {
        string ApiKey { get; }
        string SecretKey { get; }
        string PassPhrase { get; }
    }
}