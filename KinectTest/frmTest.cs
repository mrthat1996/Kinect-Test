using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KinectTest
{
    public partial class frmTest : Form
    {
        private KinectSensor kinectSensor = null;
        private MultiSourceFrameReader multiSourceFrameReader = null;

        public frmTest()
        {
            InitializeComponent();
        }

        private void frmTest_Load(object sender, EventArgs e)
        {
            kinectSensor = KinectSensor.GetDefault();
            if (kinectSensor != null)
            {
                kinectSensor.Open();
            }
            cbType.Text = "color";
        }

        private void MultiSourceFrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            if (cbType.Text == "color")
            {
                processColorFrame(e);
            }
            else if (cbType.Text == "depth")
            {
                processDepthFrame(e);
            }
        }

        private void processColorFrame(MultiSourceFrameArrivedEventArgs e)
        {
            var refFrame = e.FrameReference.AcquireFrame();
            using (var frame = refFrame.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var width = frame.FrameDescription.Width;
                    var height = frame.FrameDescription.Height;
                    var data = new byte[width * height * 4];
                    frame.CopyConvertedFrameDataToArray(data, ColorImageFormat.Bgra);
                    var bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
                    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);
                    bitmap.UnlockBits(bitmapData);
                    pbPreview.Image = bitmap;
                }
            }
        }

        private void processDepthFrame(MultiSourceFrameArrivedEventArgs e)
        {
            var refFrame = e.FrameReference.AcquireFrame();
            using (DepthFrame frame = refFrame.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var width = frame.FrameDescription.Width;
                    var height = frame.FrameDescription.Height;
                    var data = new ushort[width * height];
                    frame.CopyFrameDataToArray(data);

                    int minDepth = frame.DepthMinReliableDistance;
                    int maxDepth = frame.DepthMaxReliableDistance;

                    int colorPixelIndex = 0;
                    var newData = new byte[width * height * 4];
                    for (int i = 0; i < data.Length; ++i)
                    {
                        ushort depth = data[i];
                        byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                        // Blue
                        newData[colorPixelIndex++] = intensity;

                        // Green
                        newData[colorPixelIndex++] = intensity;

                        // Red
                        newData[colorPixelIndex++] = intensity;

                        // Alpha
                        newData[colorPixelIndex++] = 100;
                    }
                    var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    Marshal.Copy(newData, 0, bitmapData.Scan0, newData.Length);
                    bitmap.UnlockBits(bitmapData);
                    pbPreview.Image = bitmap;
                }
            }
        }

        private void frmTest_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (kinectSensor != null)
            {
                kinectSensor.Close();
                kinectSensor = null;
            }

            if (multiSourceFrameReader != null)
            {
                multiSourceFrameReader.Dispose();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbType.Text == "color")
            {
                multiSourceFrameReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
            }
            else if (cbType.Text == "depth")
            {
                multiSourceFrameReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth);
            }
            multiSourceFrameReader.MultiSourceFrameArrived -= MultiSourceFrameReader_MultiSourceFrameArrived;
            multiSourceFrameReader.MultiSourceFrameArrived += MultiSourceFrameReader_MultiSourceFrameArrived;
        }
    }
}
