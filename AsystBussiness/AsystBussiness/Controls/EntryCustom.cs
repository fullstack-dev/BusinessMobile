using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Controls
{
    public class EntryCustom : Entry
    {
        private bool mNeedShowKeyboard = false;
        public bool NeedShowKeyboard
        {
            get
            {
                return mNeedShowKeyboard;
            }
            set
            {
                mNeedShowKeyboard = value;
                OnPropertyChanged(nameof(NeedShowKeyboard));
            }
        }
    }
}
