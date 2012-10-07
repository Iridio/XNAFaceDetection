using System;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace WindowsGame1
{
    public class VideoEmgu
    {
        GraphicsDevice device;
        Texture2D frame;
        Capture capture;
        Image<Bgr, byte> nextFrame;
        ThreadStart thread;
        public bool IsRunning;
        public Color[] colorData;

        private Image<Gray, byte> gray = null;
        private HaarCascade haarCascade = new HaarCascade("haarcascade_frontalface_default.xml");
        private MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        
        public Texture2D Frame
        {
            get
            {
                if (frame.GraphicsDevice.Textures[0] == frame)
                    frame.GraphicsDevice.Textures[0] = null;
                frame.SetData<Color>(0, null, colorData, 0, colorData.Length);
                return frame;
            }
        }
        
        public VideoEmgu(GraphicsDevice device)
        {           
            this.device = device;
            capture = new Capture();
            frame = new Texture2D(device, capture.Width, capture.Height);
            colorData = new Color[capture.Width * capture.Height];
        }

        public void Start()
        {
            thread = new ThreadStart(QueryFrame);
            IsRunning = true;
            thread.BeginInvoke(null, null);
        }

        public void Dispose()
        {
            IsRunning = false;
            capture.Dispose();
        }
        
        private void QueryFrame()
        {
            while (IsRunning)
            {
                nextFrame = capture.QueryFrame().Flip(FLIP.HORIZONTAL);
                if (nextFrame != null)
                {
                    gray = nextFrame.Convert<Gray, Byte>();
                    MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(haarCascade, 1.2, 2, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new System.Drawing.Size(40, 40));
                    foreach (MCvAvgComp face in facesDetected[0])
                        nextFrame.Draw(face.rect, new Bgr(System.Drawing.Color.White), 2);
                    byte[] bgrData = nextFrame.Bytes;
                    for (int i = 0; i < colorData.Length; i++)
                        colorData[i] = new Color(bgrData[3 * i + 2], bgrData[3 * i + 1], bgrData[3 * i]);
                }
            }
        }
    }
}
