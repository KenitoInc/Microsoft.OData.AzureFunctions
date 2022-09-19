using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Microsoft.OData.AzureFunctions
{
    public class ODataBinding<T> : IBinding
    {
        private readonly ODataAttribute attribute;
        public ODataBinding(ODataAttribute attribute)
        {
            this.attribute = attribute;
        }
        bool IBinding.FromAttribute => true;

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            return null;
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            // Get the HTTP request
            var request = context.BindingData["req"] as HttpRequest;
            IEdmModelProvider modelProvider = (IEdmModelProvider)Activator.CreateInstance(attribute.Model);

            return Task.FromResult<IValueProvider>(new ODataValueProvider<T>(request, modelProvider));
        }

        public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor();
    }
}
