using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ScalaTools
{
    class PkgCmdId
    {
        public const uint cmdidSmartExecute = 0x103;
        public const uint cmdidBreakRepl = 0x104;
        public const uint cmdidResetRepl = 0x105;
        public const uint cmdidReplHistoryNext = 0x0106;
        public const uint cmdidReplHistoryPrevious = 0x0107;
        public const uint cmdidReplClearScreen = 0x0108;
        public const uint cmdidBreakLine = 0x0109;
        public const uint cmdidReplSearchHistoryNext = 0x010A;
        public const uint cmdidReplSearchHistoryPrevious = 0x010B;
        public const uint menuIdReplToolbar = 0x2000;

        public const uint comboIdReplScopes = 0x3000;
        public const uint comboIdReplScopesGetList = 0x3001;

        public const int cmdidReplWindow = 0x200;
    }
}
