using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange
{
    public class DAEventHandlerUtilities
    {
        public event EventHandler<string> DACurrentStepHandler;       
               
        public void TriggerDACurrentStepHandler(string text)
        {
            DACurrentStepHandler?.Invoke(this, text);
        }

    }
}
