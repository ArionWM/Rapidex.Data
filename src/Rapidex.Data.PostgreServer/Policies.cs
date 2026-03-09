using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Polly;
using Polly.Retry;

namespace Rapidex.Data.PostgreServer;

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

        // See: https://www.postgresql.org/docs/current/errcodes-appendix.html
        var retryOption = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<PostgresException>(ex =>
                {
                    return ex.SqlState switch
                    {
                        "40001" => true, // serialization_failure
                        "40P01" => true, // deadlock_detected
                        "08001" => true, // sqlclient_unable_to_establish_sqlconnection
                        "08003" => true, // connection_does_not_exist
                        "08006" => true, // connection_failure
                        "57P01" => true, // admin_shutdown
                        "57P02" => true, // crash_shutdown
                        "57P03" => true, // cannot_connect_now
                        "53300" => true, // too_many_connections
                        _ => ex.Message.Contains("broken")
                    };
                })
                .Handle<NpgsqlException>(ex => ex is not PostgresException && (
                    ex.InnerException is TimeoutException
                    || ex.Message.Contains("broken")
                    || ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                )),
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
