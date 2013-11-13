using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Event_Finder.Models;

namespace Event_Finder_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var x= @"{
  'data': [
    {
      'eid': 432992910153865, 
      'name': 'الذكـرى السنويـة الـ 9 لرحيـل سيـد الشهـداء ياسـر عرفـات \'أبـوعمـار\'', 
      'description': 'الساحـة الأردنيـة', 
      'venue': {
        'name': 'Amman'
      }
    }, 
    {
      'eid': 191820720996836, 
      'name': 'My marriage', 
      'description': 'صالة روتانا', 
      'venue': {
        'latitude': 32.5556, 
        'longitude': 35.85, 
        'street': '', 
        'zip': '', 
        'id': 106205736077341
      }
    }, 
    {
      'eid': 1402151660002286, 
      'name': 'عيد ميلاد لجين محمد عبد الله', 
      'description': 'تاريخ الميلاد14/11/2007', 
      'venue': {
        'latitude': 32.6369, 
        'longitude': 35.8872, 
        'street': '', 
        'zip': '', 
        'id': 107040849336561
      }
    }
  ]
}";
            Data result = JsonConvert.DeserializeObject<Event_Finder.Models.Data>(x);
            System.Diagnostics.Debug.WriteLine("ficl");
        }
    }
}
