using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* ******************************* *
 *      Eliel Dabush 204280036     *
 * ****************************** */
namespace TripCommon
{
    public class TripDbContext : DbContext
    {
        public TripDbContext()
            : base("name=TripDbConnString")
        {}

        public TripDbContext(string connString)
            : base (connString)
        {}
        public DbSet<TripAdvice> TripAdviceTable { get; set; }
    }
}
