using Microsoft.Extensions.Options;
using System.Text;

namespace NutWinSvc;

internal partial class NutOptions : IValidateOptions<NutOptions>
{
    public string? UpsName { get; set; }

    public string? Host { get; set; }

    public ValidateOptionsResult Validate(string? name, NutOptions options)
    {
        StringBuilder failure = new();
        if (string.IsNullOrWhiteSpace(options.UpsName))
            failure.AppendLine($"{nameof(UpsName)} cannot be null, empty, or whitespace.");
        if (string.IsNullOrWhiteSpace(options.Host))
            failure.AppendLine($"{nameof(Host)} cannot be null, empty, or whitespace");


        return (failure.Length > 0) ? ValidateOptionsResult.Fail(failure.ToString()) : ValidateOptionsResult.Success;

    }
}
