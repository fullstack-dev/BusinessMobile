using AsystBussiness.Core;
using AsystBussiness.Facades;
using ColonyConcierge.APIData.Data;
using ColonyConcierge.Client.PlatformServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Pages.ScheduleDelivery
{
    class DeliveryPageModel : NotifyPropertyChanged
    {
        public string ZipCode { get; private set; }
        public bool IsAutoSuggestionsEnabled { get; private set; } = true;

        ObservableCollection<Prediction> _places;
        public ObservableCollection<Prediction> Places
        {
            get { return _places; }
            set { SetField(ref _places, value); }
        }

        int _autoSuggestionListHeight;
        public int AutoSuggestionListHeight
        {
            get { return _autoSuggestionListHeight; }
            set { SetField(ref _autoSuggestionListHeight, value); }
        }

        bool _isAutoSuggestionsVisible = false;
        public bool IsAutoSuggestionsVisible
        {
            get { return _isAutoSuggestionsVisible; }
            set { SetField(ref _isAutoSuggestionsVisible, value); }
        }

        bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetField(ref _isBusy, value); }
        }

        bool _isNextStepEnabled = false;
        public bool IsNextStepEnabled
        {
            get { return _isNextStepEnabled; }
            set { SetField(ref _isNextStepEnabled, value); }
        }

        private static DeliveryPageModel _singleton;
        public static DeliveryPageModel GetSingleton()
        {
            if (_singleton == null)
                _singleton = new DeliveryPageModel();
                
            return _singleton;
        }

        INavigation _navigation;

        public void Initialize(INavigation navigation, string zipCode)
        {
            _navigation = navigation;
            this.ZipCode = zipCode;
            UpdateTimesAsync();
        }

        public async void FillAutoSuggestAsync(string text)
        {
            if (!IsAutoSuggestionsEnabled)
                return;

            var guid = Async.GenerateGUID();
            var searchPlaces = await new AddressFacade().SearchPlaces(text);
            if (!Async.CompareGUID(guid))
                return;

            if (searchPlaces == null || searchPlaces.Count == 0)
            {
                this.Places = new ObservableCollection<Prediction>();
                IsAutoSuggestionsVisible = false;
            }
            else
            {
                this.Places = new ObservableCollection<Prediction>(searchPlaces.Take(3).ToList());
                IsAutoSuggestionsVisible = true;
            }

            AutoSuggestionListHeight = (this.Places.Count * 50) + 10;
        }

        public async void HandleItemSelected(Prediction prediction)
        {
            IsAutoSuggestionsEnabled = false;
            this.DeliveryAddress = prediction.Description;
            IsAutoSuggestionsVisible = false;

            await Task.Yield();
            IsAutoSuggestionsEnabled = true;
        }

        async void UpdateTimesAsync()
        {
            if (!IsBusy)
            {
                try
                {
                    IsBusy = true;

                    var locId = Shared.RestaurantLocation;

                    var availableTimes = await Shared.APIs.IRestaurant.GetLocationAvailableOrderTimes_Async(locId, (TimeStamp)this.DeliveryDate, (TimeStamp)this.DeliveryDate.AddDays(1));

                    AvailableDeliveryTimes.Clear();
                    if (availableTimes.Count > 0)
                    {
                        availableTimes = availableTimes.Where(w => w.IsDelivery == true).ToList();
                        AvailableDeliveryTimes = availableTimes.GroupBy(x => x.StartTime.Time).Select(s => DateTime.Parse(s.First().StartTime.Time).ToString("hh:mm tt")).ToList();
                    }

                    if (AvailableDeliveryTimes.Count == 0)
                    {
                        AvailableDeliveryTimes = new List<string>() { "N/A" };
                    }

                    await Task.Yield();
                    DeliveryTime = AvailableDeliveryTimes.First();
                }
                catch (Exception e)
                {
                    Crashlytics.Log(e);
                }

                IsBusy = false;
            }          
        }

        private DeliveryPageModel()
        {
            _singleton = this;
        }

        DateTime _deliveryDate = DateTime.UtcNow;
        public DateTime DeliveryDate
        {
            get { return _deliveryDate; }
            set
            {
                SetField(ref _deliveryDate, value);

                ValidateData();
                UpdateTimesAsync();
            }
        }

        List<string> _availableDeliveryTimes = new List<string>();
        public List<string> AvailableDeliveryTimes
        {
            get { return _availableDeliveryTimes; }
            set { SetField(ref _availableDeliveryTimes, value); ValidateData(); }
        }

        string _deliveryTime;
        public string DeliveryTime
        {
            get { return _deliveryTime; }
            set { SetField(ref _deliveryTime, value); ValidateData(); }
        }

        string _deliveryName;
        public string DeliveryName
        {
            get { return _deliveryName; }
            set { SetField(ref _deliveryName, value); ValidateData(); }
        }

        string _deliveryPhone;
        public string DeliveryPhone
        {
            get { return _deliveryPhone; }
            set { SetField(ref _deliveryPhone, value); ValidateData(); }
        }

        string _deliveryAddress;
        public string DeliveryAddress
        {
            get { return _deliveryAddress; }
            set { SetField(ref _deliveryAddress, value); ValidateData(); }
        }

        void ValidateData()
        {
            if (string.IsNullOrEmpty(DeliveryAddress))
                IsNextStepEnabled = false;

            else if (string.IsNullOrEmpty(DeliveryPhone))
                IsNextStepEnabled = false;

            else if (string.IsNullOrEmpty(DeliveryTime) || DeliveryTime == "N/A")
                IsNextStepEnabled = false;

            else
                IsNextStepEnabled = true;
        }

        public Command NavigateToPreviousStep
        {
            get
            {
                return new Command(() =>
                {
                    //Utils.PushAsync(_navigation, new ZipPage());
                });
            }
        }

        public Command NavigateToNextStep
        {
            get
            {
                return new Command(() =>
                {
                    Utils.PushAsync(_navigation, new OrderingPage(DeliveryAddress, ZipCode, DeliveryDate, DeliveryTime));
                });
            }
        }
    }
}
