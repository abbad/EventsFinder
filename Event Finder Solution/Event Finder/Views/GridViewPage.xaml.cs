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
    public sealed partial class GridViewPage : Page
    {
        public GridViewPage()
        {
            this.InitializeComponent();
            EventsGridView.Loaded += EventsGridView_Loaded;
        }

        void EventsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.myEventsSelected)
            {
                EventsGridView.ItemsSource = App.commonApiHandler.UserEvents;
                myEventsButton.Label = "View All Events";
            }
            else
            {
                EventsGridView.ItemsSource = App.commonApiHandler.QueriedEvents;
                myEventsButton.Label = "My Events";
            }
        }

        private void appBarNavigateButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void EventsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemDet = (Event_Finder.Models.Event)e.ClickedItem;
            this.Frame.Navigate(typeof(ItemDetailPage1), itemDet);
        }

        private void myEventsButton_Click(object sender, RoutedEventArgs e)
        {

            string toggle = myEventsButton.Label;
            if (toggle == "My Events")
            {
                myEventsButton.Label = "View All Events";

                EventsGridView.ItemsSource = App.commonApiHandler.UserEvents;
                App.myEventsSelected = true;
            }
            else
            {
                EventsGridView.ItemsSource = App.commonApiHandler.QueriedEvents;
                myEventsButton.Label = "My Events";
                App.myEventsSelected = false;
            }

        }
    }
}
