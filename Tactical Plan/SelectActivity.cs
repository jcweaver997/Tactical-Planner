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
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Tactical_Plan
{
    [Activity(Label = "SelectActivity", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class SelectActivity : Activity
    {
        private class ImCo
        {
            private static List<ImCo> list;
            private byte[] pica;
            private Bitmap pic;
            private string comment;
            private int imageid;
            private int addid;
            private Activity activity;
            private LinearLayout piclist;
            private ImageView iv;

            public ImCo(Activity activity, byte[] pica, string comment, int imageid)
            {
                addid = BoxThing.currentIndex;
                this.activity = activity;
                if (list == null)
                {
                    list = new List<ImCo>();
                }
                this.pica = pica;
                this.comment = comment;
                this.imageid = imageid;
                list.Add(this);
                iv = new ImageView(activity.ApplicationContext);
                iv.LayoutParameters = new ViewGroup.LayoutParams(WindowManagerLayoutParams.WrapContent, (int)(50 * this.activity.ApplicationContext.Resources.DisplayMetrics.Density));
                iv.SetScaleType(ImageView.ScaleType.FitCenter);
                piclist = activity.FindViewById<LinearLayout>(Resource.Id.linearLayout2);
                
                activity.RunOnUiThread(() => piclist.AddView(iv));
                if (pic == null) { new Task(Decode).Start(); }

            }

            private async void Decode()
            {
                pic = await BitmapFactory.DecodeByteArrayAsync(pica, 0, pica.Length);
                activity.RunOnUiThread(() => iv.SetImageBitmap(pic));
                activity.RunOnUiThread(() => iv.SetScaleType(ImageView.ScaleType.FitCenter));

            }

            public static ImCo FindImCo(int imageid)
            {
                if (list!=null)
                {
                    foreach (ImCo i in list)
                    {
                        if (i.imageid == imageid)
                        {
                            return i;
                        }
                    }
                }

                return null;
            }
            public static void Showall(Activity a)
            {
                if (list == null) return;
                foreach (ImCo i in list)
                {
                    if (i.addid == BoxThing.currentIndex)
                    {
                        i.piclist.RemoveView(i.iv);
                        i.piclist = a.FindViewById<LinearLayout>(Resource.Id.linearLayout2);
                        i.activity = a;
                        a.RunOnUiThread(() => i.piclist.AddView(i.iv));
                    }

                }

            }
        }
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
            //ImageView firstImage = FindViewById<ImageView>(Resource.Id.imageView2);
            mainImage.SetImageBitmap(bt.pic);
            //firstImage.SetImageBitmap(bt.pic);


            name.Text = bt.m_name+" - ";
            address.Text = bt.m_street;

            ActionBar.SetDisplayHomeAsUpEnabled(true);
            ImCo.Showall(this);
            new Thread(GetSpecific).Start();
            //GetSpecific();
            // Create your application here
        }

        private void GetSpecific()
        {
            Tactical_Provider.TacticalCom connection = new Tactical_Provider.TacticalCom(Tactical_Provider.TacticalCom.State.Client);
            try
            {
                connection.Start();
            }
            catch (Exception)
            {
                Console.WriteLine("Error connecting");
            }

            connection.Send(Tactical_Provider.TacticalCom.GETSPECIFIC);
            Console.WriteLine("Sent spec");
            connection.Send(BitConverter.GetBytes(BoxThing.currentIndex));
            byte[] res = connection.Recv(1);
            while (res[0] == Tactical_Provider.TacticalCom.DATA)
            {
                Console.WriteLine("recv data");
                int imageid = BitConverter.ToInt32(connection.Recv(4), 0);
                Console.WriteLine("imageid = " + imageid);
                int commentLen = BitConverter.ToInt32(connection.Recv(4), 0);
                Console.WriteLine("commentLen = " + commentLen);
                string comment = Encoding.ASCII.GetString(connection.Recv(commentLen));
                Console.WriteLine("comment = " + comment);
                int picLen = BitConverter.ToInt32(connection.Recv(4), 0);
                Console.WriteLine("picLen = " + picLen);
                byte[] pic = new byte[picLen];
                MemoryStream mem = new MemoryStream(pic);
                for (int i = 0; i < picLen / 65536 + 1; i++)
                {
                    Console.WriteLine("on " + i);
                    if (picLen >= (i+1)* 65536)
                    {

                        mem.Write(connection.Recv(65536),0, 65536);
                        Console.WriteLine("read " + (i + 1) * 65536+" len "+mem.Length);
                    }
                    else
                    {
                        
                        mem.Write(connection.Recv(picLen% 65536), 0, picLen % 65536);
                        Console.WriteLine("read " + ((i) * 65536 + picLen % 65536) + " len " + mem.Length);
                    }
                    connection.Send(Tactical_Provider.TacticalCom.DATA);
                }

                //mem.Flush();
                //mem.Read(pic,0,picLen);
                Bitmap b = BitmapFactory.DecodeByteArray(pic,0,picLen);
                RunOnUiThread(() => mainImage.SetImageBitmap(b));
                Console.WriteLine("pic = " + pic.Length + " / " + picLen + " / " + mem.Length);
                mem.Close();

                res = connection.Recv(1);
                ImCo im = ImCo.FindImCo(imageid);
                if (im == null)
                {
                    Console.WriteLine("new ");
                    im = new ImCo(this, pic, comment, imageid);

                }
            }
            Console.WriteLine("recv over");
        }
    }
}