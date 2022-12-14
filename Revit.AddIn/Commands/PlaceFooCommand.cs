using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Bimdance.Framework.DependencyInjection.FactoryFunctionality;
using Bimdance.Framework.DependencyInjection.ScopedServicesFunctionality;
using Microsoft.Extensions.DependencyInjection;
using Revit.Services.Allocation;

namespace Revit.AddIn.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class PlaceFooCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var document = commandData.Application.ActiveUIDocument.Document;

            using var scopeFactory = RevitDalApp.ServiceProvider.GetService<IDocumentServiceScopeFactory>();
            var documentScope = scopeFactory?.CreateDocumentScope(document);
            var allocationService = documentScope?
                .ServiceProvider
                .GetService<IFactory<Document, ModelItemsAllocationService>>()
                ?.New(document);

            allocationService?.AllocateFoo();

            return Result.Succeeded;
        }
    }
}
