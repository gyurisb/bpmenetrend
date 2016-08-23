using CityTransitApp.Common;
using System;
using System.Collections.Generic;
using System.Text;
using NetPortableServices;

namespace CityTransitApp.NetPortableServicesImplementations
{
    public class WindowsUniversalServices : ICommonServices
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
        public IAuthenticationService Auth { get { return null; } }

        public WindowsUniversalServices()
        {
            Compression = new UniversalCompression();
            Cryptography = new UniversalCriptography();
            FileSystem = new UniversalFileSystem();
            Http = new UniversalHttpClient();
            Resources = new UniversalResources();
            Location = new UniversalLocation();
            Settings = new UniversalPersistentSettings();
            MessageBox = new MessageDialogService();
            Directions = new UniversalDirections();
        }
    }
}
