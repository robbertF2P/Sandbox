Vendored from https://github.com/zoran-horvat/immutable-domain-tools (MIT).

Local patch: `ImmutableUpdateExtensions.UpdateCollection` only removes child entities
absent from the immutable copy (upstream removed all tracked children after updates).
