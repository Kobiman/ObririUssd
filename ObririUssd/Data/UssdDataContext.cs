using Microsoft.EntityFrameworkCore;
using ObririUssd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Data
{
    public class UssdDataContext : DbContext
    {
        public UssdDataContext(DbContextOptions<UssdDataContext> options) : base(options)
        {
        }

        public DbSet<UssdTransaction> Trans { get; set; }
    }
}
