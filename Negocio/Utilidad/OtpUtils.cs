using System.Security.Cryptography;
using System.Text;

public static class OtpUtils
{
    public static string GenerarOtpNumerico(int length = 6)
    {
        var max = (int)Math.Pow(10, length);
        var n = RandomNumberGenerator.GetInt32(0, max);
        return n.ToString(new string('0', length));
    }

    public static (string hash, string salt) HashOtp(string otp)
    {
        // Salt aleatorio
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);

        using var sha = SHA256.Create();
        var data = Encoding.UTF8.GetBytes($"{otp}:{salt}");
        var hashBytes = sha.ComputeHash(data);
        var hash = Convert.ToHexString(hashBytes); // 64 chars hex

        return (hash, salt);
    }

    public static bool ValidarOtp(string otp, string hashStored, string saltStored)
    {
        using var sha = SHA256.Create();
        var data = Encoding.UTF8.GetBytes($"{otp}:{saltStored}");
        var hashBytes = sha.ComputeHash(data);
        var hash = Convert.ToHexString(hashBytes);
        return hash.Equals(hashStored, StringComparison.OrdinalIgnoreCase);
    }
}
