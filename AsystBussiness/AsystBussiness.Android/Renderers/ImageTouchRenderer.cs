using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using AsystBussiness.Controls;
using AsystBussiness.Droid.Renderers;
using AsystBussiness.Core;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(ImageTouch), typeof(ImageTouchRenderer))]
namespace AsystBussiness.Droid.Renderers
{
    public class ImageTouchRenderer : ImageRenderer
    {
        ImageTouch ImageTouch;

        void Control_Touch(object sender, TouchEventArgs e)
        {
            try
            {
                if (ImageTouch != null)
                {
                    switch (e.Event.Action)
                    {
                        case Android.Views.MotionEventActions.Pointer1Down:
                        case Android.Views.MotionEventActions.Down:
                            ImageTouch.RaiseOnTouch(true);
                            break;
                        case Android.Views.MotionEventActions.Up:
                        case Android.Views.MotionEventActions.Pointer1Up:
                        case Android.Views.MotionEventActions.Cancel:
                            ImageTouch.RaiseOnTouch(false);
                            break;
                    }
                }
            }
            catch (Exception e1)
            {
                ExceptionHandler.Catch(e1);
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Image> e)
        {
            try
            {
                base.OnElementChanged(e);

                if (e.OldElement != null)
                {
                    Control.Touch -= Control_Touch;
                }
                if (e.NewElement != null)
                {
                    ImageTouch = (ImageTouch)e.NewElement;
                    Control.Touch += Control_Touch;
                }
            }
            catch (Exception e1)
            {
                ExceptionHandler.Catch(e1);
            }
        }
    }
}