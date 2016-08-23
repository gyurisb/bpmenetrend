using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CityTransitApp.WPSilverlight.NetPortableServicesImplementations
{
    class SilverlightCriptography : ICriptographyService
    {
        public byte[] Encrypt(byte[] publicKey, byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] privateKey, byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool IsSignatureValid(byte[] publicKey, byte[] data, byte[] signature)
        {
            string xmlKeyServer = @"<RSAKeyValue><Modulus>zeBzfM+PYmq4kppjm52JCNmsyvHv89RM624qhJMf5AFYrIpyBE9Ndq+VH7V1B/2/L6cB371bkn4BN8Z/653bA2H54hlXfKid+26VjNZypEZbUgT4EKvclAX9VmFUMvXgmt4joQLg5PRXdQZH8379FAAuhj4oQshkwtZsAyeir9k=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            var rsaServer = new RSACryptoServiceProvider();
            rsaServer.FromXmlString(xmlKeyServer);
            return rsaServer.VerifyData(data, new System.Security.Cryptography.SHA1Managed(), signature);
        }

        public byte[] Sign(byte[] privateKey, byte[] data)
        {
            string xmlKeyClient = @"<RSAKeyValue><Modulus>zvY1gxsA4AyAqQH4GgyGheTtA2Q1JeckXt4klVtEhJtNeJtHvji3mJUqerSrZFn1If79uVa5f5mk5YuiDS8XdQI1EZTXp7N2Qx+hXLDUbNQ8pfiYevJgQ/7eYY/evEm7cj++/rGLh7kijBKUc+RsIgoHVrrfPd56wNRlPQn6Ny8=</Modulus><Exponent>AQAB</Exponent><P>3KeGsSbhU99HTY6TTGQoNwEBGOHV0URxr0+IseNAWVosQ0jOsOUJI2mytXi6Gaentx3OH1N1lN/3NueshkhBcw==</P><Q>8B0utcK4tln2XA51xjlkMU8DqLFCvTFO3UsIF4ZNhXjEKxYnmwZYKpUDsjQhXgDNxj3Qm0qPhSVcgnuhV8KUVQ==</Q><DP>vaVCr6mSCqshtnfvA74ljjjPv5oCUq035IxAGVwPJ3zIBEBkXUdM1mH3Fd+gW4JsNIdqbCQoL/9ak5cFAhjCHw==</DP><DQ>zGVLGdaEPlYebQayIuc/7umAB929HigXJjF01fGxk+jVtIfLdx/TdYwb9VqC/O5aPGLqbQbvXTwMn9Z15arxcQ==</DQ><InverseQ>aVGx6IouXEJwABP3SqDaLTMuLclj6XKJt+q1Du+J7oAGVwc2ZWglHodfpT4yFtnkM78pXJb7HGWmBIfNG2CZ1A==</InverseQ><D>X0UuZ/5PvlNzDJnH9HHMoSk7Q8Lmxl5rOzKGY6yiU2rMVFcixPmF5nrQFbFCbo0Mj0w6zw0RkiAPrM1E2U3SvSmd4gkQmwBLg06K+lUMDO4RfTr/QwYcp8q7VgltWuNCybOwwdzKRrUuR0cHmn6C6tTMnYScgX3KUmHK16cBaHU=</D></RSAKeyValue>";
            var rsaClient = new RSACryptoServiceProvider();
            rsaClient.FromXmlString(xmlKeyClient);
            return rsaClient.SignData(data, new System.Security.Cryptography.SHA1Managed());    
        }
    }
}
