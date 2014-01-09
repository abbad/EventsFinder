using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event_Finder.Models
{
    class GroupedEvent
    {
        public GroupedEvent()
        {
            Events = new ObservableCollection<Event>();
        }
        public String Day{ set; get;}

        public ObservableCollection<Event> Events { private set; get;}

    }
}
