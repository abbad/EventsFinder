using Bing.Maps;
using Event_Finder.ViewModel;
using Facebook;
using Facebook.Client;
using Facebook.Client.Controls;
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
            loginButton.Permissions = ViewModel.Constants.Permissions;
            
        }
 
        async protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.GettingPositionFinished = new TaskCompletionSource<bool>();
            base.OnNavigatedFrom(e);
            App.ItemEventsList.Clear();
            App.AttendingCollection.Clear();
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
            App.GettingPositionFinished.TrySetResult(true);
            
            // get list of atteneded events by user.by 
            String error = await App.commonApiHandler.QueryForUserEvents();

            // QueryForEventsWithinAnArea
            try
            {
                error = await App.commonApiHandler.QueryForEventsWithinAnArea(App.offset, DateTimeConverter.DateTimeToUnixTimestamp(App.startRange),
                    DateTimeConverter.DateTimeToUnixTimestamp(App.endRange));
            }catch(Facebook.WebExceptionWrapper exception) { error = exception.Data.ToString(); }
            
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
                App.CurrentSession = x.CurrentSession;
                App.AccessToken = x.CurrentSession.AccessToken;
                App.FacebookId = x.CurrentSession.FacebookId;
                App.CurrentUser = x.CurrentUser;
                App.isAuthenticated = true;
                Frame.Navigate(typeof(MainPage));

            }
            else if (e.SessionState == Facebook.Client.Controls.FacebookSessionState.Closed)
            {
                App.CurrentSession = null;

                // The control signals when user info is set (handled in OnUserInfoChanged below), but not when it
                // is cleared (probably a bug), so we clear our reference here when the session ends.
                App.CurrentUser = null;
            }
        }

        async protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.commonApiHandler.GettingEventsFinished = new TaskCompletionSource<bool>();
            
            prog.IsIndeterminate = true;
            base.OnNavigatedTo(e);
       
            this.loginButton.ApplicationId = Constants.FacebookAppId;

            App.CurrentSession = FacebookSessionCacheProvider.Current.GetSessionData();
            if ((App.CurrentSession != null) && (App.CurrentSession.Expires <= DateTime.UtcNow))
            {
                // User was previously logged in, but session expired.  Log them in again...
                App.CurrentSession = await App.FacebookSessionClient.LoginAsync(Constants.Permissions);
                App.isAuthenticated = true;
            }

            if (App.CurrentSession != null)
            {
                App.isAuthenticated = true;
                this.loginButton.SetValue(LoginButton.CurrentSessionProperty, App.CurrentSession);
                if (App.CurrentUser == null)
                {
                    FacebookClient client = new FacebookClient(App.CurrentSession.AccessToken);
                    dynamic result = await client.GetTaskAsync("me");
                    App.CurrentUser = new GraphUser(result);
                }
              
                App.AccessToken = App.CurrentSession.AccessToken;
                App.FacebookId = App.CurrentSession.FacebookId;
            }
           
            if (App.CurrentUser != null)
            {
                App.isAuthenticated = true;
                this.loginButton.SetValue(LoginButton.CurrentUserProperty, App.CurrentUser);
            }

            if (App.isAuthenticated) 
            {
                Frame.Navigate(typeof(MainPage));
            }

            prog.IsIndeterminate = false;
        }
    
    }
}
