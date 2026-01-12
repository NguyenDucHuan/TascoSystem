using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.UserAuthService.Repository.DataSeedings
{
    public static class RoleDataSeeding
    {
        private const string UserRoleId = "62f344b0-8cc9-4e78-9047-57af74c367ac";
        private const string AdminRoleId = "b4fefe26-a899-4611-90de-cdf797927add";
        public static void RoleData(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Id = UserRoleId, ConcurrencyStamp = UserRoleId, Name = "User", NormalizedName = "User".ToUpper() },
                new IdentityRole() { Id = AdminRoleId, ConcurrencyStamp = AdminRoleId, Name = "Admin", NormalizedName = "Admin".ToUpper() }
            );
        }
    }
}
