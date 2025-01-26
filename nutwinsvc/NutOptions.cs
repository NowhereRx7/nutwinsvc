using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text;

namespace NutWinSvc;

internal partial class NutOptions : IValidateOptions<NutOptions>
{
    public string? UpsName { get; set; }

    public string? Host { get; set; }

    public int Port { get; set; } = 3493;

    public bool UseTls { get; set; } = false;

    public string? Username { get; set; }

    public string? Password { get; set; }


    // I don't use DataAnnotations because trimming.
    public ValidateOptionsResult Validate(string? name, NutOptions options)
    {
        if (options is null) return ValidateOptionsResult.Fail("Configuration object is null.");
        StringBuilder failure = new();
        ValidateOne(options.UpsName);
        ValidateOne(options.Host);
        ValidateOne(options.Username);
        ValidateOne(options.Password);
        return (failure.Length > 0) ? ValidateOptionsResult.Fail(failure.ToString()) : ValidateOptionsResult.Success;
        void ValidateOne(string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (String.IsNullOrWhiteSpace(argument))
                failure.AppendLine($"{paramName} cannot be null, empty, or whitespace.");
        }
    }
}
