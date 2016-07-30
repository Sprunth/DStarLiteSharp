# DStarLiteSharp

A C# port of the D* Lite implementation at [https://github.com/daniel-beard/DStarLiteJava](https://github.com/daniel-beard/DStarLiteJava)

This is a work a progress.

## Installation

Open the included .sln file and build. C5 will be pulled from Nuget.
DStarLiteSharp will produce the dll file, DStarLiteSharpSample is a small demonstration of the library.

## Usage

(from the original version)
``` C#
//Create pathfinder
  DStarLite pf = new DStarLite();
  //set start and goal nodes
  pf.init(0,1,3,1);
  //set impassable nodes
  pf.updateCell(2, 1, -1);
  pf.updateCell(2, 0, -1);
  pf.updateCell(2, 2, -1);
  pf.updateCell(3, 0, -1);
  //perform the pathfinding
  pf.replan();
  //get and print the path
  List<State> path = pf.getPath();
  for (State i : path)
  {   
     System.out.println("x: " + i.x + " y: " + i.y);
  }   
```

## History

v0.1 07/28/16: Initial port. Minimal changes, not yet abiding by C# style guidelines.

## License

The MIT License (MIT)
Copyright (c) 2016 Dylan Wang

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.