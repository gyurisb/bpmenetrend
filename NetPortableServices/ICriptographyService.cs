
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetPortableServices
{
    public interface ICriptographyService
    {
        byte[] Encrypt(byte[] publicKey, byte[] data);
        byte[] Decrypt(byte[] privateKey, byte[] data);

        bool IsSignatureValid(byte[] publicKey, byte[] data, byte[] signature);
        byte[] Sign(byte[] privateKey, byte[] data);
    }
}
