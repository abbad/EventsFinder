﻿using Facebook.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        private async Task Authenticate()
        {
            string message = String.Empty;
            try
            {
                session = await App.FacebookSessionClient.LoginAsync("user_about_me, read_stream, user_events, user_friends, friends_events, rsvp_event");
                App.AccessToken = session.AccessToken;
                App.FacebookId = session.FacebookId;

                Frame.Navigate(typeof(MainPage));
            }
            catch (InvalidOperationException e)
            {
                message = "Login failed! Exception details: " + e.Message;
                MessageDialog dialog = new MessageDialog(message);
                dialog.ShowAsync();
                btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        async private void btnFacebookLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!App.isAuthenticated)
            {
                try
                {

                    btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    await Authenticate();
                    App.isAuthenticated = true;
                }
                catch (Exception exp) 
                {
                    
                   btnFacebookLogin.Visibility = Windows.UI.Xaml.Visibility.Visible;
                
                }
            }
        }
    }
}
