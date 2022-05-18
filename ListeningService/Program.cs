// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using OrderService.Models;

Console.WriteLine("Listening....");
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .Build();

var config = new ConsumerConfig
{
    BootstrapServers = configuration.GetSection("KafkaSettings").GetSection("Server").Value,
    GroupId = "DotNetFood",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

//Connect to Kafka

CancellationTokenSource cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

var topics = new string[]
{
    "Food", "AddOrder", "AcceptOrder", "DeleteOrder"
};
using (var consumer = new ConsumerBuilder<string, string>(config).Build())
{

    Console.WriteLine("Connected");
    consumer.Subscribe(topics);
    try
    {
        while (true)
        {
            var cr = consumer.Consume(cts.Token);
            Console.WriteLine($"Consumed record with Topic: {cr.Topic} Key: {cr.Message.Key} and Value: {cr.Message.Value}");

            using (var context = new DotNetFoodDbContext())
            {
                
            }

        }
    }
    catch (OperationCanceledException)
    {
        // Ctrl-C was pressed.
    }
    finally
    {
        consumer.Close();
    }
}
