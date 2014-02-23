using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;

namespace Hoggetownehack
{
    public partial class Description : PhoneApplicationPage
    {
        Geolocator geolocator = null;
        public Description()
        {
            InitializeComponent();
            geolocator = new Geolocator();
            GeoLoc();
        }


        private async void GeoLoc()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            StatusTextBlock.Text = "Loading Co-ordinates..";
            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                     maximumAge: TimeSpan.FromMinutes(5),
                     timeout: TimeSpan.FromSeconds(10)
                    );

                //With this 2 lines of code, the app is able to write on a Text Label the Latitude and the Longitude, given by {{Icode|geoposition}}
                LatitudeTextBlock.Text += geoposition.Coordinate.Latitude.ToString("0.00");
                LongitudeTextBlock.Text += geoposition.Coordinate.Longitude.ToString("0.00");
                StatusTextBlock.Text = "Current Co-ordinates";
                DisplayRestForm();
            }
            //If an error is catch 2 are the main causes: the first is that you forgot to include ID_CAP_LOCATION in your app manifest. 
            //The second is that the user doesn't turned on the Location Services
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    StatusTextBlock.Text = "location is disabled in phone settings.";
                }
                //else
                {
                    // something else happened during the acquisition of the location
                }
            }
        }

        private void DisplayRestForm()
        {
            severity.Visibility = Visibility.Visible;
            impact.Visibility = Visibility.Visible;
            ImpactSlider.Visibility = Visibility.Visible;
            SeveritySlider.Visibility = Visibility.Visible;
            SubmitButton.Visibility = Visibility.Visible;
            desc.Visibility = Visibility.Visible;
            CategoryDropDown.Visibility = Visibility.Visible;
            TypeOfReportBlock.Visibility = Visibility.Visible;
        }

        private void SeveritySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (severity != null)
            {
                if (e.NewValue < 2.0)
                    severity.Text = "Severity: Low";
                else if (e.NewValue < 3.0)
                    severity.Text = "Severity: Medium";
                else if (e.NewValue >= 3.0)
                    severity.Text = "Severity: High";
            }
        }

        private void ImpactSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (impact != null)
            {
                if (e.NewValue < 10.0)
                    impact.Text = "Impacts: Household";
                else if (e.NewValue < 50.0)
                    impact.Text = "Impacts: Neighbourhood";
                else if (e.NewValue >= 50.0)
                    impact.Text = "Impacts: Everyone";
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string ipAdd = "http://192.168.1.26/report.php?description='" + desc.Text + "'&category='" + ((System.Windows.Controls.ContentControl)(CategoryDropDown.SelectedItem)).Content + "'&severity='" + severity.Text.Replace("Severity: ", "") + "'&impact_count='" + impact.Text.Replace("Impacts: ", "") + "'&reporter_name='Shashank'&reporter_phone='352-xxx-xxxx'&latitude='" + LatitudeTextBlock.Text.Replace("Lat: ", "") + "'&longitude='" + LongitudeTextBlock.Text.Replace("Lon: ", "") + "'";

            string img = "";
            byte[] data = null;

            if (NavigationContext.QueryString.TryGetValue("img", out img)) ;

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
            string base64 = System.Convert.ToBase64String(ms.ToArray());



            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ipAdd);
            request.BeginGetResponse(GetReportCallback, request);

            request = (HttpWebRequest)HttpWebRequest.Create("http://192.168.1.26/upload.php");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string postData = String.Format("image={0}", base64);
            request.BeginGetRequestStream
                (result =>
                {
                    // Sending the request.
                    using (var requestStream = request.EndGetRequestStream(result))
                    {
                        using (StreamWriter writer = new StreamWriter(requestStream))
                        {
                            writer.Write(postData);
                            writer.Flush();
                        }
                    }

                    // Getting the response.
                    request.BeginGetResponse(responseResult =>
                    {
                        var webResponse = request.EndGetResponse(responseResult);
                        using (var responseStream = webResponse.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                string srresult = streamReader.ReadToEnd();
                            }
                        }
                    }, null);
                }, null);
        }
        //request.BeginGetResponse(GetReportCallback, request);
    
    void GetReportCallback(IAsyncResult result)
    {
        HttpWebRequest request = result.AsyncState as HttpWebRequest;
        if (request != null)
        {
            try
            {
                WebResponse response = request.EndGetResponse(result);
                //confirmation.Text = "Report successfully submitted";
            }
            catch (WebException e)
            {
               // confirmation.Text = "Report couldnot be submitted try again";
                return;
            }
        }
    }
     }


}