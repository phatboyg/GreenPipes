Green Pipes
===========

GreenPipes is a middleware library for the Task Parallel Library.

![master](https://ci.appveyor.com/api/projects/status/uea2vdj1sko1exen?svg=true
)

GreenPipes is a library built from the message pipeline of MassTransit. In fact, MassTransit will be using GreenPipes in the near future, instead of the internal pipeline (it was entirely harvested to GreenPipes for that very purpose). In addition, since the pipeline is now standalone, it can be used with other libraries such as Automatonymous to get a cohesive understanding of the data flow through the pipeline.


