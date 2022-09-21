using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.Json;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;
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

                return BuildODataQueryOptions(this.request, model, this.clrType);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Type Type => typeof(object);
        public string ToInvokeString() => string.Empty;

        private static ODataQueryOptions BuildODataQueryOptions(HttpRequest httpRequest, IEdmModel model, Type elementClrType)
        {
            ConfigureOData(httpRequest, model);
            ODataQueryContext queryContext = new ODataQueryContext(model, elementClrType, httpRequest.ODataFeature().Path);

            Type queryOptionsType = typeof(ODataQueryOptions<>).MakeGenericType(elementClrType);

            return (ODataQueryOptions)Activator.CreateInstance(queryOptionsType, queryContext, httpRequest);
        }

        private static void ConfigureOData(HttpRequest httpRequest, IEdmModel model)
        {
            // TODO: Not hardcode routePrefix
            string routePrefix = "api";
            ODataOptions options = new ODataOptions();
            options.AddRouteComponents(routePrefix, model);
            httpRequest.ODataFeature().Services = options.GetRouteServices(routePrefix);

            // Path
            // NOTE: Path should be initialized for each request
            SetODataPath(httpRequest, model, routePrefix);

            // Model
            // Required
            SetEdmModel(httpRequest, model);
        }

        // Just in case AddOData/AddRouteComponents injects services with lifetimes relevant to Azure functions
        private static void AddODataVerbose(HttpRequest httpRequest, IEdmModel model)
        {
            var services = new ServiceCollection();
            services.AddMvcCore().AddOData();

            // services.AddSingleton<ODataPathHandler, DefaultODataPathHandler>();

            // ReaderSettings and WriterSettings are registered as prototype services.
            // There will be a copy (if it is accessed) of each prototype for each request.
            services.AddTransient(sp => new ODataMessageReaderSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });
            services.AddTransient(sp => new ODataMessageWriterSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });

            services.AddTransient<IETagHandler, NoOpETagHandler>();

            services.AddTransient<ODataQuerySettings>();
            services.AddSingleton(sp => model);
            services.AddTransient(sp => new ODataUriResolver { EnableCaseInsensitive = true });
            services.AddTransient<ODataQueryValidator>();
            services.AddTransient<TopQueryValidator>();
            services.AddTransient<FilterQueryValidator>();
            services.AddTransient<SkipQueryValidator>();
            services.AddTransient<OrderByQueryValidator>();
            services.AddTransient<CountQueryValidator>();
            services.AddTransient<SelectExpandQueryValidator>();
            services.AddTransient<ODataRawValueSerializer>();
            services.AddTransient<ODataMetadataSerializer>();
            services.AddTransient<ODataErrorSerializer>();
            services.AddTransient<ODataResourceSetSerializer>();
            services.AddTransient<ODataEntityReferenceLinksSerializer>();
            services.AddTransient<ODataEntityReferenceLinkSerializer>();
            services.AddTransient<ODataServiceDocumentSerializer>();

            services.AddSingleton<SkipTokenHandler, DefaultSkipTokenHandler>();
            services.AddTransient<FilterBinder>();

            // SerializerProvider and DeserializerProvider.
            services.AddSingleton<IODataSerializerProvider, ODataSerializerProvider>();
            services.AddSingleton<IODataDeserializerProvider, ODataDeserializerProvider>();

            // Deserializers.
            services.AddSingleton<ODataResourceDeserializer>();
            services.AddSingleton<ODataEnumDeserializer>();
            services.AddSingleton<ODataPrimitiveDeserializer>();
            services.AddSingleton<ODataResourceSetDeserializer>();
            services.AddSingleton<ODataDeltaResourceSetDeserializer>();
            services.AddSingleton<ODataCollectionDeserializer>();
            services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
            services.AddSingleton<ODataActionPayloadDeserializer>();

            // Serializers.
            services.AddSingleton<ODataEnumSerializer>();
            services.AddSingleton<ODataPrimitiveSerializer>();
            services.AddSingleton<ODataResourceSerializer>();
            services.AddSingleton<ODataResourceSetSerializer>();
            services.AddSingleton<ODataDeltaResourceSetSerializer>();
            services.AddSingleton<ODataCollectionSerializer>();
            services.AddSingleton<ODataServiceDocumentSerializer>();
            services.AddSingleton<ODataEntityReferenceLinkSerializer>();
            services.AddSingleton<ODataEntityReferenceLinksSerializer>();
            services.AddSingleton<ODataErrorSerializer>();
            services.AddSingleton<ODataMetadataSerializer>();
            services.AddSingleton<ODataRawValueSerializer>();
            services.AddSingleton<ODataMetadataSerializer>();

            services.AddTransient<ODataUriParserSettings>();
            services.AddScoped<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient(typeof(ODataSimplifiedOptions), sp => new ODataSimplifiedOptions(ODataVersion.V4));
            services.AddTransient<UriPathParser>();

            //     services.AddSingleton<IJsonReaderFactory, DefaultJsonReaderFactory>();
            services.AddSingleton<IJsonWriterFactory>(sp => new DefaultJsonWriterFactory());
            services.AddSingleton<IJsonWriterFactoryAsync>(sp => new DefaultJsonWriterFactory());
            services.AddSingleton(sp => new ODataMediaTypeResolver());
            services.AddTransient<ODataMessageInfo>();
            services.AddSingleton(sp => new ODataPayloadValueConverter());

            // Default Query Settings
            services.AddTransient<DefaultQuerySettings>();

            httpRequest.ODataFeature().Services = services.BuildServiceProvider();

            string routePrefix = "api";

            // Path
            SetODataPath(httpRequest, model, routePrefix);

            // Model
            SetEdmModel(httpRequest, model);
        }

        private static void SetODataPath(HttpRequest httpRequest, IEdmModel model, string routePrefix)
        {
            httpRequest.ODataFeature().RoutePrefix = routePrefix;

            Uri serviceRoot = new Uri($"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/{routePrefix}/");
            string odataPath = httpRequest.Path.Value.Substring($"/{routePrefix}/".Length);
            ODataUriParser parser = new ODataUriParser(model, serviceRoot, new Uri(odataPath, UriKind.Relative), httpRequest.ODataFeature().Services);
            ODataPath path = parser.ParsePath();
            httpRequest.ODataFeature().Path = path;
        }

        private static void SetEdmModel(HttpRequest httpRequest, IEdmModel model)
        {
            httpRequest.ODataFeature().Model = model;
        }
    }

    public class NoOpETagHandler : IETagHandler
    {
        public EntityTagHeaderValue CreateETag(IDictionary<string, object> properties, TimeZoneInfo timeZoneInfo = null)
        {
            return null;
        }

        public IDictionary<string, object> ParseETag(EntityTagHeaderValue etagHeaderValue)
        {
            return null;
        }
    }
}
