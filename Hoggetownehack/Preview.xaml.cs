using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hoggetownehack
{
    public partial class Preview : PhoneApplicationPage
    {
        MediaLibrary _library = new MediaLibrary();
        string _img = "";
        double[] preXArray = new double[10];
        double[] preYArray = new double[10];
        public Preview()
        {
            InitializeComponent();
            Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReported);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Touch.FrameReported -= Touch_FrameReported;
        }
        void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            int pointsNumber = e.GetTouchPoints(drawCanvas).Count;
            TouchPointCollection pointCollection = e.GetTouchPoints(drawCanvas);


            for (int i = 0; i < pointsNumber; i++)
            {
                if (pointCollection[i].Action == TouchAction.Down)
                {
                    preXArray[i] = pointCollection[i].Position.X;
                    preYArray[i] = pointCollection[i].Position.Y;
                }
                if (pointCollection[i].Action == TouchAction.Move)
                {
                    Line line = new Line();


                    line.X1 = preXArray[i];
                    line.Y1 = preYArray[i];
                    line.X2 = pointCollection[i].Position.X;
                    line.Y2 = pointCollection[i].Position.Y;


                    line.Stroke = new SolidColorBrush(Colors.Red);
                    line.Fill = new SolidColorBrush(Colors.Red);
                    line.StrokeThickness = 5;
                    drawCanvas.Children.Add(line);


                    preXArray[i] = pointCollection[i].Position.X;
                    preYArray[i] = pointCollection[i].Position.Y;
                }
            }
        } 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            byte[] data=null;          

            NavigationContext.QueryString.TryGetValue("img", out _img);

            using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            {  
                using (IsolatedStorageFileStream isoStream = isStore.OpenFile("0.jpg", FileMode.Open, FileAccess.Read))
                {
                    data = new byte[isoStream.Length];
                    // Read the entire file and then close it 
                    isoStream.Read(data, 0, data.Length);
                    isoStream.Close(); 
                }
            }

            // Create memory stream and bitmap 
            MemoryStream ms = new MemoryStream(data);
            BitmapImage bi = new BitmapImage();
            // Set bitmap source to memory stream 
            bi.SetSource(ms);
            ImagePreview.Height = 300;
            ImagePreview.Width = 400;
            ImagePreview.Source = bi;


            RotateTransform MyTransform = new RotateTransform();
            MyTransform.Angle = 90;
            ImagePreview.RenderTransform = MyTransform;

            ImagePreview.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Description.xaml?img=" + _img , UriKind.Relative));
        }
    }
}