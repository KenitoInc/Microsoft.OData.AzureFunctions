using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.OData.AzureFunctions
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class ODataAttribute : Attribute
    {
        public Type Model { get; set; }
    }
}