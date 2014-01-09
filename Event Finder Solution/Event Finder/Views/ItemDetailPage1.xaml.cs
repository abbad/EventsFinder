using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Event_Finder.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Event_Finder.Models;
using Windows.UI.Popups;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Event_Finder.Views
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage1 : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        MessageDialog dialog = new MessageDialog("Error occured!!");

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        Event selectedEvent;

        public ItemDetailPage1()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.Loaded += ItemDetailPage1_Loaded;
            
        }

        async void ItemDetailPage1_Loaded(object sender, RoutedEventArgs e)
        {
            bool error = false; 
            progBar.IsIndeterminate = true;
            App.commonApiHandler.friendList.Clear();
            FriendRoot vsx= null;
            try
            {
              vsx = await App.commonApiHandler.facebookApi.GetFriendsAttendingEvent(selectedEvent.eid);
            }
            catch (Facebook.WebExceptionWrapper){error = true;}
            if (error) 
            {
                try {
                    MessageDialog msg = new MessageDialog("Connection Lost");
                    await msg.ShowAsync();
                    return;
                }catch(Exception){}
            
            }
            
            App.commonApiHandler.FillFriendsAttendingCollection(vsx);
            attendFr.ItemsSource = App.commonApiHandler.friendList;

            // see RSVP status of event.
            RootObject rsvp = await App.commonApiHandler.facebookApi.GetRSVPStatusForUser(selectedEvent.eid);
            // git list of friends.

            if (rsvp.data.Count != 0)
            {
                SetButtonToStatus(rsvp.data[0]);
            }
            else
            {
                // enable all buttons.
                SetButtonToStatus(null);
            }

            progBar.IsIndeterminate = false;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Event navigationParameter;
            if (e.PageState != null && e.PageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = (Event)e.PageState["SelectedItem"];
                 
            }
           
            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
            // TODO: Assign the selected item to this.flipView.SelectedItem
        }

        

        private void SetButtonToStatus(RSVP rsvp)
        {
            if (rsvp != null)
            {
                if (rsvp.rsvp_status == "attending")
                {
                    AttendButton.IsEnabled = false;
                    DeclineButton.IsEnabled = true;
                    MaybeButton.IsEnabled = true;
                }
                else if (rsvp.rsvp_status == "unsure")
                {
                    DeclineButton.IsEnabled = true;
                    AttendButton.IsEnabled = true;
                    MaybeButton.IsEnabled = false;
                }
                else if (rsvp.rsvp_status == "declined")
                {
                    DeclineButton.IsEnabled = false;
                    AttendButton.IsEnabled = true;
                    MaybeButton.IsEnabled = true;
                }

            }
            else
            {
                DeclineButton.IsEnabled = true;
                AttendButton.IsEnabled = true;
                MaybeButton.IsEnabled = true;

            }
        }

        async private void AttendButton_Click(object sender, RoutedEventArgs e)
        {
            progBar.IsIndeterminate = true;

            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool attending = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "attending");
            if (attending == true)
            {
                SetButtonToStatus(new RSVP { rsvp_status = "attending" });
                // check if the event is in the list of the attended events.
                if (!App.AttendingCollection.Contains(selectedEvent))
                {
                    App.AttendingCollection.Add(selectedEvent);
                }
            }
            else
            {
                dialog.Content = "Could not RSVP for Event";
                await dialog.ShowAsync();
            }

            progBar.IsIndeterminate = false;
        }

        async private void MaybeButton_Click(object sender, RoutedEventArgs e)
        {
            progBar.IsIndeterminate = true;

            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;
            bool maybe = false;

            maybe = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "maybe");

            if (maybe == true)
            {

                SetButtonToStatus(new RSVP { rsvp_status = "unsure" });
                // check if the event is in the list of the attended events.
                if (!App.AttendingCollection.Contains(selectedEvent))
                {
                    App.AttendingCollection.Add(selectedEvent);
                }
            }
            else
            {
                dialog.Content = "Could not RSVP for Event";
                await dialog.ShowAsync();
            }

            progBar.IsIndeterminate = false;

        }

        async private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            progBar.IsIndeterminate = true;

            Button btn = (Button)sender;
            Event selectedEvent = (Event)btn.DataContext;

            bool decline = await App.commonApiHandler.facebookApi.RSVPEvent(selectedEvent.eid, "declined");

            if (decline == true)
            {
                SetButtonToStatus(new RSVP { rsvp_status = "declined" });
                // check if the event is in the list of the attended events.
                if (!App.AttendingCollection.Contains(selectedEvent))
                {
                    App.AttendingCollection.Add(selectedEvent);
                }
            }
            else
            {
                dialog.Content = "Could not RSVP for Event";
                await dialog.ShowAsync();
            }

            progBar.IsIndeterminate = false;

        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            this.DataContext = e.Parameter;
            selectedEvent = e.Parameter as Event;
        }

       

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
