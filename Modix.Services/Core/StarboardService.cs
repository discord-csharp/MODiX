using System;
using System.Threading.Tasks;
using Serilog;

namespace Modix.Services.Core
{
    public class StarboardService : BehaviorBase
    {
        public StarboardService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        internal protected override Task OnStartingAsync()
        {


            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        internal protected override Task OnStoppedAsync()
        {




            return Task.CompletedTask;
            //throw new NotImplementedException();
        }
    }
}
