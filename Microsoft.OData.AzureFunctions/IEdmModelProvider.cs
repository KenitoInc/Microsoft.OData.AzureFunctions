using Microsoft.OData.Edm;

namespace Microsoft.OData.AzureFunctions
{
    public interface IEdmModelProvider
    {
        EdmModel GetEdmModel();
    }
}
