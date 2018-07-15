using AsystBussiness.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Controls
{
    public class CustomCell : ViewCell
    {
        public bool IsNeedResize
        {
            get;
            set;
        }

        public bool TransparentHover
        {
            get;
            set;
        } = false;

        public Color ColorHover
        {
            get;
            set;
        } = Color.Transparent;

        public bool IsColorHover
        {
            get
            {
                return ColorHover != Color.Transparent;
            }
        }

        public CustomCell()
        {
            try
            {
                this.ForceUpdateSize();

                BindingForceUpdateSize();
                this.BindingContextChanged += (sender, e) =>
                {
                    BindingForceUpdateSize();
                };
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        private void BindingForceUpdateSize()
        {
            try
            {
                if (this.BindingContext is INotifyPropertyChanged)
                {
                    (this.BindingContext as INotifyPropertyChanged).PropertyChanged += (sender2, e2) =>
                    {
                        if (e2.PropertyName == "ForceUpdateSize")
                        {
                            this.ForceUpdateSize();
                        }
                    };
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
