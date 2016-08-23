using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class MessageBoxService : IMessageBoxService
    {
        public async Task Show(string message)
        {
            MessageBox.Show(message);
        }
    }
}
