using Bing.Maps;
using Event_Finder.ViewModel;
using Facebook.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Event_Finder.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private FacebookSession session;
        public LoginPage()
        {
            this.InitializeComponent();
            
        }
        private async Task<bool> Authenticate()
        {
            String errorMessage = "";
            Boolean errorOccured = false;
            MessageDialog dialog = new MessageDialog("");
            string message = String.Empty;
            try
            {
                session = await App.FacebookSessionClient.LoginAsync("user_about_me, read_stream, user_events, user_friends, friends_events, rsvp_event");
                App.AccessToken = session.AccessToken;
                App.FacebookId = session.FacebookId;
            }
            catch (InvalidOperationException e)
            {
                errorMessage = "Login failed! Exception details:" + e.Message;
               
                btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                errorOccured = true;
            } if (errorOccured) 
            {
                dialog.Content = errorMessage;
                try
                {
                    await dialog.ShowAsync();
                    return false;
                }
                catch (Exception) { }
            }

            return false;
        }

        async private void btnFacebookLogin_Click(object sender, RoutedEventArgs e)
        {
            App.myPosition = await App.commonApiHandler.lController.GetCurrentLocation();
            App.myLocation = new Location(App.myPosition.Coordinate.Point.Position.Latitude, App.myPosition.Coordinate.Point.Position.Longitude);
            bool error = false ;
            if (!App.isAuthenticated)
            {
                try
                {

                    btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    error = await Authenticate();
                    App.isAuthenticated = error;
                    if (!error)
                    { 
                        Frame.Navigate(typeof(MainPage));
                    }
                }
                catch (Exception ) 
                {
                    App.isAuthenticated = false;
                    btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                
                }
            }
        }

        async protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

           
            // get list of atteneded events by user.
            App.commonApiHandler.FillAttendedEventsByUserInCollection(await App.commonApiHandler.facebookApi.getListOfEventsAttendedByUser(
                DateTimeConverter.DateTimeToUnixTimestamp(App.startRange),
                DateTimeConverter.DateTimeToUnixTimestamp(App.endRange)));

            // QueryForEventsWithinAnArea
            String error = await App.commonApiHandler.QueryForEventsWithinAnArea(App.offset, DateTimeConverter.DateTimeToUnixTimestamp(App.startRange),
                DateTimeConverter.DateTimeToUnixTimestamp(App.endRange));

            if (error != null)
            {
                App.errorOccured = true;
                App.errorMessage = error;
            }


           

        }
    }
}
