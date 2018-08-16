
namespace Mel.VoxelGen
{
    public static class StringToBytes
    {
        public static byte[] ToBytes(string s)
        {
            var chars = s.ToCharArray();
            byte[] bytes = new byte[chars.Length];
            for(int i=0; i < chars.Length; ++i)
            {
                bytes[i] = (byte)chars[i];
            }
            return bytes;
        }

        public static string FromBytes(byte[] bytes)
        {
            char[] chars = new char[bytes.Length];
            for(int i=0; i<chars.Length; ++i)
            {
                chars[i] = (char)bytes[i];
            }
            return new string(chars);
        }
    }
}
