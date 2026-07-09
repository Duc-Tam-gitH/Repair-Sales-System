namespace R_SS.BLL.Exceptions;

public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}
