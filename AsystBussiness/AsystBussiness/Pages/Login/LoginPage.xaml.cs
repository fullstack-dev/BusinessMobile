using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginPageModel();
            this.Appearing += LoginPage_Appearing;
        }

        private void LoginPage_Appearing(object sender, EventArgs e)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            (this.BindingContext as LoginPageModel).OnAppearing();
        }
    }
}