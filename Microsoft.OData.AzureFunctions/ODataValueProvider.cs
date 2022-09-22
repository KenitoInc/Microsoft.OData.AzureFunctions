using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.OData.Edm;

namespace Microsoft.OData.AzureFunctions
{
    public class ODataValueProvider<T> : IValueProvider
    {
        private HttpRequest request;
        private IEdmModelProvider modelProvider;
        private Type clrType;
        private string routePrefix;

        public ODataValueProvider(HttpRequest request, IEdmModelProvider modelProvider, Type clrType)
        {
            this.request = request;
            this.modelProvider = modelProvider;
            this.clrType = clrType;
            this.routePrefix = "api";
        }
        public async Task<object> GetValueAsync()
        {
            try
            {
                // This is where we handle OData related logic using 
                // the IEdmModel, HttpRequest and clrType
                IEdmModel model = this.modelProvider.GetEdmModel();

                // TODO: Add routePrefix to constructor to be passed from the function
                return ODataBindingHelper.BuildODataQueryOptions(this.request, model, this.clrType, this.routePrefix);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Type Type => typeof(object);
        public string ToInvokeString() => string.Empty;
    }
}
