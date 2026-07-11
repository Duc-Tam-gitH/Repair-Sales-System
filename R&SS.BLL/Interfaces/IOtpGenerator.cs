namespace R_SS.BLL.Interfaces;

public interface IOtpGenerator
{
    /// <summary>
    /// Generates a numeric OTP with the specified number of digits.
    /// </summary>
    /// <param name="digits">The OTP length.</param>
    /// <returns>The generated OTP code.</returns>
    string Generate(int digits = 6);
}
