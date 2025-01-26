using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutClient;

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

internal static class Extensions
{
    public static string ToNutString(this Error error)
    {
        return error.ToString().Replace("_", "-");
    }

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