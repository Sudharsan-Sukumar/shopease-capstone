namespace Application.Api.Features.Auth.Revocation;

/// <summary>
/// JWT `iat` is whole-second Unix time (RFC 7519), but User timestamps like
/// PasswordChangedAtUtc carry millisecond precision. Comparing them directly
/// makes a token issued in the SAME second as the change look "older" than
/// the change purely due to truncation, and get falsely revoked. Truncating
/// the DB timestamp down to seconds before comparing removes that false
/// positive; the trade-off is a token issued a fraction of a second before
/// the change - but in the same second - stays valid until the next
/// full-second boundary, which is an acceptable, unavoidable consequence of
/// `iat`'s second-level granularity.
/// </summary>
internal static class RevocationTimestamp
{
    public static DateTime TruncateToSeconds(this DateTime value) =>
        new(value.Ticks - value.Ticks % TimeSpan.TicksPerSecond, value.Kind);
}
