using Microsoft.Phone.Info;
using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class SilverlightUserIdentification : IAuthenticationService
    {
        public string GetUserID()
        {
            object anidObject = null;
            UserExtendedProperties.TryGetValue("ANID2", out anidObject);
            if (anidObject == null)
                UserExtendedProperties.TryGetValue("ANID", out anidObject);
            return anidObject.ToString();
        }
    }
}
