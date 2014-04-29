namespace Autofac.Core.Lifetime
{
    /// <summary>
    /// Well-known tags used in setting up matching lifetime scopes.
    /// </summary>
    public static class MatchingScopeLifetimeTags
    {
        /// <summary>
        /// Tag used in setting up per-request lifetime scope registrations
        /// (e.g., per-HTTP-request or per-API-request).
        /// </summary>
        public static readonly object RequestLifetimeScopeTag = "AutofacWebRequest";
    }
}
