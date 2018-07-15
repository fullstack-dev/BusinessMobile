using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.ViewModels
{
    public class DateTimeViewModel : BindableObject
    {
        public string DateTime
        {
            get;
            set;
        }

        private bool mIsSelected = false;
        public bool IsSelected
        {
            get
            {
                return mIsSelected;
            }
            set
            {
                OnPropertyChanging(nameof(IsSelected));
                mIsSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(TextColor));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public Color TextColor
        {
            get
            {
                return IsSelected ? AppearanceBase.Instance.PrimaryColor : Color.Gray;
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return IsSelected ? Color.FromRgb(223, 251, 216) : Color.White;
            }
        }

        public DateTimeViewModel(string dateTime)
        {
            DateTime = dateTime;
        }
    }
}
