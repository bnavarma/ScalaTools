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
        public const string ScalaLanguageInfoString = "5F6789D8-49C2-40E9-AD84-B753208F0733";
        public const string ScalaDebugLanguageString = "B7774E5D-8081-425C-9BBC-49FCC8433604";

        public static readonly Guid ScalaCmdSet = new Guid(ScalaCmdSetString);
        public static readonly Guid ScalaDebugLanguage = new Guid(ScalaDebugLanguageString);
    }
}
