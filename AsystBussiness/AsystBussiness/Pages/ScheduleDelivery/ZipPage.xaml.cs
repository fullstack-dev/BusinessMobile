using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.ScheduleDelivery
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ZipPage : ContentPage
    {
        public ZipPage()
        {
            InitializeComponent();
            this.BindingContext = ZipPageModel.GetSingleton(this.Navigation);
            this.Appearing += ZipPage_Appearing;
        }

        private void ZipPage_Appearing(object sender, EventArgs e)
        {
            if (Debugger.IsAttached)
            {
                var model = ZipPageModel.GetSingleton(this.Navigation);
                if (string.IsNullOrEmpty(model.ZipCode))
                {
                    model.ZipCode = "31750";
                }                   
            }
        }
    }
}