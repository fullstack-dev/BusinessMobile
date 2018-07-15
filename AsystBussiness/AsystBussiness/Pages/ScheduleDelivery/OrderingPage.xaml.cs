using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.ScheduleDelivery
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OrderingPage : ContentPage
	{
		public OrderingPage (string deliveryAddress, string zipCode, DateTime deliveryDate, string deliveryTime)
		{
			InitializeComponent ();
            this.BindingContext = OrderingPageModel.GetSingleton(this.Navigation);
            (this.BindingContext as OrderingPageModel).InitializeAsync(deliveryAddress, zipCode, deliveryDate, deliveryTime);
        }
	}
}