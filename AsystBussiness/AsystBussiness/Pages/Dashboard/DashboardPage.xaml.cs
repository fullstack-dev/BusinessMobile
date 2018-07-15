using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DashboardPage : ContentPageBase
    {
        public DashboardPage()
        {
            InitializeComponent();
            this.BindingContext = new DashboardPageModel();
            this.Appearing += DashboardPage_Appearing;
        }

        private void DashboardPage_Appearing(object sender, EventArgs e)
        {
            NavigationPage.SetHasNavigationBar(this, true);
            (this.BindingContext as DashboardPageModel).OnAppearing();
        }
    }
}