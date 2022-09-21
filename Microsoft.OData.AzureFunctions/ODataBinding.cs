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
            HttpRequest request = context.BindingData["req"] as HttpRequest;
            IEdmModelProvider modelProvider = (IEdmModelProvider)Activator.CreateInstance(attribute.Model);

            // this.GetType is ODataBinding<ODataQueryOptions<T>>

            // This code extracts ODataQueryOptions<T> from ODataBinding<ODataQueryOptions<T>>
            Type odataBindingArgumentType = this.GetType().GenericTypeArguments.First();

            // TODO: validation if the type is ODataQueryOptions<T>
            // Custom logic if we will also be supporting IQueryable<T>

            // This code extracts T from ODataQueryOptions<T>
            Type type = odataBindingArgumentType.GenericTypeArguments.First();

            return Task.FromResult<IValueProvider>(new ODataValueProvider<T>(request, modelProvider, type));
        }

        public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor();
    }
}
