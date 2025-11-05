using DotIA_Mobile.Views;

namespace DotIA_Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new NavigationPage(new LoginPage());
    }
}