﻿using System;
using Amazon.Runtime;

namespace SampleReconcileService
{
    public class AwsCredentials : AWSCredentials
    {
        private readonly AppConfig _appConfig;

        public AwsCredentials(AppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public override ImmutableCredentials GetCredentials()
        {
            return new ImmutableCredentials(_appConfig.AwsAccessKey, _appConfig.AwsSecretKey, null);
        }
    }
}
