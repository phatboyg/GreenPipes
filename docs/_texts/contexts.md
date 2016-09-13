---
layout: default
title: Contexts
subtitle: A unit of interaction with the pipe
---

The `context` is an object of an interaction with a pipe. Each time you call `pipe.Send` you will be passing an instance of a `PipeContext`. This is not your typical message payload. This is the context for passing through the pipes. If you built a message bus on top of GreenPipes the message would exist in the context, but would not be the context. That feels way more meta than it needs to be. The context can store data inside of it, and should be considered write only and once a value is written it should be immutable.

> We highly recommend using interfaces with Read Only properties when ever possible.

## Overly simple usage

```
pipe.Send(new SomeContext());
```

## Loading up a context with HTTP stuffs

```
var httpContext = new HttpContext(this.Request, this.Response);
pipe.Send(httpContext); 
```
