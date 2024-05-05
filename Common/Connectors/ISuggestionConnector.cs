namespace Common.Connectors;

public interface ISuggestionConnector
{
    IAsyncEnumerable<string> GetSuggestion(string context, string personality, CancellationToken cancellationToken);
    void Initialize();
}