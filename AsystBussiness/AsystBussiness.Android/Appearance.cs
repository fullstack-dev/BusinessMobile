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

using Android.Graphics;

namespace AsystBussiness.Droid
{
    public class Appearance : AppearanceBase
    {
        public new static Appearance Instance
        {
            get
            {
                return AppearanceBase.Instance as Appearance;
            }
        }

        public static void Init()
        {
            if (mInstance == null)
            {
                var appearance = new Appearance();
                mInstance = appearance;
            }
        }

        Typeface mTypeface;
        public Typeface TypefaceDefault
        {
            get
            {
                if (mTypeface == null)
                {
                    mTypeface = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, FontFileNameDefault);
                }
                return mTypeface;
            }
        }

        Typeface mTypefaceBold;
        public Typeface TypefaceBold
        {
            get
            {
                if (mTypefaceBold == null)
                {
                    mTypefaceBold = Typeface.CreateFromAsset(Android.App.Application.Context.Assets, FontFileNameBold);
                }
                return mTypefaceBold;
            }
        }

        private Appearance() : base()
        {
        }

        protected override void OnConfigure()
        {
            base.OnConfigure();
        }

    }
}