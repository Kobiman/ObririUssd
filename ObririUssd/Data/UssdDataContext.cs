using Microsoft.EntityFrameworkCore;
using ObririUssd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Data
{
    public interface IUssdDataContext
    {
        DbSet<UssdTransaction> Trans { get; set; }
        DbSet<TransactionLog> TransactionLogs { get; set; }
        DbSet<UssdLock> UssdLock { get; set; }
    }

    public class UssdDataContext : DbContext, IUssdDataContext
    {
        public UssdDataContext(DbContextOptions<UssdDataContext> options) : base(options)
        {
        }

        public DbSet<UssdTransaction> Trans { get; set; }
        public DbSet<UssdLock> UssdLock { get; set; }
        public DbSet<TransactionLog> TransactionLogs { get; set; }
    }
}
