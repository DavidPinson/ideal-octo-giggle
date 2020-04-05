# YAWNA

## What?

YAWNA stands for: Yet Another Wiki Note App.

It's quite simple, I used during a couple of months [dokuwiki](https://www.dokuwiki.org/dokuwiki) in "portable" mode and it worked great. I synced my wiki/note on multiple device using OneDrive. But the sync stopped working recently (2019). So I began looking for a replacement... [StakEditPro](https://stackeditpro.io/), [w2wiki](https://github.com/panicsteve/w2wiki), [Jingo](https://github.com/claudioc/jingo), [Zim](https://zim-wiki.org/index.html), [MDwiki](http://dynalon.github.io/mdwiki/#!index.md), [WikiJs](https://wiki.js.org/), [Start-and-doc](https://start-and-doc.com/#/), [Joplin](https://joplinapp.org/) and I'm sure there's plenty more...

But I was not able to find what I wanted:

- a simple solution
  - not hosted
  - no local web server
- use markdown
- uses flat files for storage
  - simple backup
  - sync or not on multiple device using my sync preference
- be able to view/edit notes/pages in the same environment
- use it like a wiki
  - add link and create pages if not exist
- be able to search through all the notes/pages easily

## How?

The application is gonna be in C# dotnet core WPF. So sadly I lost the multiplatform advantage of dotnet core but I want something usable relatively quick...

The application will use the library [Markdig](https://github.com/lunet-io/markdig), [Markdig.wpf](https://github.com/Kryptos-FR/markdig.wpf) and [hOOt](https://github.com/mgholam/hOOt) for the core functionalities.

## When?

Well, like everybody I already have a lot of stuff to do, but I want to have those functionalities fast:

- view pages/notes
- add pages/notes
- edit pages/notes
- search through pages/notes
- => nothing fancy just functional...
