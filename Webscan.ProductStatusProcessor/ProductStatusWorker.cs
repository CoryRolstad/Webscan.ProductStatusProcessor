using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Webscan.ProductStatusProcessor.Models;
using Webscan.ProductStatusProcessor.Services;

namespace Webscan.ProductStatusProcessor
{
    public class ProductStatusWorker : BackgroundService
    {
        private readonly ILogger<ProductStatusWorker> _logger;
        private readonly IOptions<KafkaSettings> _kafkaSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly IProductQueryService _productQueryService;

        public ProductStatusWorker(ILogger<ProductStatusWorker> logger, IOptions<KafkaSettings> kafkaSettings, IServiceProvider serviceProvider, IProductQueryService productQueryService)
        {
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)} cannot be null");
            _kafkaSettings = kafkaSettings ?? throw new ArgumentNullException($"{nameof(kafkaSettings)} cannot be null");
            _serviceProvider = serviceProvider ?? throw new ArgumentOutOfRangeException($"{nameof(serviceProvider)} cannot be null");
            _productQueryService = productQueryService ?? throw new ArgumentNullException($"{nameof(productQueryService)} cannot be null");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}: ProductStatusProcessor Started ");

            ConsumerConfig consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.Value.Broker,
                GroupId = _kafkaSettings.Value.SchedulerTopicGroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            ProducerConfig producerConfig = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.Value.Broker                
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
            {
                consumer.Subscribe(_kafkaSettings.Value.SchedulerTopicName);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    
                    StatusCheck statusCheck = JsonConvert.DeserializeObject<StatusCheck>(consumeResult.Message.Value);
                    _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}:\tReceived Kafka Message on {_kafkaSettings.Value.SchedulerTopicName}");
                    _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}:\t\t{statusCheck.Name}\n\t\t\t{statusCheck.Url}");
                    // handle consumed message.
                    bool productIsInStock = await _productQueryService.IsProductInStock(statusCheck);
                    _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}:\t\t{statusCheck.Name} in stock: {productIsInStock}");
                    if (productIsInStock)
                    {
                        producerConfig.ClientId = statusCheck.Name;
                        _logger.LogInformation($"\t{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff")}:\t\t{statusCheck.Name} in stock sending notification message to {_kafkaSettings.Value.NotifierTopicName} topic");
                        using (var p = new ProducerBuilder<Null, string>(producerConfig).Build())
                        {
                            var response = await p.ProduceAsync(_kafkaSettings.Value.NotifierTopicName, new Message<Null, string> { Value = JsonConvert.SerializeObject(statusCheck, Formatting.Indented) })
                                .ContinueWith(task => task.IsFaulted
                                        ? $"error producing message: {task.Exception.Message}"
                                        : $"produced to: {task.Result.TopicPartitionOffset}");

                            // wait for up to 10 seconds for any inflight messages to be delivered.
                            p.Flush(TimeSpan.FromSeconds(10));
                        }

                    }

                }
            
                consumer.Close();
            }


            // 1.) Get the StatusCheck and Deserialize it
            // 2.) Use ProductQueryService to Query the Product.
            // 3.) If fail topic string doesn't equal what was retireved then add message to the Notifier kafka topic


        }
    }
}
