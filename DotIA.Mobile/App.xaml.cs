namespace DDTIA_UDDILE;

public partial class App : Application
{
    public App()
    {
        // TESTE ULTRA SIMPLES - página básica
        MainPage = new ContentPage
        {
            BackgroundColor = Colors.Red,
            Content = new Label
            {
                Text = "APP FUNCIONANDO!",
                TextColor = Colors.Black,
                FontSize = 32,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
    }
}