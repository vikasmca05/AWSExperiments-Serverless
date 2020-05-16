using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SampleReconcileService
{
    public class SQS_Consumer
    {
        //private readonly AppConfig _appConfig;

        public AmazonSQSClient _client;
        public AppConfig _awsConfig;
        public TransactionsRecord transactionRecord;

        public SQS_Consumer(AppConfig appConfig)
        {
            _awsConfig = appConfig;
        

            AmazonSQSConfig amazonSQSConfig = new AmazonSQSConfig();

            try
            {
                var config = new AmazonSQSConfig()
                {
                    RegionEndpoint = Amazon.RegionEndpoint.USEast1
           
                    //ServiceURL = "https://sqs.us-east-1.amazonaws.com/283124460764/NewProvisioning" // Region and URL
                };

                //_messageRequest.QueueUrl = _appConfig.AwsQueueURL;

                var awsCredentials = new AwsCredentials(_awsConfig);


                _client = new AmazonSQSClient(awsCredentials, config);


                //Create object for DB Operations
                transactionRecord = new TransactionsRecord(awsCredentials);

            }

            catch (Exception ex)
            {

            }
        }




        public string GetQueueUrl()
        {
            return "https://sqs.us-east-1.amazonaws.com/283124460764/NewProvisioning";
            //return _appConfig.AwsQueueURL;
        }



        public async Task<SendMessageResponse> SendMessagetoSQS()
        {
            AmazonSQSConfig amazonSQSConfig = new AmazonSQSConfig();
            SendMessageResponse sendMessageResponse = null;
            try
            {

                var _messageRequest = new SendMessageRequest();
                _messageRequest.QueueUrl = GetQueueUrl();
                Guid obj = Guid.NewGuid();
                _messageRequest.MessageBody = obj.ToString();
                sendMessageResponse = await _client.SendMessageAsync(_messageRequest);

            }

            catch (Exception ex)
            {
                Console.WriteLine("Caught Exception: " + ex.Message);
            }
            return sendMessageResponse;

        }

        public async Task<SendMessageResponse> SendMessagetoSQS(string messageBody)
        {
            AmazonSQSConfig amazonSQSConfig = new AmazonSQSConfig();
            SendMessageResponse sendMessageResponse = null;
            try
            {

                var _messageRequest = new SendMessageRequest();
                _messageRequest.QueueUrl = GetQueueUrl();
                _messageRequest.MessageBody = messageBody;
                sendMessageResponse = await _client.SendMessageAsync(_messageRequest);

            }

            catch (Exception ex)
            {
                Console.WriteLine("Caught Exception: " + ex.Message);
            }
            return sendMessageResponse;

        }

        public async Task<List<Message>> GetMessagesAsync(string queueName, CancellationToken cancellationToken = default)
        {

            try
            {
                var response = await _client.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    WaitTimeSeconds = _awsConfig.AwsQueueLongPollTimeSeconds,
                    QueueUrl = GetQueueUrl(),
                    AttributeNames = new List<string> { "ApproximateReceiveCount" },
                    MessageAttributeNames = new List<string> { "All" }
                }, cancellationToken); ;

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new AmazonSQSException($"Failed to GetMessagesAsync for queue {queueName}. Response: {response.HttpStatusCode}");
                }

                foreach(Message m in response.Messages)
                {
                    Order order = new Order();
                    order.SerialNumber = m.Body;
                    order.OrderStatus = "Created";
                    order.OrderType = "New";

                    //Update the record in DynamoDB table
                    await transactionRecord.InsertRecord<Order>(order);

                    //Delete the record in SQS
                    await DeleteMessageAsync("NewOrder", m.ReceiptHandle);
                   
                }

                return response.Messages;
            }
            catch (TaskCanceledException)
            {
                return new List<Message>();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task DeleteMessageAsync(string queueName, string receiptHandle)
        {

            try
            {
                var response = await _client.DeleteMessageAsync(GetQueueUrl(), receiptHandle);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new AmazonSQSException($"Failed to DeleteMessageAsync with for [{receiptHandle}] from queue '{queueName}'. Response: {response.HttpStatusCode}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }





    }


    public static class SqsMessageTypeAttribute
    {
        private const string AttributeName = "MessageType";

        public static string GetMessageTypeAttributeValue(this Dictionary<string, MessageAttributeValue> attributes)
        {
            return attributes.SingleOrDefault(x => x.Key == AttributeName).Value?.StringValue;
        }

        public static Dictionary<string, MessageAttributeValue> CreateAttributes<T>()
        {
            return CreateAttributes(typeof(T).Name);
        }

        public static Dictionary<string, MessageAttributeValue> CreateAttributes(string messageType)
        {
            return new Dictionary<string, MessageAttributeValue>
        {
            {
                AttributeName, new MessageAttributeValue
                {
                    DataType = nameof(String),
                    StringValue = messageType
                }
            }
        };
        }
    }

}

