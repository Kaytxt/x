using DotIA_Mobile.Views;

namespace DotIA_Mobile;

public partial class App : Application
{
    public App()
    {
        // Teste simples - se aparecer uma tela vermelha, o app está funcionando
        MainPage = new ContentPage
        {
            BackgroundColor = Colors.Red,
            Content = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "APP FUNCIONANDO!",
                        TextColor = Colors.White,
                        FontSize = 32,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Button
                    {
                        Text = "Ir para Login",
                        BackgroundColor = Colors.Blue,
                        TextColor = Colors.White,
                        Margin = new Thickness(0, 20, 0, 0)
                    }
                }
            }
        };
    }
}