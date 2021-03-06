﻿using AsystBussiness.Localization.Resx;
using ColonyConcierge.APIData.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.ViewModels
{
    public class CreditCardDataItemViewModel : BindableObject
    {
        public CreditCardData Model
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
            }
        }

        //public CreditCardType CardType
        //{
        //	get;
        //	set;
        //}

        public string CreditCardNumber
        {
            get
            {
                if (Model == null || string.IsNullOrEmpty(Model.CreditCardNumber))
                {
                    return string.Empty;
                }
                var index = Model.CreditCardNumber.IndexOf("*", StringComparison.OrdinalIgnoreCase);
                if (index < Model.CreditCardNumber.Length - 1)
                {
                    return Model.CreditCardNumber.Substring(index + 1);
                }
                return string.Empty;
            }
        }

        public string Preferred
        {
            get
            {
                return Model.IsPreferred ? AppResources.Preferred : string.Empty;
            }
        }


        public CreditCardDataItemViewModel(CreditCardData creditCardData)
        {
            Model = creditCardData;
            //CardType = DependencyService.Get<IAppServices>().GetCreditCardType(creditCardData.CreditCardNumber.Replace("*", "").Replace(" ", ""));
        }
    }
}
