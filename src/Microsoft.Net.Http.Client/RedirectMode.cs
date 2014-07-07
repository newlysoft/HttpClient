namespace Microsoft.Net.Http.Client
{
    public enum RedirectMode
    {
        /// <summary>
        /// Do not follow redirects.
        /// </summary>
        None,

        /// <summary>
        /// Dissalows redirecting from HTTPS to HTTP
        /// </summary>
        NoDowngrade,

        /// <summary>
        /// Follow all redirects
        /// </summary>
        All,
    }
}