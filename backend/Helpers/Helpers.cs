using System.Security.Cryptography;
namespace backend.Helpers;

public class Helpers
{
    public static int GenerateRanInt(int min, int max)
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] data = new byte[4];
        rng.GetBytes(data);
        int value = Math.Abs(BitConverter.ToInt32(data, 0));
        return min + (value % (max - min));
    }
}