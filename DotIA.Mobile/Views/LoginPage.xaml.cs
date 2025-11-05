using DotIA_Mobile.Models;
using DotIA_Mobile.Services;
using System.Diagnostics;

namespace DotIA_Mobile.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly IAuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                lblMensagem.Text = "⚠️ Digite seu e-mail";
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSenha.Text))
            {
                lblMensagem.Text = "⚠️ Digite sua senha";
                return;
            }

            btnEntrar.IsEnabled = false;
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;
            lblMensagem.Text = string.Empty;

            try
            {
                // ✅ PRIMEIRO: TESTAR CONEXÃO COM A API
                Debug.WriteLine("🔗 TESTANDO CONEXÃO COM A API...");

                bool conexaoOk = await TestarConexaoAPI();

                if (!conexaoOk)
                {
                    lblMensagem.Text = "❌ Sem conexão com a API";
                    return;
                }

                Debug.WriteLine("✅ CONEXÃO OK - INICIANDO LOGIN...");

                // ✅ SEGUNDO: FAZER LOGIN
                var request = new LoginRequest
                {
                    Email = txtEmail.Text.Trim(),
                    Senha = txtSenha.Text
                };

                var response = await _authService.LoginAsync(request);

                if (response.Sucesso)
                {
                    await DisplayAlert("Sucesso", "Login realizado!", "OK");
                    lblMensagem.Text = "✅ Login realizado com sucesso!";

                    // Aqui você pode navegar para a próxima tela
                    // await Navigation.PushAsync(new MainPage());
                }
                else
                {
                    lblMensagem.Text = $"❌ {response.Mensagem}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 ERRO: {ex.Message}");
                lblMensagem.Text = $"❌ Erro: {ex.Message}";
            }
            finally
            {
                btnEntrar.IsEnabled = true;
                loadingIndicator.IsVisible = false;
                loadingIndicator.IsRunning = false;
            }
        }

        // ✅ MÉTODO PARA TESTAR CONEXÃO
        private async Task<bool> TestarConexaoAPI()
        {
            try
            {
                using var testClient = new HttpClient();
                testClient.Timeout = TimeSpan.FromSeconds(10);

                var testResponse = await testClient.GetAsync("http://10.0.2.2:5100/api/Auth/departamentos");

                Debug.WriteLine($"🔗 STATUS DA CONEXÃO: {testResponse.StatusCode}");

                if (testResponse.IsSuccessStatusCode)
                {
                    Debug.WriteLine("✅ CONEXÃO COM API ESTABELECIDA!");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"❌ API RETORNOU ERRO: {testResponse.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"💥 ERRO NA CONEXÃO: {ex.Message}");
                return false;
            }
        }
    }
}