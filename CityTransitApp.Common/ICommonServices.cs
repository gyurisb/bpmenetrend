using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetPortableServices;

namespace CityTransitApp.Common
{
    public interface ICommonServices
    {
        ICompressionService Compression { get; }
        ICriptographyService Cryptography { get; }
        IFileSystemService FileSystem { get; }
        IHttpService Http { get; }
        IResourcesService Resources { get; }
        ILocationService Location { get; }
        ISettingsService Settings { get; }
        ITileService Tiles { get; }
        IMessageBoxService MessageBox { get; }
        IDirectionsService Directions { get; }
        IAuthenticationService Auth { get; }
    }
}
