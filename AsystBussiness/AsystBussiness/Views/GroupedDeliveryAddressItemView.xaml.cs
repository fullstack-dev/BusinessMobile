using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GroupedDeliveryAddressItemView : ContentView
    {
        public event EventHandler Clicked;

        public GroupedDeliveryAddressItemView()
        {
            InitializeComponent();

            this.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    if (Clicked != null)
                    {
                        Clicked(this, EventArgs.Empty);
                    }
                })
            });
        }
    }
}