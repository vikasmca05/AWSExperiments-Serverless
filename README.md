# AWSExperiments-Serverless
Serverless Experiment


Problem Statement - To retry the failure records which has failed with specific error codes.

Solution Approach - During the Original service request, we would log all the failure scenarios in the table of DynamoDB with Error code. Filter the records with error code which need to be retried and maximum retry count has not exceeded. Create the message in SQS for original service to pick it up.
Create a AWS lambda application. Application would be woke up periodically by CloudWatch event. Lambda will read the filtered records from Dynamo DB table. And then record will be entered back into SQS standard queue.
 
 ![High Level Design](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/Lambda.png)


Technology Stack – 
1.	Application – AWS Serverless Lambda Application
2.	Messaging Platform - SQS Standard Queue
3.	Database  - Dynamo DB Table Deployment
4.	Platform - DotNet Core

Implementation Details -
1. Create AWS Serverless c# web application
2. Add AWS SDK for SQS and DynamoDB.
3. Add the helper class for SQS operation and DynamoDB operation.
4. Run Lambda locally using AWS Mock Dot Net SDK Tool.
 ![AWS Mock Dotnet SDK Tool](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/Mock-1.PNG)
5. Test and if it works fine then push to AWS by right clicking the project and selecting option for Publish.
 ![AWS Publish](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/Publish-1.PNG)
6. Name the function and fill other details.
![AWS Publish](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/Publishing-2.PNG)
7. Just after publishing, local window can be used to test as well.
![AWS Publish](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/Test-after-Publish.PNG)
8. Success page after Publish is complete.
![AWS Publish](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/aws-console-lambda.PNG)
9. Create alarm in Cloudwatch to trigger Lambda.
![Cloudwatch Alarm](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/Add-trigger-2.PNG)
10.	Configure Test event to check the Lambda flow from console.
![Test Event](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/Configure-Test-Event-1.PNG)
12.	Lambda will read the filtered records from DynamoDB table.Enter the record in SQS queue. Success response in console logs.
![Success logs](https://github.com/vikasmca05/AWSExperiments-Serverless/blob/master/Lambda/SampleReconcileService/Success-1.PNG)
