// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if DEBUG
              Debugger.Launch();
#endif
            var host = new HostBuilder()
                           //<docsnippet_configure_defaults>
                           .ConfigureFunctionsWorkerDefaults()
                           //</docsnippet_configure_defaults>
                           //<docsnippet_dependency_injection>
                           //</docsnippet_dependency_injection>
                           .Build();
            //</docsnippet_startup>
            //<docsnippet_host_run>
            await host.RunAsync();
            //</docsnippet_host_run>
        }
    }
}
