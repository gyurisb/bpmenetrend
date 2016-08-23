using CityTransitApp.Common;
using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class WPSilverlightServices : ICommonServices
    {
        public ICompressionService Compression { get; private set; }
        public ICriptographyService Cryptography { get; private set; }
        public IFileSystemService FileSystem { get; private set; }
        public IHttpService Http { get; private set; }
        public IResourcesService Resources { get; private set; }
        public ILocationService Location { get; private set; }
        public ISettingsService Settings { get; private set; }
        public ITileService Tiles { get; private set; }
        public IMessageBoxService MessageBox { get; private set; }
        public IDirectionsService Directions { get; private set; }
        public IAuthenticationService Auth { get; private set; }

        public WPSilverlightServices()
        {
            Compression = new SilverlightFastdbCompression();
            Cryptography = new SilverlightCriptography();
            FileSystem = new SilverlightFileSystem();
            Http = new SilverlightHttpClient();
            Resources = new SilverlightResources();
            Location = new SilverlightLocation();
            Settings = new SilverlightPersistentSettings();
            MessageBox = new MessageBoxService();
            Directions = new SilverlightDirections();
            Auth = new SilverlightUserIdentification();
        }
    }
}
