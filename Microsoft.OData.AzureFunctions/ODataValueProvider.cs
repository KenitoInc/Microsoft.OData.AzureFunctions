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
        public ODataValueProvider(HttpRequest request, IEdmModelProvider modelProvider)
        {
            this.request = request;
            this.modelProvider = modelProvider;
        }
        public async Task<object> GetValueAsync()
        {
            try
            {
                // This is where we will handle OData related logic using 
                // the IEdmModel and HttpRequest
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
