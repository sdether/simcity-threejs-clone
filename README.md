# CitySim.js

This is a fork of Daniel Greenheck's CitySim.js

https://github.com/dgreenheck/simcity-threejs-clone

## What this is

This is a refactor of Daniel's city simulator with the simulation and presentation layers
separated by a Command Query Responsibility Segregation (CQRS) pattern with the presentation
living in the browser and the simulation living on a server. Changes are planned (mouse-down)
on the client and sent as REST commands once committed (mouse-up) to the server.

The presentation is updated by change events over a web socket stream from the server.

## What this is not

This is not an attempt to improve or comment on the design and decisions of Daniel's project.
The goals of this project are entirely different and is only taking advantage of the
excellent base his project provides for presentation experimentation.

## How do I run this locally?

### C# backend (current)

The simulation runs as a C# ASP.NET Core service. You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Node.js](https://nodejs.org).

**Terminal 1 — C# simulation server (port 3001):**
```bash
dotnet run --project CitySim.WebService
```
Or launch `WebService` from the `Server.slnx` solution in Rider/Visual Studio for a debuggable session.

**Terminal 2 — Vite frontend (port 3333):**
```bash
npm install
npm run dev
```

Then open `http://127.0.0.1:3333/simcity-threejs-clone/`.

### Node.js backend (legacy reference only)

```bash
npm install
npm run dev:full
```

`dev:full` runs the original Node.js simulation server alongside the frontend. It exists only to compare against the previous JS-only behaviour and is not kept in sync with active development.

## License

This code is covered by the MIT License. TLDR; you can do whatever you want with it!

