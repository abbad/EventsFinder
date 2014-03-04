using Bing.Maps;
using Event_Finder.Common;
using Event_Finder.Models;
using Event_Finder.ViewModel;
using Event_Finder.Views;
using Facebook.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Event_Finder
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        internal static GraphUser CurrentUser;
        internal static FacebookSession CurrentSession;

        internal static TaskCompletionSource<bool> GettingPositionFinished = new TaskCompletionSource<bool>(); 
        // checking for error Occured value
        internal static TaskCompletionSource<bool> ErrorOccuredFinished = new TaskCompletionSource<bool>();
            
        internal static Geoposition myPosition;
        internal static string AccessToken = String.Empty;
        internal static string FacebookId = String.Empty;
        public static bool isAuthenticated = false;
        public static FacebookSessionClient FacebookSessionClient = new FacebookSessionClient(Constants.FacebookAppId);

        internal static bool myEventsSelected = false;
        internal static string errorMessage = "";

        internal static bool errorOccured = false;

        internal static CommonApiHandler commonApiHandler = new CommonApiHandler();

        internal static DateTime endRange = DateTime.Today.AddDays(5);

        internal static DateTime startRange = DateTime.Today;

        internal static double offset = 0.5;

        internal static Location myLocation;

        internal static Location MyLocation 
        {
            get { return myLocation; }
        }

        internal static double zoomLevel = 15;
        // List of events attended by user.
        
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
         
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (localSettings.Values.ContainsKey("offset"))
            {
                offset = Convert.ToDouble(localSettings.Values["offset"]);
            }
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    
                }
                
                
                //settings

                // Register handler for CommandsRequested events from the settings pane
                SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
                

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(LoginPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }
        
        
        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            //if you want to put settings in the settings chart , just remove the slashes and add usercontrol called MyUserControl1,MyUserControl2 and put what ever controls you want
            //becuase i read that you have to put the settings in the settings chart not in the bottom bar.


            //var chng = new SettingsCommand("Settings", "Settings", (handler) =>
            //{
                //var settings = new Windows.UI.Xaml.Controls.SettingsFlyout();
                //settings.Content = new MyUserControl1();
                //settings.HeaderBackground = new SolidColorBrush(Windows.UI.Color.FromArgb(50, 10, 10, 150));
                //settings.HeaderForeground = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 0, 0, 0));
                //settings.Title = "Settings";
                //settings.Visibility = Visibility.Visible;
              //  settings.Show();
            //});

            //args.Request.ApplicationCommands.Add(chng);





            // Add an About command

            //var about = new SettingsCommand("about", "About", (handler) =>
            //{

              //var settings = new Windows.UI.Xaml.Controls.SettingsFlyout();
                //settings.Content = new MyUserControl2();
                //settings.HeaderBackground = new SolidColorBrush(Windows.UI.Color.FromArgb(50, 10, 10, 150));
                //settings.HeaderForeground = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 0, 0, 0));
                //settings.Title = "About";
                //settings.Visibility = Visibility.Visible;
                //settings.Show();
            //});

            //args.Request.ApplicationCommands.Add(about);

        }
        

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            localSettings.Values["offset"] = offset.ToString();
            //TODO: Save application state and stop any background activity
            var deferral = e.SuspendingOperation.GetDeferral();
            
           
            deferral.Complete();
        }

    
        public static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }
    }
}
