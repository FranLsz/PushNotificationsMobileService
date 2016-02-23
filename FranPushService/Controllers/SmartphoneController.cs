﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using FranPushService.DataObjects;
using FranPushService.Models;
using Microsoft.WindowsAzure.Mobile.Service;

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
        public async Task<IHttpActionResult> PostSmartphone(Smartphone item)
        {
            Smartphone current = await InsertAsync(item);

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "message", item.Modelo}
            };
            GooglePushMessage messageGoogle = new GooglePushMessage(data, TimeSpan.FromHours(1));



            var wns = @"<?xml version=""1.0"" encoding=""utf - 8""?><toast><visual><binding template=""ToastText01""><text id=""1"">Nuevo smartphone</text><text id=""1"">Modelo: " + item.Modelo + @"</text><text id=""1"">Fabricante: " + item.Fabricante + @"</text></binding></visual><actions><action content=""Ver"" arguments=""check"" /><action content=""Descartar"" arguments=""cancel""/></actions></toast>";
            WindowsPushMessage messageUwp = new WindowsPushMessage { XmlPayload = wns };

            try
            {
                // Google
                var resultGoogle = await Services.Push.SendAsync(messageGoogle);
                Services.Log.Info("Google - " + resultGoogle.State.ToString() + " | " + resultGoogle.TrackingId.ToString() + " | " + messageGoogle);

                // UWP
                var resultUwp = await Services.Push.SendAsync(messageUwp);
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
