namespace R_SS.Models.Entities;

public class PasswordResetRequest
{
    public int PasswordResetRequestId { get; set; }

    public int UserId { get; set; }

    public required string OtpCodeHash { get; set; }

    public int OtpAttemptCount { get; set; }

    public DateTime OtpSentAtUtc { get; set; }

    public DateTime OtpExpiresAtUtc { get; set; }

    public int SendAttemptCount { get; set; }

    public DateTime SendWindowStartedAtUtc { get; set; }

    public DateTime? FunctionLockedUntilUtc { get; set; }

    public bool IsOtpVerified { get; set; }

    public DateTime? OtpVerifiedAtUtc { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public required virtual User User { get; set; }
}
