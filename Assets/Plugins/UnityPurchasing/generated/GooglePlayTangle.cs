#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("/OnsJ+es5nNudUDps1CimMcjmXo4u7W6iji7sLg4u7u6Lsbewmn7e+iJwTtc3wIh1lddzfilwnxwhE9YN85BIbISg1qQ0Dkvxuhnv425gsaSAhziQIQIQkyYjlArZImIBNZhQjKXl4aMDTf/d+T21IQt0fySbB5Piji7mIq3vLOQPPI8Tbe7u7u/urmw5Y9Mc56YeQA0nWWtc9yX5VZuaXykAmOcqJJIWyJkDhBFvQWrjvyq8V0QNyynDz28gzMaLBXV0idlpU5mM2lo6k1QFSXdaViOjOEFaZpN5ZsoVp0mWFTJQuslUOYH5w46CHvw1PKUBd+oIpti48rzF187i1tF20v6FRdKngQV9JdIAZOE5zdoHqaoYbB6svFX2UK6L7i5u7q7");
        private static int[] order = new int[] { 12,1,11,7,4,10,12,8,9,9,11,12,13,13,14 };
        private static int key = 186;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
