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

```bash
npm install
npm run dev:full
```

## License

This code is covered by the MIT License. TLDR; you can do whatever you want with it!

