using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.UserAuthService.Repository.DataSeedings;

namespace Tasco.UserAuthService.Repository
{
    public class TascoAuthDBContext : IdentityDbContext
    {
        public TascoAuthDBContext(DbContextOptions<TascoAuthDBContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.RoleData();
        }
    }
}
