using System.Security.Cryptography;
using R_SS.BLL.Interfaces;

namespace R_SS.BLL.Helpers;

public sealed class OtpGenerator : IOtpGenerator
{
    public string Generate(int digits = 6)
    {
        if (digits < 4 || digits > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(digits), "OTP length must be between 4 and 10 digits.");
        }

        var bytes = RandomNumberGenerator.GetBytes(digits);
        var chars = new char[digits];

        for (var i = 0; i < digits; i++)
        {
            chars[i] = (char)('0' + (bytes[i] % 10));
        }

        return new string(chars);
    }
}
