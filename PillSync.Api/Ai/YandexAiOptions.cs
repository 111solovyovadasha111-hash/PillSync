namespace PillSync.Api.Ai;
public class YandexAiOptions
{
    public const string SectionName = "YandexAi";

    public bool UseMock { get; set; } = true;
    public string BaseUrl { get; set; } = "https://llm.api.cloud.yandex.net/";
    public string FolderId { get; set; } = "";
    public string Model { get; set; } = "yandexgpt-lite";
    public string ModelVersion { get; set; } = "latest";
    public double Temperature { get; set; } = 0.1;
    public int MaxTokens { get; set; } = 2000;
    public int TimeoutSeconds { get; set; } = 30;
    public string ApiKey { get; set; } = "";   // только из секретов
}

