using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.OData.Edm;
using Newtonsoft.Json;

namespace Microsoft.OData.AzureFunctions
{
    public class ODataValueProvider<T> : IValueProvider
    {
        private HttpRequest request;
        private IEdmModelProvider modelProvider;
        private Type clrType;
        public ODataValueProvider(HttpRequest request, IEdmModelProvider modelProvider, Type clrType)
        {
            this.request = request;
            this.modelProvider = modelProvider;
            this.clrType = clrType;
        }
        public async Task<object> GetValueAsync()
        {
            try
            {
                // This is where we will handle OData related logic using 
                // the IEdmModel, HttpRequest and clrType
                // The code below will be removed.
                IEdmModel model = this.modelProvider.GetEdmModel();
                string requestBody = await new StreamReader(this.request.Body).ReadToEndAsync();
                T result = JsonConvert.DeserializeObject<T>(requestBody);
                return result;
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
