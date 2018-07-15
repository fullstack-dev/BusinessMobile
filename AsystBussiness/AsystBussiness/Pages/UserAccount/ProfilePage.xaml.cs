using AsystBussiness.Core;
using AsystBussiness.Localization.Resx;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AsystBussiness.Pages.UserAccount
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProfilePage : ContentPageBase
    {
        public ProfilePage()
        {
            InitializeComponent();

            NavigationPage.SetBackButtonTitle(this, AppResources.Back);

            ActivityIndicatorProfile.IsVisible = true;
        }

        private bool mFirstLoad = true;
        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (mFirstLoad)
            {
                mFirstLoad = false;
                LoadData();
            }
            else
            {
                LoadData(true);
            }
        }

        List<int> _locationIds = new List<int>();
        List<string> _locationNames = new List<string>();

        public void LoadData(bool isRefresh = false)
        {
            User userModel = null;
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();
            

            if (Shared.IsLoggedIn)
            {
                Task.Run(async () =>
                {
                    Utils.IReloadPageCurrent = this;
                    try
                    {
                        userModel = await Shared.APIs.IUsers.GetCurrentUser_Async();
                        phoneNumbers = await Shared.APIs.IUsers.GetPhoneNumbers_Async(Shared.UserId);
                        _locationIds = await Shared.APIs.IRestaurantBusiness.GetLocationIds_Async();

                        if (_locationIds != null)
                        {
                            foreach (var id in _locationIds)
                            {
                                var location = await Shared.APIs.IRestaurant.GetLocation_Async(id);
                                var restaurant = await Shared.APIs.IRestaurant.GetRestaurant_Async(location.RestaurantID);
                                _locationNames.Add(restaurant.DisplayName);
                            }
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        if (this.IsErrorPage)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                Utils.ShowErrorMessage(ex);
                            });
                        }
                    }
                    if (Utils.IReloadPageCurrent == this)
                    {
                        Utils.IReloadPageCurrent = null;
                    }
                }).ContinueWith((obj) =>
                {
                    if (!this.IsErrorPage)
                    {
                        if (userModel != null)
                        {
                            this.Name.Text = userModel.FirstName + " " + userModel.LastName;
                            this.Email.Text = userModel.EmailAddress;
                        }
                        if (phoneNumbers != null)
                        {
                            var mobile = phoneNumbers.Find(x => x.Type == "Mobile");
                            this.Phone.Text = mobile.Number;
                            StackLayoutProfile.Opacity = 1;
                            ActivityIndicatorProfile.IsVisible = false;
                        }

                        if (_locationNames != null && _locationNames.Count > 0)
                        {
                            RestaurantPicker.ItemsSource = _locationNames;

                            if (string.IsNullOrEmpty(Shared.RestaurantLocation))
                                RestaurantPicker.SelectedItem = RestaurantPicker.ItemsSource[0];
                            else
                            {
                                var item = _locationIds.First(x => x.Equals(int.Parse(Shared.RestaurantLocation)));
                                var index = _locationIds.IndexOf(item);
                                RestaurantPicker.SelectedItem = RestaurantPicker.ItemsSource[index];
                            }

                            RestaurantPicker.SelectedIndexChanged += RestaurantPicker_SelectedIndexChanged;
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                StackLayoutProfile.Opacity = 1;
                ActivityIndicatorProfile.IsVisible = false;
            }
        }

        private void RestaurantPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = RestaurantPicker.SelectedIndex;
            Shared.RestaurantLocation = _locationIds[index].ToString();

        }
    }
}