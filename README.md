# QEngine.Core
Queue Engine Core is all in one library to run SignalR Messaging Queue using .Net 6.

Why .Net? because I love it!
Why SignalR? because its provide realtime communication between Server and Client.

# Usage

### Producer
```CSharp
using QEngine.Core;
using QEngine.Core.Clients;

var taskProducer = await Task.Factory.StartNew(async () =>
{
    using var qProducer = new QProducer("localhost", 7149, true); //Host, Port, Ssl (Currently ignore certificate verification)
    var qChannel = new QChannel("DemoProducer", "temp"); //DemoProducer is a producer name, "temp" is queueName
    qProducer.Use(qChannel);
    var index = 0;
    while (true)
    {
        var data = $"data {index++}";
        await qProducer.ProduceAsync(data);
        Console.WriteLine($"Producing {data}");
        await Task.Delay(5000);
    }
});
```
### Consumer
```CSharp
using QEngine.Core;
using QEngine.Core.Clients;

var taskConsumer = await Task.Factory.StartNew(async () =>
{
    var qConsumer = new QConsumer("localhost", 7149, true); //Host, Port, Ssl (Currently ignore certificate verification)
    var qChannel = new QChannel("DemoConsumer", "temp"); //DemoConsumer is a consumer name, "temp" is queueName
    qConsumer.Use(qChannel);
    while (true)
    {
        var content = await qConsumer.ConsumeAsync<string>(0);
        Console.WriteLine("Consume Result: " + content);
    }
});
```
