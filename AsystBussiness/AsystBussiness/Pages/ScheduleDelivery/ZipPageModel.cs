using AsystBussiness.Core;
using ColonyConcierge.Client.PlatformServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Pages.ScheduleDelivery
{
    class ZipPageModel : NotifyPropertyChanged
    {
        private static ZipPageModel _singleton;
        public static ZipPageModel GetSingleton(INavigation navigation)
        {
            if (_singleton == null)
                _singleton = new ZipPageModel();

            _singleton.OnAppearing(navigation);

            return _singleton;
        }

        INavigation _navigation;
        private void OnAppearing(INavigation navigation)
        {
            _navigation = navigation;
        }

        private ZipPageModel()
        {
            _singleton = this;
        }

        bool _isNextStepEnabled = false;
        public bool IsNextStepEnabled
        {
            get { return _isNextStepEnabled; }
            set { SetField(ref _isNextStepEnabled, value); }
        }

        bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetField(ref _isBusy, value); }
        }

        bool _isZipCheckEnabled = true;
        public bool IsZipCheckEnabled
        {
            get { return _isZipCheckEnabled; }
            set { SetField(ref _isZipCheckEnabled, value); }
        }

        string _zipCode;
        public string ZipCode
        {
            get { return _zipCode; }
            set
            {
                if (_zipCode != value)
                {
                    IsNextStepEnabled = false;
                    IsZipCheckEnabled = true;
                    ZipInformation = "";
                }

                SetField(ref _zipCode, value);
            }
        }

        string _zipInformation;
        public string ZipInformation
        {
            get { return _zipInformation; }
            set { SetField(ref _zipInformation, value); }
        }

        public Command CheckZipAvailability
        {
            get
            {
                return new Command(async () =>
                {
                    IsBusy = true;

                    try
                    {
                        IsZipCheckEnabled = false;
                        bool available = await Shared.APIs.IRestaurantBusiness.GetDeliveryAvailability_Async(Provider.G.Config["RestaurantLoc"], ZipCode);

                        if (available)
                        {
                            ZipInformation = String.Empty;
                            await Utils.PushAsync(_navigation, new DeliveryPage(ZipCode));
                        }
                        else
                        {
                            ZipInformation = "Delivery not available";
                        }
                    }
                    catch (Exception e)
                    {

                        Crashlytics.Log(e);
                    }

                    IsZipCheckEnabled = true;
                    IsBusy = false;
                });
            }
        }
    }
}
