using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Controls
{
    public class CustomNavigationPage : NavigationPage
    {
        public CustomNavigationPage()
        {
        }

        public CustomNavigationPage(Page root) : base(root)
        {
        }

        public Func<Page, bool> PopView;
        public Func<Page, bool> PopToRoot;
        public Func<Page, bool> PushView;
    }
}
