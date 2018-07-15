using AsystBussiness.Controls;
using AsystBussiness.Core;
using AsystBussiness.Pages.UserAccount;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Facades
{
    public class UserFacade
    {
        public void GetCurrentUser(Page currentPage, Action<User> doneAction)
        {
            var userModel = Shared.APIs.IUsers.GetCurrentUser();
            doneAction(userModel);
        }
    }
}
