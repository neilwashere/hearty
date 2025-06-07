# A hearty message from the internet

This is a simple fullstack application intended to 
serve as a demonstration of how one might handle and visualise a set
timeseries HRM (heart rate monitor) data supplied by a remote websocket server.

The upstream source is known to be a bit flakey so we are also demonstrating
a fault tollerance strategy for dealing with the inevitible errors and dropouts.

## Architecture

            Timeseries stream source
            ┌───┐                   
            │ 1 │                   
            │   │                   
            └─┬─┘                   
    Charts     │                     
    ┌───┐   ┌─┴─┐    ┌─┐Persistence 
    │ 4 ├───┤ 2 ├────│3│            
    │   │   │   │    └─┘            
    └───┘   └───┘                   
            backend                


This application is a standard n-tier stack comprised of some
upstream source, backend handler, persistence and frontend component.

1. The source data - a remote streaming service (websocket). All
we know is that it can, and will, misbehave!

2. Backend is a dotnet webapi - dotnet core is a very robust platform for application development. I'm using the minimal API syntax in this application as opposed to the larger MVC structure. This is a good way to start developing quickly whilst allowing for future restructuring as needed.

The application has several concerns:

- Streaming data sourcing
- Data persistence
- Data streaming for client applications (see next)
- Basic HTML serving for frontend charts

Each of these concerns, and more besides, could be handled by distinct (micro) services. For this reason some cursory thought has been given to
application structure which might facilitate future extraction (but this is not a primary concern and some shortcuts have been taken).

TODO - provide some more details about the architecture as it transpires

3. Persistence - TODO

4. Frontend - TODO
