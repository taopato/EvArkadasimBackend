using System;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Hashing
{
    public static class HashingHelper
    {
        public static string CreatePasswordHash(string password)
        {
            // 1) Yeni HMACSHA512 örneği oluşturup key’i salt olarak kullan
            using var hmac = new HMACSHA512();
            byte[] saltBytes = hmac.Key;

            // 2) Parolanın hash’ini al
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = hmac.ComputeHash(passwordBytes);

            // 3) Salt ve hash’i Base64’le string’e dönüştür ve “:” ile birleştir
            string salt = Convert.ToBase64String(saltBytes);
            string hash = Convert.ToBase64String(hashBytes);
            return $"{salt}:{hash}";
        }

        public static bool VerifyPasswordHash(string password, string stored)
        {
            // 1) “salt:hash” formatını ayrıştır
            var parts = stored.Split(':', 2);
            if (parts.Length != 2) return false;

            byte[] saltBytes = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);

            // 2) Aynı salt ile HMACSHA512 oluştur ve verilen parolanın hash’ini al
            using var hmac = new HMACSHA512(saltBytes);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] computedHash = hmac.ComputeHash(passwordBytes);

            // 3) Uzunlukları ve byte’ları karşılaştır
            if (computedHash.Length != storedHashBytes.Length)
                return false;

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != storedHashBytes[i])
                    return false;
            }

            return true;
        }
    }
}
