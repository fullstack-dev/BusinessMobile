using AsystBussiness.Core;
using AsystBussiness.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Pages
{
    public class ContentPageBase : ContentPage, IReloadPage
    {
        public ContentPageBase()
        {
        }

        public bool IsErrorPage
        {
            get;
            set;
        }

        public virtual void ReloadPage()
        {
            IsErrorPage = false;
        }

        public virtual void ShowLoadErrorPage()
        {
            if (!IsErrorPage)
            {
                IsErrorPage = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        var errorView = new ErrorView();
                        var content = Content;
                        Content = errorView;
                        errorView.TryAgain += (sender, e) =>
                        {
                            this.IsErrorPage = false;
                            Content = content;
                            ReloadPage();
                        };
                    }
                    catch(Exception e)
                    {
                        ExceptionHandler.Catch(e);
                    }                   
                });
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        public static bool CheckCurrentPage(Page currentPage)
        {
            try
            {
                if (Application.Current != null)
                {
                    if (Application.Current.MainPage is MasterDetailPage)
                    {
                        var masterDetailPage = Application.Current.MainPage as MasterDetailPage;
                        if ((masterDetailPage).Detail is NavigationPage)
                        {
                            return ((masterDetailPage).Detail as NavigationPage).CurrentPage == currentPage;
                        }
                        else
                        {
                            return masterDetailPage.Detail == currentPage;
                        }
                    }
                    else
                    {
                        if (Application.Current.MainPage is NavigationPage)
                        {
                            return (Application.Current.MainPage as NavigationPage).CurrentPage == currentPage;
                        }
                        else
                        {
                            return Application.Current.MainPage == currentPage;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Check page when app pause/stop
                ExceptionHandler.Catch(e);
                return false;
            }
            return false;
        }

        public bool CheckCurrentPage()
        {
            return CheckCurrentPage(this);
        }
    }
}