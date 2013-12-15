using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace Event_Finder
{
    public class tryPush:INotifyPropertyChanged
    {
        private double _latit;
        private double _longtit;
        public double Latit { set { _latit = value; 
            NotifyPropertyChanged("Latit"); ; } get { return _latit; } }
        public double Longtit
        {
            set
            {
                _longtit = value;
            NotifyPropertyChanged("Longtit");; } get { return _longtit; } }

        public tryPush(double lat, double longt) {
            this.Latit = lat;
            this.Longtit = longt;
        
        }

        


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
