# A hearty message from the internet

This is a simple fullstack application intended to serve as an MVP of how one might handle and visualise a set
timeseries data supplied by a remote websocket server.

### Highlevel architecture

            Remote data source
            ┌───┐
            │ 1 │
            │   │
            └─┬─┘
    Charts    │
    ┌───┐   ┌─┴─┐    ┌─┐Persistence
    │ 4 ├───┤ 2 ├────│3│
    │   │   │   │    └─┘
    └───┘   └───┘
            backend


This application is a standard n-tier stack comprised of an
upstream source, backend server, some basic persistence and a user facing asset.

1. The source data

   A remote streaming/websocket service. When it is not misbehaving (and it misbehaves often!), it exposes a single set
   of timeseries data which we might find in a device such as a heart rate monitor (HRM):

        timestamp: long
        value: int

2. Backend is a dotnet (core) webapi written with C# - I'm using the minimal WebAPI syntax in this application as opposed to the larger MVC structure. This is a good way to start developing quickly with little boilerplate whilst allowing for future restructuring as needed. dotnet/C# expose some astonishingly powerful APIs which make it a great combo for all scale of application and use cases.

   The application architecture is quite flat at this point with some obvious concerns separated to help us scale out/up
   should this MinimalVP become a MaximalVP. Let's take a brief look at what these are:

   - Upstream data ingestor -  This is configured as a background service that will startup and shutdown gracefully on application lifecycle. It's one responsibility is to do whatever necessary to retrieve messages and pass them on.

   - Message handler - Determine if our received data is good and if so allow it to propagate to our application code.

   - Message persister - Another background service that take a piece of data and stores it. This is another lifecycle aware component to ensure we have graceful handling of resources (I/O) and messages in-flight.

   - Message retriever - Take a date range (currently expressed as a start and end DateTime) and return all data within it.

   - Message streamer - Re-exposes our upstream timeseries data as another stream for a real-time view of data

   #### A few notes on some of the technical choices

   `Channels` (thread-safe, in-memory message passing) are used as a lightweight pubsub pattern to separate our core application concerns. If you are unfamiliar with this style of abstraction it may be a little confusing at first to understand how things are wired together - especially as we are also using Dependency Injection for Type resolution. This is an easy tradeoff as it doesn't take long to understand (message writers/readers) and makes it conceptually easy to think about future service extraction without making development unnecessarily complicated during the MVP stage.

   `File` persistence is used for our storage. This is a concious decision to defer selection of a more robust storage engine until such time as more requirements emerge. On the face of it we have a simple `key/value` data structure for which almost any document, relational or KV store will handle (and do we use a hosted or managed service!? - lock-in could be costly at this time). We also know we have a `timeseries` for which a specialist engine would be worth considering. Using a simple file allows us to think about the requirements at hand to deliver this MVC whilst we better understand what our access patterns and usage will be in the medium term. Getting the storage wrong early on can be costly both in terms of money and time.

   `SignalR` is Microsoft's answer to data streaming. I've never used this before so there are a few things I will circle back on to fully understand but for now the documentation and API quality is such that I didn't have to spend much time thinking about it at all - It Just Works™️️
