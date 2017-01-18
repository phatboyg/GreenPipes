# Green Pipes

GreenPipes is a middleware library for the Task Parallel Library.

GreenPipes is a library built from the message pipeline of MassTransit. 
In fact, MassTransit is using GreenPipes, instead of the internal pipeline 
(it was entirely harvested to GreenPipes for that very purpose). 
In addition, since the pipeline is now standalone, it can be used with 
other libraries such as Automatonymous to get a cohesive understanding of 
the data flow through the pipeline.

To get a quick idea about GreenPipes, have a look at Dru Sellers'
[blog post][1], which gives some explanation and code samples.



[1]: https://drusellers.com/greenpipes/2016/10/30/greenpipes.html