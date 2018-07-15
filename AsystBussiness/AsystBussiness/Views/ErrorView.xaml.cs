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
    public partial class ErrorView : ContentView
    {
        public event EventHandler TryAgain;

        public ErrorView()
        {
            InitializeComponent();

            GridTryAgain.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (TryAgain != null)
                    {
                        TryAgain(this, EventArgs.Empty);
                    }
                })
            });
        }
    }
}