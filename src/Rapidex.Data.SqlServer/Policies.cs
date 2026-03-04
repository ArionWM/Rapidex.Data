using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Polly;
using Polly.Retry;

namespace Rapidex.Data.SqlServer;

internal class Policies
{
    //See: https://www.pollydocs.org/pipelines/index.html#usage
    public static ResiliencePipeline RetryPipeline { get; private set; }

    static Policies()
    {
        BuildPolicies();
    }

    public static void BuildPolicies()
    {
        var policies = Database.Configuration.PoliciesInfo ?? new PoliciesInfo();
        policies.MaxRetryCount = policies.MaxRetryCount ?? 0;
        policies.MaxRetryCount = policies.MaxRetryCount <= 0 ? 1 : policies.MaxRetryCount;

        policies.WaitForRetryMs = policies.WaitForRetryMs ?? 0;
        policies.WaitForRetryMs = policies.WaitForRetryMs <= 0 ? 1000 : policies.WaitForRetryMs;


        var retryOption = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SqlException>(ex =>
            {
                return
                    ex.Number == 1205 /*deadlock: Transaction (Process ID X) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction. */
                    || ex.Number == 1222 /*deadlock*/
                    || ex.Number == -1 /* The server was not found or was not accessible*/
                    || ex.Number == -2 /*ADO.net timeout*/
                    || ex.Number == 11 /*Network error*/
                    || ex.Number == 3981 /*The transaction operation cannot be performed because there are pending requests working on this transaction.*/
                    || ex.Number == 8642 /*The query processor could not start the necessary thread resources for parallel query execution.*/
                    || ex.Number == 2714 /*There is already an object named (temp file)*/
                    || ex.Message.Contains("broken");//ex.Number == 0 ???
            }),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,  // Adds a random factor to the delay
            MaxRetryAttempts = policies.MaxRetryCount.Value,
            Delay = TimeSpan.FromMilliseconds(policies.WaitForRetryMs.Value),
            OnRetry = (args) =>
            {
                var propKey = new ResiliencePropertyKey<string>("CorrelationId");
                args.Context.Properties.TryGetValue(propKey, out var correlationId);
                Common.DefaultLogger?.LogWarning("{Key} Retrying due to exception: {Message}. Attempt {Attempt}/{MaxAttempts}", correlationId, null, args.AttemptNumber, policies.MaxRetryCount);
                return default;
            }
        };

        RetryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(retryOption)
            .Build();
    }
}
