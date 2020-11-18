using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitInventorExchange.CoreBusinessLayer
{
    public class UIRelevantException : Exception
    {
        public UIRelevantException()
        {

        }

        public UIRelevantException(string msg) : base(String.Format("{0}", msg))
        {

        }
    }
}
