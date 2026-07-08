using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using R_SS.DAL.Data;


namespace R_SS.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
           .UseInMemoryDatabase(Guid.NewGuid().ToString())
           .Options;

        return new AppDbContext(options);
    }
}
