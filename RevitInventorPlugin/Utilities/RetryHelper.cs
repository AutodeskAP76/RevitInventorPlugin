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
        public static ForgeRestResponse Retry(int numberOfRetry, int timeDelay, Func<Dictionary<string, string>, Task<ForgeRestResponse>> function, Dictionary<string, string> parameters)
        {
            NLogger.LogText($"Entered Retry for function {function.Method.Name}");

            int counter = 1;
            while (counter <= numberOfRetry)
            {
                var ret = function(parameters);
                var checkIfSuceeded = ret.Wait(ConfigUtilities.GetAsyncHTTPCallWaitTime());

                if (checkIfSuceeded)
                {
                    NLogger.LogText("Exit Retry sucessfully");

                    return ret.Result;
                }
                else
                {
                    NLogger.LogText($"Failed attempt number {counter.ToString()}");
                    Thread.Sleep(timeDelay);
                    //Task.Delay(timeDelay);
                }

                counter++;
            }

            NLogger.LogText("Exit Retry with error");

            throw new Exception("HTTP invocation failed");
        }        
    }
}
