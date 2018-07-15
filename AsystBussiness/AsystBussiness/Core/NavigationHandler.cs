using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Core
{
    class NavigationHandler
    {
        static MasterDetailPage _masterPage;

        public static void SetDetailPage(Page page)
        {
            try
            {
                _masterPage.Detail = new NavigationPage(page);
            }
            catch (Exception error)
            {
                ExceptionHandler.Catch(error);
            }
        }

        internal static void SetMasterPage(MasterDetailPage masterPage)
        {
            _masterPage = masterPage;
        }
    }
}
