using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SampleReconcileService
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// 
        public SQS_Consumer _client;
        public AppConfig _appConfig = null;
        public TransactionsRecord transactionRecord;



        //Read records from DB.
        //Filter records with Error
        //Push the errorneous records for retry in SQS 
        public async Task< string> FunctionHandler(object input, ILambdaContext context)
        {
            _appConfig = new AppConfig();
            _appConfig.AwsAccessKey = "XXX";
            _appConfig.AwsSecretKey = "YYY";

            AwsCredentials awsCredentials = new AwsCredentials(_appConfig);

            _client = new SQS_Consumer(_appConfig);


            transactionRecord = new TransactionsRecord(awsCredentials);

            await ProcessErrorRecord();

            return "success";

        }

        public async Task<string> ProcessErrorRecord()
        {
            //Read record from DB
            IList<Order> orderList = new List<Order>();

            List<ScanCondition> conditions = new List<ScanCondition>();
            conditions.Add(new ScanCondition("OrderStatus", ScanOperator.Equal, "Failed"));

            //Update the record in DynamoDB table
            orderList = await transactionRecord.GetRows<Order>(conditions);

            foreach(Order o in orderList)
            {
                SendRequest(o.SerialNumber);
            }

            return "success";
        }


        //Send record to SQS for Re-Processing
        public async void SendRequest(string message)
            {
                //Send Sample message to Queue
                await _client.SendMessagetoSQS(message);
                return;
            }

 

         
        }
    }

