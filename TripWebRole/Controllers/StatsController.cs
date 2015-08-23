using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TripCommon;
using TripWebRole.Models;

namespace TripWebRole.Controllers
{
    public class StatsController : Controller
    {
        // GET: Stats
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetStats()
        {
            ManageAdvisorCache advisorCache = new ManageAdvisorCache();
            List<TripAdvice> list = advisorCache.GetAdvisesList();
            IEnumerable<IGrouping<string, string>> cityGB = list.GroupBy(a => a.City, a => a.City);
            IEnumerable<IGrouping<string, string>> countyGB = list.GroupBy(a => a.Country, a => a.Country);
            IEnumerable<IGrouping<string, string>> typeGB = list.GroupBy(a => a.AdviceType, a => a.AdviceType);
            IEnumerable<IGrouping<string, string>> monthGB = list.GroupBy(a => a.AdviceDate.Month.ToString(), a => a.AdviceDate.Month.ToString());

            IEnumerable<IGrouping<string, string>>[] gbArray = { cityGB, countyGB, typeGB, monthGB };
            return Json(gbArray, JsonRequestBehavior.AllowGet);
        }
    }
}