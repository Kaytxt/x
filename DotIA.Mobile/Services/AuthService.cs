using System.Text;
using System.Text.Json;
using DotIA_Mobile.Models;

namespace DotIA_Mobile.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        // URL para Android Emulator acessar localhost da máquina host
        private const string API_BASE_URL = "http://10.0.2.2:5100";

        public AuthService()
        {
            // Configurar HttpClient para conectar na API
            var handler = new HttpClientHandler();

            // Para desenvolvimento - ignora certificados SSL (apenas em debug)
#if DEBUG
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(API_BASE_URL)
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Serializar requisição
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Fazer chamada para a API real
                var response = await _httpClient.PostAsync("/api/Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    // Ler resposta da API
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                    return apiResponse ?? new LoginResponse
                    {
                        Sucesso = false,
                        Mensagem = "Resposta inválida da API"
                    };
                }
                else
                {
                    // Erro da API
                    return new LoginResponse
                    {
                        Sucesso = false,
                        Mensagem = $"Erro: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                // Erro de conexão
                return new LoginResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro na conexão: {ex.Message}"
                };
            }
        }
    }
}