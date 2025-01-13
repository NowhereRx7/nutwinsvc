namespace NutClient
{
    public enum Command
    {
        [Obsolete("Needs more investigating")]
        VER,
        [Obsolete("Use PROTVER when ver > 2.8.0")]
        NETVER,
        [Obsolete("Needs more investigating")]
        PROTVER,
        HELP,
        STARTTLS,
        GET,
        LIST,
        USERNAME,
        PASSWORD,
        LOGIN,
        LOGOUT,
        [Obsolete("Use PRIMARY when ver > 2.8.0")]
        MASTER,
        PRIMARY,
        FSD,
        SET,
        INSTCMD
    }
}
