using System.Web.Mvc;

namespace CP.Azure.Samples.MultiComponentRole.Frontend.Controllers
{
    using System.Linq;
    using CP.Azure.Samples.MultiComponentRole.Model;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(StorageNames.ConnectionStringSettingKey));

            var table = storageAccount
                .CreateCloudTableClient()
                .GetTableReference(StorageNames.EntityTableName);

            var results = table.ExecuteQuery(
                new TableQuery<FileDownloadEntity>());

            return View(results.OrderByDescending(p => p.Timestamp));
        }

    }
}
