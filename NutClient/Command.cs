namespace NutClient
{
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
        HELP,
        STARTTLS,
        GET,
        LIST,
        USERNAME,
        PASSWORD,
        LOGIN,
        LOGOUT,
        MASTER,
        /// <summary>
        /// Alias of MASTER on VER >= 2.8.0
        /// </summary>
        PRIMARY,
        FSD,
        SET,
        INSTCMD
    }
}
