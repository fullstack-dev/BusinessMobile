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
    public partial class RestaurantMenuListView : ContentView
    {
        public ListView ListViewMenus
        {
            get
            {
                return this.ListView;
            }
        }

        public ActivityIndicatorView IndicatorView
        {
            get
            {
                return this.ActivityIndicatorView;
            }
        }

        public RestaurantMenuListView()
        {
            InitializeComponent();
        }
    }
}