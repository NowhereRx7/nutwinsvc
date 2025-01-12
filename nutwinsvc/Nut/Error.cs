using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutWinSvc.Nut;

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

public static class Extensions
{
    public static string ToNutString(this Error error)
    {
        return error.ToString().Replace("_", "-");
    }

    public static Error ToError(this string err)
    {
        err = err.Trim().ToUpperInvariant();
        if (err.StartsWith("ERR"))
            err = err[3..].TrimEnd();
        if (err.Contains(' '))
            err = err.Remove(err.IndexOf(' ')).TrimEnd();
        if (Enum.TryParse(typeof(Error), err.ToUpperInvariant().Replace("-", "_"), out var result))
            return (Error)result;
        else
            return Error.UNKNOWN;
    }
}