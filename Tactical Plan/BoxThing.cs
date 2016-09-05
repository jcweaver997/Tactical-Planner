using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Tactical_Plan
{
    public class BoxThing
    {
        public string m_name, m_street;
        public Bitmap pic;
        private TextView street, zip, name;
        private ImageView iv, divider;
        private byte[] pica;
        private int id;
        private LinearLayout outerv, outerh, innerv, innerh;
        private static List<BoxThing> boxlist;
        private Activity activity;
        private ViewGroup parent;
        private bool showing = false;
        public BoxThing(Activity activity, ViewGroup parent, List<BoxThing> boxlist, string name, string street, int zip, int id, byte[] pica)
        {
            if (BoxThing.boxlist == null){BoxThing.boxlist = boxlist;}
            m_name = name;
            m_street = street;
            this.id = id;
            this.pica = pica;
            this.activity = activity;
            this.parent = parent;

            

            outerh = new LinearLayout(this.activity.ApplicationContext);
            outerh.Orientation = Orientation.Horizontal;
            outerh.SetVerticalGravity(GravityFlags.Center);

            innerv = new LinearLayout(this.activity.ApplicationContext);
            innerv.Orientation = Orientation.Vertical;

            innerh = new LinearLayout(this.activity.ApplicationContext);
            innerh.Orientation = Orientation.Horizontal;

            int padding = (int)(5 * this.activity.ApplicationContext.Resources.DisplayMetrics.Density);

            iv = new ImageView(this.activity.ApplicationContext);
            iv.SetImageBitmap(this.pic);
            iv.LayoutParameters = new ViewGroup.LayoutParams(padding*20,padding*20);
            iv.SetScaleType(ImageView.ScaleType.FitCenter);
            outerh.AddView(iv);

            if (pic == null) { new Task(Decode).Start(); }

            this.name = new TextView(this.activity.ApplicationContext);
            this.name.Text = name;
            this.name.SetTextSize(Android.Util.ComplexUnitType.Sp,24);
            this.name.SetPadding(padding*3, 0, 0, padding*3);

            innerv.AddView(this.name);

            this.street = new TextView(this.activity.ApplicationContext);
            this.street.Text = street;
            this.street.SetPadding(padding * 3, 0, 0, padding*3);
            innerh.AddView(this.street);

            this.zip = new TextView(this.activity.ApplicationContext);
            this.zip.SetPadding(padding, 0, padding, padding * 3);
            this.zip.Text = zip + "";
            innerh.AddView(this.zip);

            innerv.AddView(innerh);
            outerh.AddView(innerv);

            outerh.SetPadding(padding, padding, padding, padding);
            outerh.SetBackgroundColor(new Android.Graphics.Color(63, 63, 63));

            divider = new ImageView(this.activity.ApplicationContext);
            divider.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, padding * 2);

            outerv = new LinearLayout(this.activity.ApplicationContext);
            outerv.Orientation = Orientation.Vertical;
            outerv.AddView(outerh);
            outerv.AddView(divider);

            boxlist.Add(this);

            outerv.Click += onClick;
        }

        public static int currentIndex { get; set; }
        private void onClick( object sender, EventArgs e)
        {
            currentIndex = id;
            activity.StartActivity(typeof(SelectActivity));
        }


        public static BoxThing Findboxthing(int id)
        {
            if (boxlist == null)
            {
                return null;
            }
            foreach (BoxThing b in boxlist)
            {
                if (b.id == id)
                {
                    return b;
                }
            }
            return null;
        }

        private async void Decode()
        {
            pic = await BitmapFactory.DecodeByteArrayAsync(pica, 0, pica.Length);
            iv.SetImageBitmap(pic);
        }

        public void Show()
        {
            if (showing == false)
            {
                activity.RunOnUiThread(() => parent.AddView(outerv));
            }
            showing = true;
        }

        public void Remove()
        {
            if (showing == true)
            {
                activity.RunOnUiThread(() => parent.RemoveView(outerv));
            }
            showing = false;
        }

    }
}