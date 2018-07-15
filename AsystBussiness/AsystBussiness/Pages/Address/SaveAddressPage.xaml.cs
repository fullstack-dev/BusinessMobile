using AsystBussiness.Controls;
using AsystBussiness.Core;
using AsystBussiness.Facades;
using AsystBussiness.Localization.Resx;
using AsystBussiness.Pages.UserAccount;
using ColonyConcierge.APIData.Data;
using Plugin.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.Address
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SaveAddressPage : ContentPageBase
    {
        ExtendedAddress mServiceAddress;
        //private bool mIsLoadingAddress = false;
        private double mLatitude, mLongitude;
        private Page ParentPage;
        private Action<ExtendedAddress> Saved;
        AddressFacade mAddressFacade = new AddressFacade();
        SerivesFacade serivesService = new SerivesFacade();
        private IAppServices mAppServices;

        public bool IsBusiness
        {
            get;
            set;
        }

        public string FullName { get; set; }
        public string EmailAddress { get; set; }

        public SaveAddressPage(Page parentPage, ExtendedAddress serviceAddress, Action<ExtendedAddress> saved = null)
        {
            InitializeComponent();

            ParentPage = parentPage;
            Saved = saved;
            mServiceAddress = serviceAddress;

            mAppServices = DependencyService.Get<IAppServices>();

            this.ToolbarItems.Add(new ToolbarItem()
            {
                Icon = "close_white.png",
                Command = new Command(async (t) =>
                {
                    await Navigation.PopToRootAsync(true).ConfigureAwait(false);
                }),
            });

            Position position = new Position((double)mServiceAddress.Latitude, (double)mServiceAddress.Longitude);
            LabelAddress.Text = mServiceAddress.BasicAddress.ToCannonicalString().Replace("\n", " ");

            Map.RegionChanged += (sender, e) =>
            {
                mLatitude = e.Latitude;
                mLongitude = e.Longitude;
            };

            var appServices = DependencyService.Get<IAppServices>();
            var latitude = appServices.LastLatitude;
            var longitude = appServices.LastLongitude;
            if (!latitude.Equals(0) || !longitude.Equals(0))
            {
                Map.CurrentPin = new CustomPin()
                {
                    Pin = new Pin
                    {
                        Label = AppResources.CurrentLocation,
                        Position = new Position(latitude, longitude),
                    }
                };
            }

            Map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMiles(0.5)));
            Map.Center = position;

            ImagePin.SizeChanged += (sender, e) =>
            {
                if (ImagePin.Height > 0)
                {
                    ImagePin.TranslationY = -ImagePin.Height / 2;
                }
            };
            ImagePin.IsVisible = true;

            //Map.VisibleRegion.Center;
            ButtonSave.Clicked += async (sender, e) =>
            {
                try
                {
                    this.IsBusy = true;

                    var currentAddressDetails = await mAddressFacade.GetGmsDetails(mLatitude, mLongitude);
                    ExtendedAddress currentExtendedAddress = new ExtendedAddress { BasicAddress = new ColonyConcierge.APIData.Data.Address() };
                    mAddressFacade.FillExtendedAddress(currentExtendedAddress, currentAddressDetails);
                    var zipCode = serivesService.CheckAvailableServices(currentExtendedAddress);
                    if (!string.IsNullOrEmpty(zipCode))
                    {
                        mServiceAddress.BasicAddress.ZipCode = zipCode;
                        mServiceAddress.Latitude = (decimal)mLatitude;
                        mServiceAddress.Longitude = (decimal)mLongitude;

                        if (!IsBusiness)
                        {
                            Shared.LocalAddress = mServiceAddress;
                        }
                        if (Saved != null)
                        {
                            Saved.Invoke(mServiceAddress);
                        }

                        if (ParentPage != null)
                        {
                            var pages = Navigation.NavigationStack.Reverse().Skip(1).ToList();
                            foreach (var page in pages)
                            {
                                if (page != ParentPage)
                                {
                                    Navigation.RemovePage(page);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            await Navigation.PopAsync(true).ConfigureAwait(false);
                        }
                        else
                        {
                            await Navigation.PopToRootAsync(true).ConfigureAwait(false);
                        }

                        appServices.SetNetworkBar(false);
                    }
                    else
                    {
                        var newserviceAddress = new ExtendedAddress { BasicAddress = new ColonyConcierge.APIData.Data.Address() };
                        mAddressFacade.FillExtendedAddress(newserviceAddress, currentAddressDetails);
                        var serviceNotAvailablePage = new ServiceNotAvailablePage(newserviceAddress, FullName, EmailAddress);
                        await Navigation.PushAsync(serviceNotAvailablePage);

                        //var notificator = DependencyService.Get<IToastNotificator>();
                        //await notificator.Notify(ToastNotificationType.Error, AppResources.Error, AppResources.AreaInvalid, TimeSpan.FromSeconds(2));
                    }
                }
                catch (Exception ex)
                {
                    var notificator = DependencyService.Get<IToastNotificator>();
                    await notificator.Notify(ToastNotificationType.Error, AppResources.Error, ex.Message, TimeSpan.FromSeconds(2));
                }
                finally
                {
                    (sender as VisualElement).IsEnabled = true;
                    this.IsBusy = false;
                }
            };
        }
    }
}