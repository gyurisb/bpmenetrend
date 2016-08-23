using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace CityTransitApp.NetPortableServicesImplementations
{
    class MessageDialogService : IMessageBoxService
    {
        public async Task Show(string message)
        {
            await new MessageDialog(message).ShowAsync();
        }
    }
}
