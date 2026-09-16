namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// The workflow a P6 session is being established for.
    /// </summary>
    public enum P6WorkflowKind
    {
        /// <summary>Page the project catalog and reply with a raw data download.</summary>
        RawData,

        /// <summary>Page the project catalog and reply with the projects themselves.</summary>
        ProjectList,

        /// <summary>Run a full catalog synchronisation.</summary>
        Sync
    }
}
