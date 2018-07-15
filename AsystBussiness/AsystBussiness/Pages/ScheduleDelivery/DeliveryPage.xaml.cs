using AsystBussiness.Core;
using AsystBussiness.Facades;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.ScheduleDelivery
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeliveryPage : ContentPage
	{
        public DeliveryPage (string zipCode)
		{
            try
            {
                InitializeComponent();
                this.BindingContext = DeliveryPageModel.GetSingleton();
                this.Appearing += DeliveryPage_Appearing;
                (this.BindingContext as DeliveryPageModel).Initialize(this.Navigation, zipCode);
            }
            catch (Exception e)
            {
                Crashlytics.Log(e);
            }
        }

        private void DeliveryPage_Appearing(object sender, EventArgs e)
        {
            try
            {
                var model = DeliveryPageModel.GetSingleton();

                if (Debugger.IsAttached)
                {
                    model.DeliveryName = "Scott Spivey";
                    model.DeliveryPhone = "555-555-1212";
                    model.DeliveryAddress = "1668 Main St, Sarasota, FL, United States";
                }
            }
            catch (Exception ex)
            {
                Crashlytics.Log(ex);
            }
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var model = (DeliveryPageModel)this.BindingContext;
            model.FillAutoSuggestAsync(e.NewTextValue);
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            var prediction = (Prediction)e.SelectedItem;

            var model = (DeliveryPageModel)this.BindingContext;
            model.HandleItemSelected(prediction);
        }
    }
}