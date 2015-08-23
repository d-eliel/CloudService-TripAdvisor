using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TripCommon;

namespace TripWebRole.Models
{
    public class ManageAdvisorCache
    {
        public List<TripAdvice> GetAdvisesList()
        {
            DataCache cache = new DataCache("default");
            List<TripAdvice> advises;

            object obj = cache.Get("AdvisesList");
            if (obj == null)
            {
                // Products list is not in cache. Obtain it from the database
                TripDbContext db = new TripDbContext();
                advises = db.TripAdviceTable.ToList();
                cache.Add("AdvisesList", advises);
            }
            else
            {
                // Products list is in cache, cast result to correct type.
                advises = (List<TripAdvice>) obj;
            }
            return advises;
        }

    }
}