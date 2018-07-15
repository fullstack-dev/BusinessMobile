using AsystBussiness.Core;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsystBussiness.Adapters
{
    class OrderAdapterModel : NotifyPropertyChanged
    {
        string _time;
        public string Time
        {
            get { return _time; }
            set { SetField(ref _time, value); }
        }

        string _deliveryCount;
        public string DeliveryCount
        {
            get { return _deliveryCount; }
            set { SetField(ref _deliveryCount, value); }
        }

        string _pickupCount;
        public string PickupCount
        {
            get { return _pickupCount; }
            set { SetField(ref _pickupCount, value); }
        }

        bool _isDeliveryVisible;
        public bool IsDeliveryVisible
        {
            get { return _isDeliveryVisible; }
            set { SetField(ref _isDeliveryVisible, value); }
        }

        bool _isPickupVisible;
        public bool IsPickupVisible
        {
            get { return _isPickupVisible; }
            set { SetField(ref _isPickupVisible, value); }
        }

        public OrderAdapterModel(string time, int deliveryCount, int pickupCount)
        {
            Time = time;

            DeliveryCount = deliveryCount.ToString();
            PickupCount = pickupCount.ToString();

            if (deliveryCount > 0)
                IsDeliveryVisible = true;
            else
                IsDeliveryVisible = false;

            if (pickupCount > 0)
                IsPickupVisible = true;
            else
                IsPickupVisible = false;
        }
    }
}
