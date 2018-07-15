using AsystBussiness.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AsystBussiness.Controls
{
    public class ImageTouch : Image
    {
        public event EventHandler<bool> Touch;

        public void RaiseOnTouch(bool isTouch)
        {
            try
            {
                if (Touch != null)
                {
                    Touch(this, isTouch);
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.Catch(e);
            }
        }
    }
}
