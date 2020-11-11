using RevitInventorExchange.CoreBusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RevitInventorExchange
{
    public static class RetryHelper
    {
        //  This solution requires a timeout set as Task.Delay
        public async static Task<ForgeRestResponse> Retry(int numberOfRetry, int timeDelay, Func<Dictionary<string, string>, Task<ForgeRestResponse>> function, Dictionary<string, string> parameters)
        {
            NLogger.LogText($"Entered Retry for function {function.Method.Name}");

            var asyncHTTPCallWaitTime = ConfigUtilities.GetAsyncHTTPCallWaitTime();
            int counter = 1;
            int timeDel = timeDelay;

            NLogger.LogText($"Number of retries: {numberOfRetry}");
            NLogger.LogText($"Time to wait between retries of HTTP calls: {timeDelay} ms");
            NLogger.LogText($"Time to wait for HTTP Call: {asyncHTTPCallWaitTime} ms");
            
            while (counter <= numberOfRetry)
            {
                NLogger.LogText($"Attempt number {counter.ToString()}");

                //  Call function and create a Delay task
                var functionCallTask = function(parameters);
                var timeoutTask = Task.Delay(asyncHTTPCallWaitTime);

                //  Get the first task which finishes
                var completedTask = await Task.WhenAny(functionCallTask, timeoutTask);

                //  Based on which one finishes before, go along or retry
                if (completedTask == functionCallTask)
                {                                         
                    var ret = await functionCallTask;

                    NLogger.LogText($"Suceeded attempt number {counter.ToString()}");
                    NLogger.LogText("Exit Retry sucessfully");

                    return ret;
                }
                else
                {                    
                    NLogger.LogText($"Failed attempt number {counter.ToString()} - HTTP Call timed-out");
                    //Thread.Sleep(timeDelay);
                    await Task.Delay(timeDel);

                    timeDel = timeDel + timeDel;
                }

                counter++;
            }

            NLogger.LogText("Exit Retry with error");

            throw new Exception("HTTP invocation failed");
        }

        //  This solution requires a timeout set on the HTTPClient
        public async static Task<ForgeRestResponse> Retry_ALT_APPROACH(int numberOfRetry, int timeDelay, Func<Dictionary<string, string>, Task<ForgeRestResponse>> function, Dictionary<string, string> parameters)
        {
            NLogger.LogText($"Entered Retry for function {function.Method.Name}");

            int counter = 1;
            while (counter <= numberOfRetry)
            {
                var functionCallTask = await function(parameters);
                                

                if (functionCallTask.IsSuccessStatusCode())
                {
                    //var checkIfSuceeded = functionCallTask.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

                    NLogger.LogText("Exit Retry sucessfully");

                    return functionCallTask;
                }
                else
                {
                    NLogger.LogText($"Failed attempt number {counter.ToString()}");
                    //Thread.Sleep(timeDelay);
                    await Task.Delay(timeDelay);
                }

                counter++;
            }

            NLogger.LogText("Exit Retry with error");

            throw new Exception("HTTP invocation failed");
        }
    }
}
