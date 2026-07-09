namespace R_SS.BLL.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
