using Bing.Maps;
using Event_Finder.ViewModel;
using Facebook.Client;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Event_Finder.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        
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
               
                App.session = await App.FacebookSessionClient.LoginAsync("user_about_me, read_stream, user_events, user_friends, friends_events, rsvp_event");
               
                App.AccessToken = App.session.AccessToken;
                App.FacebookId = App.session.FacebookId;
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
                    return true;
                }
                catch (Exception) { }
            }

            return false;
        }

        async private void btnFacebookLogin_Click(object sender, RoutedEventArgs e)
        {
          
            bool error = false ;
            if (!App.isAuthenticated)
            {

                btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                error = await Authenticate();
                App.isAuthenticated = !error;
                if (App.isAuthenticated)
                {
                    Frame.Navigate(typeof(MainPage));
                }
                else
                {
                    App.isAuthenticated = false;
                    btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            else {
                Frame.Navigate(typeof(MainPage));
            }
        }

        async protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
                App.myPosition = await App.commonApiHandler.lController.GetCurrentLocation();
                App.ErrorOccuredFinished.TrySetResult(false);
            }
            catch (System.UnauthorizedAccessException) { App.errorOccured = true; 
                                                            App.errorMessage = "Could not find your location. Please set your location on the map using app bar.";
                                                            App.ErrorOccuredFinished.TrySetResult(true);
                                                            return; }
            catch (System.Exception) { App.errorOccured = true; 
                                                    App.errorMessage = "Could not find your location. Please set your location on the map using app bar.";
                                                    App.ErrorOccuredFinished.TrySetResult(true);   
                                                    return; }
            App.myLocation = new Location(App.myPosition.Coordinate.Point.Position.Latitude, App.myPosition.Coordinate.Point.Position.Longitude);
            App.GettingPositionFinished.SetResult(true);
            // get list of atteneded events by user.by 
            String error = await App.commonApiHandler.QueryForUserEvents();

            // QueryForEventsWithinAnArea
            error = await App.commonApiHandler.QueryForEventsWithinAnArea(App.offset, DateTimeConverter.DateTimeToUnixTimestamp(App.startRange),
                DateTimeConverter.DateTimeToUnixTimestamp(App.endRange));

            if (error != null)
            {
                App.errorOccured = true;
                App.errorMessage = error;
                App.ErrorOccuredFinished.TrySetResult(true);
            }

           

        }

        private void loginButton_SessionStateChanged(object sender, Facebook.Client.Controls.SessionStateChangedEventArgs e)
        {
            Facebook.Client.Controls.LoginButton x = (Facebook.Client.Controls.LoginButton)sender;
            if (e.SessionState == Facebook.Client.Controls.FacebookSessionState.Opened) {
                App.session = x.CurrentSession;
                App.AccessToken = x.CurrentSession.AccessToken;
                App.FacebookId = x.CurrentSession.FacebookId;
                App.isAuthenticated = true;
                Frame.Navigate(typeof(MainPage));
               
            }
        }
    }
}
