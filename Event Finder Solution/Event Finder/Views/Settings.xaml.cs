using Event_Finder.ViewModel;
using Facebook;
using Facebook.Client;
using Facebook.Client.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            OffsetSlider.Value = App.offset;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

       async protected override void OnNavigatedTo(NavigationEventArgs e)
       {
           base.OnNavigatedTo(e);
           this.loginButton.ApplicationId = Constants.FacebookAppId;
           
           App.CurrentSession = FacebookSessionCacheProvider.Current.GetSessionData();
           if ((App.CurrentSession != null) && (App.CurrentSession.Expires <= DateTime.UtcNow))
           {
                // User was previously logged in, but session expired.  Log them in again...
                App.CurrentSession = await App.FacebookSessionClient.LoginAsync(Constants.Permissions);
           }

            if (App.CurrentSession != null)
            {
                this.loginButton.SetValue(LoginButton.CurrentSessionProperty, App.CurrentSession);
                if (App.CurrentUser == null)
                {
                    FacebookClient client = new FacebookClient(App.CurrentSession.AccessToken);
                    dynamic result = await client.GetTaskAsync("me");
                    App.CurrentUser = new GraphUser(result);
                }
            }

            if (App.CurrentUser != null)
            {
                this.loginButton.SetValue(LoginButton.CurrentUserProperty, App.CurrentUser);
            }
            App.isAuthenticated = false;
          
        
        }


       private void loginButton_SessionStateChanged(object sender, Facebook.Client.Controls.SessionStateChangedEventArgs e)
       {
           Facebook.Client.Controls.LoginButton x = (Facebook.Client.Controls.LoginButton)sender;
           if (e.SessionState == Facebook.Client.Controls.FacebookSessionState.Opened)
           {
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
               App.isAuthenticated = false;
               Frame.Navigate(typeof(LoginPage));
           }
       }


       private void OffsetSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
       {
           if (OffsetSlider != null) { 
                App.offset = OffsetSlider.Value;
           }
       }

       async protected override void OnNavigatedFrom(NavigationEventArgs e)
       {
           App.localSettings.Values["offset"] = App.offset.ToString();
           base.OnNavigatedFrom(e);
           App.ItemEventsList.Clear();
           App.AttendingCollection.Clear();
           
          
           // get list of atteneded events by user.by 
           String error = await App.commonApiHandler.QueryForUserEvents();

           // QueryForEventsWithinAnArea
           try
           {
               error = await App.commonApiHandler.QueryForEventsWithinAnArea(App.offset, DateTimeConverter.DateTimeToUnixTimestamp(App.startRange),
                   DateTimeConverter.DateTimeToUnixTimestamp(App.endRange));
           }
           catch (Facebook.WebExceptionWrapper exception) { error = exception.Data.ToString(); }

           if (error != null)
           {
               App.errorOccured = true;
               App.errorMessage = error;
               App.ErrorOccuredFinished.TrySetResult(true);
           }
       }
    }
}
