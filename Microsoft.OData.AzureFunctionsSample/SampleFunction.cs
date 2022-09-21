using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.OData.AzureFunctions;
using Microsoft.AspNetCore.OData.Query;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OData.AzureFunctionsSample
{
    public static class SampleFunction
    {
        private static IList<Customer> customers = new List<Customer>
        {
            new Customer { Id = 1, Name = "Ruto" },
            new Customer { Id = 2, Name = "Ken" },
            new Customer { Id = 3, Name = "Koko" },
            new Customer { Id = 4, Name = "Smith" }
        };

        [FunctionName("GetCustomers")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            [OData(Model = typeof(EdmModelProvider))] ODataQueryOptions<Customer> options)
        {
            var result = options.ApplyTo(customers.AsQueryable());
            return new OkObjectResult(result);
        }
    }
}
