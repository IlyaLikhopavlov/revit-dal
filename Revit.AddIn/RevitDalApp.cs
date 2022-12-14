using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Bimdance.Framework.DependencyInjection;
using Bimdance.Framework.DependencyInjection.ScopedServicesFunctionality;
using Microsoft.Extensions.DependencyInjection;
using Revit.AddIn.Commands.Initialization;
using Revit.AddIn.RibbonPanels;
using Revit.DAL.Converters;
using Revit.DAL.Storage.Infrastructure.Model;
using Revit.DAL.Storage.Infrastructure;
using Revit.DAL.Storage;
using Revit.DAL.Storage.Schemas;
using Revit.DAL.Converters.Common;
using Revit.DAL.DataContext;
using Revit.DAL.DataContext.RevitSets;
using Revit.DML;
using Element = Autodesk.Revit.DB.Element;
using Revit.DAL.Processing;
using Revit.Families.Rendering;
using Revit.Services.Allocation;
using Revit.Services.Allocation.Common;

namespace Revit.AddIn
{
    [Transaction(TransactionMode.Manual)]
    internal class RevitDalApp : IExternalApplication
    {
        private readonly MainRibbonPanel _mainPanel = new();

        public static IServiceProvider ServiceProvider { get; set; }

        public Result OnStartup(UIControlledApplication application)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IDocumentServiceScopeFactory, DocumentServiceScopeFactory>();
            serviceCollection.AddTransient<SchemaDescriptor>();

            serviceCollection.AddSingleton(new ApplicationProcessing(application));
            serviceCollection.AddSingleton<FamilyInstanceAllocationService>();
            serviceCollection.AddScoped<ModelItemsAllocationService>();
            serviceCollection.AddScoped<SampleRendering>();

            serviceCollection.AddScoped<ExtensibleStorage<DataSchema>>();
            serviceCollection.AddScoped<ExtensibleStorageDictionary>();
            serviceCollection.AddScoped<IIntIdGenerator, IntIdGenerator>();
            serviceCollection.AddScoped<IExtensibleStorageService, ExtensibleStorageService>();
            serviceCollection.AddScoped<ISchemaDescriptorsRepository, SchemaDescriptorsRepository>();

            serviceCollection.AddScoped<RevitInstanceConverter<Foo, FamilyInstance>, FooConverter>();
            serviceCollection.AddScoped<RevitInstanceConverter<Bar, FamilyInstance>, BarConverter>();

            serviceCollection.AddScoped<BarSet>();
            serviceCollection.AddScoped<FooSet>();

            serviceCollection.AddScoped<IDataContext, DataContext>();

            serviceCollection.AddFactoryFacility();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            _mainPanel.Create(application);

            application.ControlledApplication.ApplicationInitialized += ControlledApplication_ApplicationInitialized;
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            ServiceProvider?.GetService<IDocumentServiceScopeFactory>()?.Dispose();

            return Result.Succeeded;
        }

        private static bool DocumentWasChanged(Element currentActiveView, Element previousActiveView)
        {
            return !Equals(currentActiveView?.Document, previousActiveView?.Document);
        }

        private static void ControlledApplication_ApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            var initCommand = new InitializationCommand();

            switch (sender)
            {
                case UIApplication uiApplication:
                    {
                        uiApplication.ViewActivated += UiControlledApplicationOnViewActivated;
                        initCommand.Execute(uiApplication);
                        OnViewActivatedInternal(uiApplication.ActiveUIDocument.Document,
                            uiApplication.ActiveUIDocument.ActiveView, null);
                    }
                    break;
                case Application application:
                    {
                        var newUiApplication = new UIApplication(application);
                        newUiApplication.ViewActivated += UiControlledApplicationOnViewActivated;
                        initCommand.Execute(newUiApplication);
                        if (newUiApplication.ActiveUIDocument != null)
                        {
                            OnViewActivatedInternal(newUiApplication.ActiveUIDocument.Document,
                                newUiApplication.ActiveUIDocument.ActiveView, null);
                        }
                    }
                    break;
            }
        }

        private static void UiControlledApplicationOnViewActivated(object sender, ViewActivatedEventArgs e)
        {
            OnViewActivatedInternal(e.Document, e.CurrentActiveView, e.PreviousActiveView);
        }

        private static void OnViewActivatedInternal(Document document, Element currentActiveView, Element previousActiveView)
        {
            if (document == null || currentActiveView == null)
            {
                return;
            }

            if (DocumentWasChanged(currentActiveView, previousActiveView))
            {

            }
        }
    }
}
