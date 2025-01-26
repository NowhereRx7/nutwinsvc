namespace NutClient;

/// <summary>
/// An enumeration of errors that the NUT server could respond with.
/// </summary>
public enum Error
{
    None,
    ACCESS_DENIED,
    UNKNOWN_UPS,
    VAR_NOT_SUPPORTED,
    CMD_NOT_SUPPORTED,
    INVALID_ARGUMENT,
    INSTCMD_FAILED,
    SET_FAILED,
    READONLY,
    TOO_LONG,
    FEATURE_NOT_SUPPORTED,
    FEATURE_NOT_CONFIGURED,
    ALREADY_SSL_MODE,
    DRIVER_NOT_CONNECTED,
    DATA_STALE,
    ALREADY_LOGGED_IN,
    INVALID_PASSWORD,
    ALREADY_SET_PASSWORD,
    INVALID_USERNAME,
    ALREADY_SET_USERNAME,
    USERNAME_REQUIRED,
    PASSWORD_REQUIRED,
    UNKNOWN_COMMAND,
    INVALID_VALUE,
    UNKNOWN
}

public static partial class Extensions
{
    public static string ToNutString(this Error error) => Enum.GetName<Error>(error)?.Replace("_", "-") ?? string.Empty;

    /// <summary>
    /// Converts an ERR string to an <see cref="Error"/> value.
    /// </summary>
    /// <param name="nutErrorMessage">
    /// The ERR message from the server (whole or partial).
    /// </param>
    /// <returns>
    /// An <see cref="Error"/> value; or <see cref="Error.UNKNOWN"/> if unable to determine.
    /// </returns>
    public static Error ToError(this string nutErrorMessage)
    {
        nutErrorMessage = nutErrorMessage.Trim().ToUpperInvariant();
        if (nutErrorMessage.StartsWith("ERR"))
            nutErrorMessage = nutErrorMessage[3..].TrimEnd();
        if (nutErrorMessage.Contains(' '))
            nutErrorMessage = nutErrorMessage.Remove(nutErrorMessage.IndexOf(' ')).TrimEnd();
        if (Enum.TryParse(typeof(Error), nutErrorMessage.ToUpperInvariant().Replace("-", "_"), out var result))
            return (Error)result;
        else
            return Error.UNKNOWN;
    }
}