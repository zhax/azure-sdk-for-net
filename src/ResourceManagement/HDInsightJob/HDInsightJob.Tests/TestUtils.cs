﻿//
// Copyright (c) Microsoft.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Net.Http;
using Hyak.Common;
using Microsoft.Azure.Management.HDInsight.Job;
using Microsoft.Azure.Test.HttpRecorder;

namespace HDInsightJob.Tests
{
    public static class TestUtils
    {
        public static string ClusterName = "hdisdk-resize1.hdinsight-stable.azure-test.net";
        public static string UserName = "hadoopuser";
        public static string Password = "Password1!";
        public static string StorageAccountName = "giyerwestus1";//".blob.core.windows.net";
        public static string StorageAccountKey = "O9EQvp3A3AjXq/W27rst1GQfLllhp01qlJMJfSU1hVW2K42gUeiUUn2D8zX2lU3taiXSSfqkZlcPv+nQcYUxYw==";
        public static string DefaultContainer = "giyertestcsmv2";

        public static HDInsightJobManagementClient GetHDInsightJobManagementClient(string dnsName, BasicAuthenticationCloudCredentials creds)
        {
            var client = new HDInsightJobManagementClient(dnsName, creds);
            return AddMockHandler(ref client);
        }

        private static T AddMockHandler<T>(ref T client) where T : class
        {
            HttpMockServer server;

            try
            {
                server = HttpMockServer.CreateInstance();
            }
            catch (ApplicationException)
            {
                // mock server has never been initialized, we will need to initialize it.
                HttpMockServer.Initialize("TestEnvironment", "InitialCreation");
                server = HttpMockServer.CreateInstance();
            }
            
            var method = typeof(T).GetMethod("WithHandler", new Type[] { typeof(DelegatingHandler) });
            client = method.Invoke(client, new object[] { server }) as T;
            
            if (HttpMockServer.Mode != HttpRecorderMode.Playback) return client;
            
            var initialTimeout = typeof(T).GetProperty("LongRunningOperationInitialTimeout", typeof(int));
            var retryTimeout = typeof(T).GetProperty("LongRunningOperationRetryTimeout", typeof(int));
            if (initialTimeout == null || retryTimeout == null) return client;
            
            initialTimeout.SetValue(client, 0);
            retryTimeout.SetValue(client, 0);
            return client;
        }
    }
}
