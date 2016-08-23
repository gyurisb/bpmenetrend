using NetPortableServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using System.Runtime.InteropServices.WindowsRuntime;

namespace CityTransitApp.NetPortableServicesImplementations
{
    class UniversalCriptography : ICriptographyService
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
            string base64ServerKey = @"BgIAAACkAABSU0ExAAQAAAEAAQDZr6InA2zWwmTIQig+hi4AFP1+80cGdVf05OACoSPemuD1MlRhVv0FlNyrEPgEUltGpHLWjJVu+52ofFcZ4vlhA9ud63/GNwF+klu93wGnL7/9B3W1H5Wvdk1PBHKKrFgB5B+ThCpu60zU8+/xyqzZCImdm2OakrhqYo/PfHPgzQ==";
            var alg = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaSignPkcs1Sha1);
            var key = alg.ImportPublicKey(CryptographicBuffer.DecodeFromBase64String(base64ServerKey));
            return CryptographicEngine.VerifySignature(key, data.AsBuffer(), signature.AsBuffer());
        }

        public byte[] Sign(byte[] privateKey, byte[] data)
        {
            string base64ClientKey = @"BwIAAACkAABSU0EyAAQAAAEAAQAvN/oJPWXUwHrePd+6VgcKImzkc5QSjCK5h4ux/r4/crtJvN6PYd7+Q2Dyepj4pTzUbNSwXKEfQ3azp9eUETUCdRcvDaKL5aSZf7lWuf3+IfVZZKu0eiqVmLc4vkebeE2bhERblSTeXiTnJTVkA+3khYYMGvgBqYAM4AAbgzX2znNBSIas5zb335R1Ux/OHbenpxm6eLWyaSMJ5bDOSEMsWllA47GIT69xRNHV4RgBATcoZEyTjk1H31PhJrGGp9xVlMJXoXuCXCWFj0qb0D3GzQBeITSyA5UqWAabJxYrxHiFTYYXCEvdTjG9QrGoA08xZDnGdQ5c9lm2uMK1Lh3wH8IYAgWXk1r/LygkbGqHNGyCW6DfFfdh1kxHXWRABMh8Jw9cGUCM5DetUgKav884jiW+A+93tiGrCpKpr0KlvXHxquV11p8MPF3vBm3qYjxa7vyCWvUbjHXTH3fLh7TV6JOx8dV0MSYXKB693QeA6e4/5yKyBm0eVj6E1hlLZczUmWAbzYcEpmUc+5ZcKb8z5NkWMj6lX4ceJWhlNgdXBoDuie8Oteq3iXLpY8ktLjMt2qBK9xMAcEJcLorosVFpdWgBp9fKYVLKfYGchJ3M1OqCfpoHR0cutUbK3MGws8lC41ptCVa7yqccBkP/On0R7gwMVfqKToNLAJsQCeKdKb3STdlEzawPIJIRDc86TI8MjW5CsRXQeuaF+cQiV1TMalOirGOGMjtrXsbmwkM7KaHMcfTHmQxzU75P/mcuRV8=";
            var alg = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaSignPkcs1Sha1);
            var key = alg.ImportKeyPair(CryptographicBuffer.DecodeFromBase64String(base64ClientKey));
            var signBuffer = CryptographicEngine.Sign(key, data.AsBuffer());
            return signBuffer.ToArray();
        }
    }
}
