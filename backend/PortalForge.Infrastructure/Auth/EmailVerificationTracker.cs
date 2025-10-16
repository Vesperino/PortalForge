using System.Collections.Concurrent;

namespace PortalForge.Infrastructure.Auth;

/// <summary>
/// Tracks email verification resend attempts to prevent spam
/// </summary>
public class EmailVerificationTracker
{
    private readonly ConcurrentDictionary<string, DateTime> _lastResendTimes = new();
    private readonly TimeSpan _cooldownPeriod = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Checks if email can be resent for the given email address
    /// </summary>
    public bool CanResendEmail(string email)
    {
        if (!_lastResendTimes.TryGetValue(email, out var lastResendTime))
        {
            return true;
        }

        return DateTime.UtcNow - lastResendTime >= _cooldownPeriod;
    }

    /// <summary>
    /// Records that an email was sent
    /// </summary>
    public void RecordResend(string email)
    {
        _lastResendTimes[email] = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the remaining cooldown time for the given email
    /// </summary>
    public TimeSpan GetRemainingCooldown(string email)
    {
        if (!_lastResendTimes.TryGetValue(email, out var lastResendTime))
        {
            return TimeSpan.Zero;
        }

        var elapsed = DateTime.UtcNow - lastResendTime;
        var remaining = _cooldownPeriod - elapsed;

        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Cleans up old entries (called periodically)
    /// </summary>
    public void Cleanup()
    {
        var expiredEntries = _lastResendTimes
            .Where(kvp => DateTime.UtcNow - kvp.Value > _cooldownPeriod)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredEntries)
        {
            _lastResendTimes.TryRemove(key, out _);
        }
    }
}
