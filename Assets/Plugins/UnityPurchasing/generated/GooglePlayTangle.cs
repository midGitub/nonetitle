#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("WjI4jXSqN2uZfIWqu3iiDKG0ZPiDa8kGW8ctjvTHwjfNVAnDoklSnBvRC4HBkhGYlb3ealvI7KcDA8n5GqgrCBonLCMArGKs3ScrKysvKilIFyYtY9dAM+ZxQncbvd9pOvLH0ZiJs1QzffPefC+157vGVdJISVteT0TFupktyl6OMIYCXCbG4h52igqoKyUqGqgrICioKysqvJPV8iJ5jlUOtbI9QfpQS6/6x5DqDp62SHE0dMtwKyV4RYR3L+qhcfR5WyJvgKVpyf5p+NiLo4sffMTuesL+K+iuVBAX9r/O4pOT3SVXHfr4fDwHts88gQ0h0Cxjs5BdxppTZVZ9X9a91KjoXjI4nc28AjqIeBzF7jXgl7vPlzmRV5V3AQeT8ygpKyor");
        private static int[] order = new int[] { 7,13,10,7,4,10,11,13,13,9,12,13,13,13,14 };
        private static int key = 42;

        public static byte[] Data() {
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
