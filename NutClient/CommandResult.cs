namespace NutClient;

/// <summary>
/// Creates a new instance of the <see cref="CommandResult"/> class.
/// </summary>
/// <param name="command">The command that was execute.</param>
/// <param name="error">An <see cref="Error"> result.</param>
/// <param name="data">Any addition data returned by the command.</param>
public class CommandResult(Command command, Error error, object? data)
{
    /// <summary>
    /// Gets the <see cref="Command"/> that was executed.
    /// </summary>
    public Command Command { get; } = command;

    /// <summary>
    /// Gets the last error that occurred.<br />
    /// A success is indicated by <see cref="Error.None"/>
    /// </summary>
    public Error Error { get; } = error;

    /// <summary>
    /// Gets any additional data that was returned by the command;
    /// <b>null</b> if no additional data was returned.<br />
    /// This could be a string or string array.
    /// </summary>
    public object? Data { get; } = data;

    /// <summary>
    /// Creates a new instance of the <see cref="CommandResult"/> class
    /// with <see cref="Error"/> set to <see cref="Error.None"/>.
    /// </summary>
    /// <inheritdoc cref="CommandResult(Command, Error, object)" path="/param[@name=='command']"/>
    public CommandResult(Command command) : this(command, Error.None, null) { }

    /// <summary>
    /// Creates a new instance of the <see cref="CommandResult"/> class
    /// with <see cref="Error"/> set to <see cref="Error.None"/>
    /// and <see cref="Data"/> set to <paramref name="data"/>.
    /// </summary>
    /// <inheritdoc cref="CommandResult(Command, Error, object)" path="/param[@name=='command']"/>
    /// <inheritdoc cref="CommandResult(Command, Error, object)" path="/param[@name=='data']"/>
    public CommandResult(Command command, object? data) : this(command, Error.None, data) { }

    /// <summary>
    /// Creates a new instance of the <see cref="CommandResult"/> class
    /// with <see cref="Error"/> set to <paramref name="error"/>.
    /// </summary>
    /// <inheritdoc cref="CommandResult(Command, Error, object)" path="/param[@name=='command']"/>
    /// <inheritdoc cref="CommandResult(Command, Error, object)" path="/param[@name=='error']"/>
    public CommandResult(Command command, Error error) : this(command, error, null) { }

}
