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

namespace Tactical_Plan
{
    [Activity(Label = "SelectActivity", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class SelectActivity : Activity
    {

        private ImageView mainImage;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SelectLayout);

            BoxThing bt = BoxThing.Findboxthing(BoxThing.currentIndex);
            TextView name = FindViewById<TextView>(Resource.Id.textView1);
            TextView address = FindViewById<TextView>(Resource.Id.textView2);
            EditText comments = FindViewById<EditText>(Resource.Id.editText1);
            comments.SetBackgroundColor(new Android.Graphics.Color(63, 63, 63));
            comments.Text = "test text";
            comments.Enabled = false;
            mainImage = FindViewById<ImageView>(Resource.Id.imageView1);
            ImageView firstImage = FindViewById<ImageView>(Resource.Id.imageView2);
            mainImage.SetImageBitmap(bt.pic);
            firstImage.SetImageBitmap(bt.pic);


            name.Text = bt.m_name+" - ";
            address.Text = bt.m_street;

            ActionBar.SetDisplayHomeAsUpEnabled(true);

            GetSpecific();
            // Create your application here
        }

        private void GetSpecific()
        {

        }
    }
}