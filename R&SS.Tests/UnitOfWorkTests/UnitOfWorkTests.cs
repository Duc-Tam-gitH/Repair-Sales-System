using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;
using R_SS.Tests.Helpers;

namespace R_SS.Tests.UnitOfWorkTests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_Should_Save_New_Customer()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        using var unitOfWork = new UnitOfWork(context);

        var customer = new Customer
        {
            CustomerCode = "KH001",
            FullName = "Nguyen Van A",
            Phone = "0909123456",
            Email = "a@gmail.com"
        };

        // Act
        await unitOfWork.Customers.AddAsync(customer);
        await unitOfWork.SaveChangesAsync();

        var customers = await unitOfWork.Customers.GetAllAsync();

        // Assert
        customers.Should().HaveCount(1);
    }

    [Fact]
    public void SaveChanges_Should_Save_New_Customer()
    {
        using var context = TestDbContextFactory.Create();
        using var unitOfWork = new UnitOfWork(context);

        var customer = new Customer
        {
            CustomerCode = "KH002",
            FullName = "Tran Van B"
        };

        unitOfWork.Customers.AddAsync(customer).GetAwaiter().GetResult();

        unitOfWork.SaveChanges();

        var customers = unitOfWork.Customers.GetAllAsync().GetAwaiter().GetResult();

        customers.Should().HaveCount(1);
    }

    //Transaction Test để sau, khi có API và SQL sever thật
}
