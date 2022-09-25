using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Continuations
{
    class LicenseException : Exception
    {
        public LicenseException(string message) : base(message)
        {

        }
    }
}
