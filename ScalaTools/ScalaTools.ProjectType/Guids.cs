using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ScalaTools
{
    static class Guids
    {
        public const string ScalaPackageString = "6de842fa-c9e0-4a6a-b7df-f077ca3357fd";
        public const string ScalaCmdSetString = "af26140f-c865-4e3a-883b-963382dd0d0a";

        public static readonly Guid ScalaCmdSet = new Guid(ScalaCmdSetString);
    }
}
