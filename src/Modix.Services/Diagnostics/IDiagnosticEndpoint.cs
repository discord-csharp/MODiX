using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modix.Services.Diagnostics
{
    public interface IDiagnosticEndpoint
    {
        string DisplayName { get; }
    }
}
