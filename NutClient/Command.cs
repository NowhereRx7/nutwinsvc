namespace NutClient;

/// <summary>
/// The NUT client command to execute.
/// </summary>
public enum Command
{
    /// <summary>
    /// Gets the NUT version.
    /// </summary>
    VER,
    /// <summary>
    /// Gets the protocol version.
    /// </summary>
    NETVER,
    /// <summary>
    /// Alias of NETVER, but only valid on VER >= 2.8.0
    /// </summary>
    PROTVER,
    /// <summary>
    /// Gets help information from the server.
    /// </summary>
    HELP,
    /// <summary>
    /// Switches the data stream to TLS encryption.
    /// </summary>
    STARTTLS,
    /// <summary>
    /// Gets a value for a UPS.<br/>
    /// <see cref="GetCommand"/>
    /// </summary>
    GET,
    /// <summary>
    /// Gets a list of values for a UPS.<br />
    /// <see cref="ListCommand"/>
    /// </summary>
    LIST,
    /// <summary>
    /// Sets the username for the session.
    /// </summary>
    USERNAME,
    /// <summary>
    /// Sets the password for the session.
    /// </summary>
    PASSWORD,
    /// <summary>
    /// Logs into the specified UPS using the previously provided USERNAME and PASSWORD command.<br />
    /// <b>Note</b>: This should only be used for clients acting like <b>upsmon</b>.
    /// </summary>
    LOGIN,
    /// <summary>
    /// Log out of a previously logged in UPS.<br/>
    /// <b>Requires USERNAME and PASSWORD</b>
    /// </summary>
    LOGOUT,
    /// <summary>
    /// <b>Note</b>: Alias of MASTER on VER >= 2.8.0.
    /// </summary>
    MASTER,
    /// <summary>
    /// <b>Requires USERNAME and PASSWORD</b><br />
    /// <b>Note</b>: Alias of MASTER on VER >= 2.8.0.
    /// </summary>
    PRIMARY,
    /// <summary>
    /// <b>Requires USERNAME and PASSWORD</b>
    /// </summary>
    FSD,
    /// <summary>
    /// Sets a value for a UPS.<br/>
    /// <b>Requires USERNAME and PASSWORD</b>
    /// </summary>
    SET,
    /// <summary>
    /// <b>Requires USERNAME and PASSWORD</b>
    /// </summary>
    INSTCMD
}
