# Style Guide

## Actor system contracts

- Actor system messages and events live in `AkkaSignalRVuePoc.Contracts`.
- Actor system events are declared as C# `record` types.
- Event type names use past tense and camel-cased words in C# PascalCase; for example, `ActorSystemStarted`.
- Actor implementations live in `AkkaSignalRVuePoc.Core` and consume contracts from the contracts library.
