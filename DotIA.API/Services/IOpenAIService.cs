using System.Text;
using System.Text.Json;

namespace DotIA.API.Services
{
    public interface IOpenAIService
    {
        Task<string> ObterRespostaAsync(string pergunta);
    }

    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> ObterRespostaAsync(string pergunta)
        {
            try
            {
                var endpoint = _configuration["AzureOpenAI:Endpoint"];
                var apiKey = _configuration["AzureOpenAI:ApiKey"];

                if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
                {
                    return "⚠️ Configuração da Azure OpenAI não encontrada.";
                }

                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "Você é um assistente de TI." },
                        new { role = "user", content = pergunta }
                    },
                    max_tokens = 800,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                content.Headers.Add("api-key", apiKey);

                var response = await _httpClient.PostAsync(endpoint, content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(result);
                    var resposta = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return resposta ?? "Não consegui gerar resposta.";
                }

                return $"❌ Erro API: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"❌ Erro: {ex.Message}";
            }
        }
    }
}