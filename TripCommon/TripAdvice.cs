using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* ******************************* *
 *      Eliel Dabush 204280036     *
 * ****************************** */
namespace TripCommon
{
    public class TripAdvice
    {
        public int TripAdviceID { get; set; }
        public string UserName { get; set; }
        public string AdviceType { get; set; }
        public string PlaceName { get; set; }
        public string GeoLocation { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string AdviceText { get; set; }
        public string RankPlace { get; set; }
        public string ImageURL { get; set; }
        public string ThumbnailURL { get; set; }
        public DateTime AdviceDate { get; set; }

    }
}
