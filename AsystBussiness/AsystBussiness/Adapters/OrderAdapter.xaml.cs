using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Adapters
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderAdapter : ViewCell
    {
        public OrderAdapter()
        {
            InitializeComponent();     
        }
    }
}