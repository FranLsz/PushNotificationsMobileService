using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using FranPushService.DataObjects;
using FranPushService.Models;
using Microsoft.ServiceBus.Notifications;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Notifications;
using Newtonsoft.Json;

namespace FranPushService.Controllers
{
    public class SmartphoneController : TableController<Smartphone>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            FranPushContext context = new FranPushContext();
            DomainManager = new EntityDomainManager<Smartphone>(context, Request, Services);
        }

        // GET tables/Smartphone
        public IQueryable<Smartphone> GetAllSmartphones()
        {
            return Query();
        }

        // GET tables/Smartphone/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Smartphone> GetSmartphone(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Smartphone/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Smartphone> PatchSmartphone(string id, Delta<Smartphone> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Smartphone
        public async Task<IHttpActionResult> PostSmartphone(Smartphone smartphone)
        {
            Smartphone current = await InsertAsync(smartphone);


            // Android (Google)
            var dataGoogle = new Dictionary<string, string>()
            {
                { "mensaje", JsonConvert.SerializeObject(smartphone)}
            };
            var mensajeGoogle = new GooglePushMessage(dataGoogle, TimeSpan.FromHours(1));

            // UWP
            var dataUwp = @"<?xml version=""1.0"" encoding=""utf - 8""?>
<toast>
    <visual>
        <binding template=""ToastText01"">
            <text id=""1"">Nuevo smartphone</text>
            <text id=""1"">Modelo: " + smartphone.Modelo + @"</text>
            <text id=""1"">Fabricante: " + smartphone.Fabricante + @"</text>
        </binding>
    </visual>
<actions>
    <action content=""Ver"" arguments=""check"" />
    <action content=""Descartar"" arguments=""cancel""/>
</actions>
</toast>";
            var mensajeUwp = new WindowsPushMessage { XmlPayload = dataUwp };

            // Tags
            List<string> tags = new List<string> { "NuevoSmartphone" };


            try
            {
                // Google
                var resultGoogle = await Services.Push.SendAsync(mensajeGoogle, tags);
                Services.Log.Info("Google - " + resultGoogle.State.ToString() + " | " + resultGoogle.TrackingId.ToString() + " | " + mensajeGoogle);

                // UWP
                var resultUwp = await Services.Push.SendAsync(mensajeUwp, tags); ;
                Services.Log.Info("Microsoft - " + resultUwp.State.ToString() + " | " + resultUwp.TrackingId.ToString());
            }
            catch (Exception ex)
            {
                Services.Log.Error(ex.Message, null, "Push.SendAsync Error");
            }

            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Smartphone/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteSmartphone(string id)
        {
            return DeleteAsync(id);
        }
    }
}
