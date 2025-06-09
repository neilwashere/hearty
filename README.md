# A hearty message from the internet

This is a simple fullstack application intended to serve as an MVP of how one might handle and visualise a set
timeseries data supplied by a remote websocket server.

[Hosted Hearty ❤️](https://hearty-g2frazh4hvgnc6g7.westeurope-01.azurewebsites.net/index.html)


## How to run the solution locally

Clone the repo then from within the root folder you have two options

1. [dotnet sdk/runtime](https://dotnet.microsoft.com/en-us/download)

        cd src/Hearty.WebApp
        dotnet run

   note that a local file `hearty_data.log` will be created where incoming messages are persisted

2. [docker](https://www.docker.com/)

        docker build -t hearty-webapp .
        docker run -p 5030:5030 heart-webapp

In either scenario, the default configuration will have the server listening on `http://localhost:5030`

## Running the tests

No docker for this as I don't have any time left.

      dotnet test

### Tech notes

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

2. Backend is a dotnet (core) webapi written with C#

   I'm using the minimal WebAPI syntax in this application as opposed to the larger MVC structure. This is a good way to start developing quickly with little boilerplate whilst allowing for future restructuring as needed. dotnet/C# expose some astonishingly powerful APIs which make it a great combo for all scale of application and use cases.

   The application architecture is quite flat at this point with some obvious concerns separated to help us scale out/up
   should this MinimalVP become a MaximalVP. Let's take a brief look at what these are:

   - Upstream data ingestor -  This is configured as a background service that will startup and shutdown gracefully on application lifecycle. It's one responsibility is to do whatever necessary to retrieve messages and pass them on.

   - Message handler - Determine if our received data is good and if so allow it to propagate to our application code.

   - Message persister - Another background service that take a piece of data and stores it. This is another lifecycle aware component to ensure we have graceful handling of resources (I/O) and messages in-flight.

   - Message retriever - Take a date range (currently expressed as a start and end DateTime) and return all data within it.

   - Message streamer - Re-exposes our upstream timeseries data as another stream for a real-time view of data

3. Persistence is a File

   `File` persistence is used for our storage. This is a concious decision to defer selection of a more robust storage engine until such time as more requirements emerge. On the face of it we have a simple `key/value` data structure for which almost any document, relational or KV store will handle (and then do we use a hosted or managed service!?). We also know we have a `timeseries` for which a specialist engine would be worth considering. Using a simple file allows us to think about the requirements at hand to deliver this MVC whilst we better understand what our access patterns and usage will be in the medium term. Getting the storage wrong early on can be costly both in terms of money and time.

4. Front end is basic HTML, JS

   Coming into this project I had to put aside some time to look into charting libraries. I decided ahead of time to use the minimal amount of wizardry for serving the front end in order to geek out a little on the charts. I've accrued technical debt here - some copy/pasta, duplication and inconsitencies are apparent (albeit manageable at this point). If I had to spend more time on just one area, this would be it. I'd add the Blazor framework to this project and use the templating facilities it provides to keep the code nicely organised.

#### A few notes on some other technical choices

   [Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels) (thread-safe, in-memory message passing) are used as a lightweight pubsub pattern to separate our core application concerns. If you are unfamiliar with this style of abstraction it may be a little confusing at first to understand how things are wired together - especially as we are also using Dependency Injection for Type resolution. This is an easy tradeoff as it doesn't take long to understand (message writers/readers) and makes it conceptually easy to think about future service extraction without making development unnecessarily complicated during the MVP stage.

   [SignalR](https://dotnet.microsoft.com/en-us/apps/aspnet/signalr) is Microsoft's answer to data streaming (websockets). I've never used this before so there are a few things I will circle back on to fully understand but for now the documentation and API quality is such that I didn't have to spend much time thinking about it at all - It Just Works™️️

   [ChartJS](https://www.chartjs.org/) (along with SignalR client) is the only external frontend dependency. This is a reasonably well documented and competent open source charting solution which is more than enough for this MVP. I spent a little bit of time looking at [highcharts](https://www.highcharts.com/) - this is something I would consider if taking this solution to market - utterly gorgeous

   [azure](https://azure.microsoft.com) has been used for hosting for no other reason than I have some free credits and it plays nicely with dotnet stuff out of the box (as you might expect for microsoft). The application uses a github workflow to build and deploy on commits to the `main` branch.

#### What to do with some more time
There are some very obvious, and not so obvious, problems with the current solution:

1. Scale

   It won't. This works at the moment because there is one, and only one, server processing the incoming data and servicing client requests. I don't have to fire up another instance to know that a rift in spacetime would be the likely result. Duplicate data and inconsistencies in the representation would be the result assuming a resource conflict on data writes doesn't blow the system up to start with. That leads me to...

2. Storage

   I've already discussed this a bit. A suitable storage engine needs to be added if we're going to go anywhere with this. It will be a good first step to ensuring we can start to get some decent data guarantees and not worry about singularities (too much) when firing up multiple potential data writers. Something with streaming support might be neat (InfluxDB + Highcharts looks like a potent mix).

3. Authn

   This application has zero concept of access controls. Not a requirement now but it would be nice to know who is using the app (especially if we can get them to give us payment details).

4. Observability

   We have some logs which are ok. Next step would be structured logging and capturing some useful metrics. Structured logs first, then telemetry. We need to know what is happening, how long it is taking and when it goes wrong. We can't (prove to) improve what we can't measure.

5. Mobile? (and other display types)

   I don't even know if the front end will work on mobile. I've splashed some css which might help a little but definitely is not going to look good. What other sorts of device might we need to support and how much do we want to invest in regression testing? Front end tests are often laborious to setup and can be brittle and finneky. Speaking of tests...

6. Tests

   Although at number 6, this is not ordered by importance! My approach throughout this endevour has been manual exploration (the console project in `explore` was my entry point). I have some very cursory regression tests for the backend stuff but these are by no means comprehensive, well structured or robust.

   As happens quite often with front end stuff, manual checks are just simpler at the start so there are ZERO browser behaviour / view tests. 👀 eyeballing is good enough for now.


7. Structure

   I'm actually really happy with the application construction but it lacks meaningful organisation. I'd spend a little bit of time shuffling pieces around to represent the domain it is expressing. Actually, I might even not do that much _just yet_. There is something to be said for simplicity at this stage and having things within reach requires less mental effort. Still, a little organisation will help drive and evolve the domain understanding and act as navigation breadcrumbs for new contributors.

8. Front End

   Already mentioned but it's worth repeating. The front end assets right now are a danger spot. It won't actually take much time to fix at all but some choices can be made which will affect _how_ it is addressed. We could add support into our exising webapp, we could separate the frontend entirely into a separate project/repo, we can deploy on a CDN, we could do server side rendering! I would keep the assets here for now and introduce Blazor templating to keep things nice and tidy, maintain a consistent layout, share components and so on. I don't think we need a full frontend framework (such as React) just yet but we are not restricted either and I'd keep that option open.


#### Some things I really like about this solution

Have I mentioned how few dependencies there are? From an MVP perspective we've managed to hit all of the high notes without constraining any future steps or making expensive choices. What has been used has been minimal and to the point. Very few, if any, assumptions have been made.
