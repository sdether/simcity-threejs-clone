# CitySim.js

This is a fork of Daniel Greenheck's CitySim.js

https://github.com/dgreenheck/simcity-threejs-clone

## What this is

This is a project to create a thin presentation layer for an out-of-process city simulator.
The end goal is to separate the presentation and interaction so that it can receive events
about simulation state changes and send event about world changes enacted through user
interaction.

## What this is not

This is not an attempt to improve or comment on the design and decisions of Daniel's project.
The goals of this project are entirely different and are only taking advantage of the
excellent base his project provides for presentation experimentation.

## How do I run this locally?

You will first need to install [Node.js](https://nodejs.org).

After that, clone this repository, navigate to the root directory and run the following command

```bash
npm run dev
```

## License

This code is covered by the MIT License. TLDR; you can do whatever you want with it!

