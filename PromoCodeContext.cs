using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodeService.Models
{
    public class PromoCodeContext : DbContext
    {
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<RedeemedCode> RedeemedCodes { get; set; }

        public PromoCodeContext(DbContextOptions<PromoCodeContext> options)
            : base(options)
        {

        }
    }
}
