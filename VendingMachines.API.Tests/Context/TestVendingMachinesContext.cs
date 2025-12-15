using Microsoft.EntityFrameworkCore;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Tests.Context;

public class TestVendingMachinesContext : VendingMachinesContext
{
    public TestVendingMachinesContext(DbContextOptions<VendingMachinesContext> options) 
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
}