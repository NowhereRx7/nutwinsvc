namespace NutWinSvc.Nut
{
    internal enum Command
    {
        VER,
        NETVER,
        PROTVER,
        HELP,
        STARTTLS,
        GET,
        LIST,
        USERNAME,
        PASSWORD,
        LOGIN,
        LOGOUT,
        PRIMARY,
        [Obsolete("Use PRIMARY when ver > 2.8.0")]
        MASTER,
        FSD,
        SET,
        INSTCMD
    }
}
