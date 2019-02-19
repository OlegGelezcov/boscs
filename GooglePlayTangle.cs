#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("nfwSMcTxlhEaXjpAsw6MTTfrLS1IfIEdTAL7sBm6IKGqh6LKXNgwz1o02bDzMXrjA/s59xpZT9d7EOc4wTMNBQfjtjZtvVXAeX1mxluy/5tnQynjE1/nHiqM01gW6Cqv/9zK+SxbqMUmhxQ9N9/m4LlTg/BiXt0LOd1mjl2bCiWux1YdmmUzaABAhbZX5WZFV2phbk3hL+GQamZmZmJnZALHij2wnT9tSKUT4MqThFi2Vdi+5HiYKB6BAV9ja6A0K/STetd3Ulh2y4TG3Nx0Drfnj3P6nh4iASOeIuVmaGdX5WZtZeVmZmfBdruyDB5zWgAsA/AWCTA84AI3b3CPsfyZ15crrRT9YMnsHx1cEJFPU3L4lkYpIjgEhoBVVmcmqGVkZmdm");
        private static int[] order = new int[] { 6,7,2,4,12,8,10,10,8,13,11,11,12,13,14 };
        private static int key = 103;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
