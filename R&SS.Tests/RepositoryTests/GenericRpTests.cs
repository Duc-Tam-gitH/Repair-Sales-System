using FluentAssertions;
using R_SS.DAL.Repositories;
using R_SS.Models.Entities;
using R_SS.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace R_SS.Tests.RepositoryTests;

public class GenericRepositoryTests
{
    //Tạo nhanh một Khách hàng mẫu (Nguyen Van A - KH001) để dùng chung cho các bài test
    private static Customer CreateCustomer()
    {
        return new Customer
        {
            CustomerCode = "KH001",
            FullName = "Nguyen Van A",
            Phone = "0909123456",
            Email = "a@gmail.com",
            Address = "Vung Tau",
            Notes = "Test Customer",
            IsActive = true
        };
    }

    // ==========================================
    // NHÓM TEST: THÊM MỚI & ĐỌC DỮ LIỆU (CRUD)
    // ==========================================

    // TEST 1: Thêm mới -> Hệ thống phải lưu thành công một khách hàng mới vào cơ sở dữ liệu
    [Fact]
    public async Task AddAsync_Should_Add_New_Customer()
    {
        // Arrange 
        using var context = TestDbContextFactory.Create();
        var repository = new GenericRp<Customer>(context);
        var customer = CreateCustomer();

        // Act 
        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        // Assert 
        var result = await repository.GetAllAsync();
        result.Should().HaveCount(1);
        result.First().CustomerCode.Should().Be("KH001"); 
    }

    // TEST 2: Lấy tất cả -> Hệ thống phải trả về danh sách chứa đầy đủ toàn bộ khách hàng đang có
    [Fact]
    public async Task GetAllAsync_Should_Return_All_Customers()
    {
        // Arrange 
        using var context = TestDbContextFactory.Create();
        var repository = new GenericRp<Customer>(context);

        await repository.AddAsync(CreateCustomer());
        await repository.AddAsync(new Customer
        {
            CustomerCode = "KH002",
            FullName = "Tran Van B"
        });
        await context.SaveChangesAsync();

        // Act 
        var customers = await repository.GetAllAsync();

        // Assert 
        customers.Should().HaveCount(2); 
    }

    // TEST 3: Lấy theo ID -> Hệ thống phải tìm và trả về đúng thông tin khách hàng dựa trên ID truyền vào
    [Fact]
    public async Task GetByIdAsync_Should_Return_Customer()
    {
        // Arrange 
        using var context = TestDbContextFactory.Create();
        var repository = new GenericRp<Customer>(context);
        var customer = CreateCustomer();

        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        // Act 
        var result = await repository.GetByIdAsync(customer.CustomerId);

        // Assert 
        result.Should().NotBeNull(); // Kết quả: Tìm phải thấy (không được trả về null)
        result!.FullName.Should().Be("Nguyen Van A"); // Kết quả: Tên của người tìm thấy phải là "Nguyen Van A"
    }

    // ==========================================
    // NHÓM TEST: CẬP NHẬT & XÓA DỮ LIỆU
    // ==========================================

    // TEST 4: Cập nhật -> Hệ thống phải lưu lại được những thay đổi thông tin của khách hàng
    [Fact]
    public async Task Update_Should_Update_Customer()
    {
        // Arrange 
        using var context = TestDbContextFactory.Create();
        var repository = new GenericRp<Customer>(context);
        var customer = CreateCustomer();

        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        
        customer.FullName = "Nguyen Van B";

        // Act 
        repository.Update(customer);
        await context.SaveChangesAsync();

        // Assert 
        var updated = await repository.GetByIdAsync(customer.CustomerId);
        updated.Should().NotBeNull();
        updated!.FullName.Should().Be("Nguyen Van B"); // Kết quả: Tên trong DB lúc này phải đổi thành "Nguyen Van B"
    }

    // TEST 5: Xóa bỏ -> Hệ thống phải xóa hoàn toàn bản ghi khách hàng khỏi cơ sở dữ liệu
    [Fact]
    public async Task Delete_Should_Remove_Customer()
    {
        // Arrange 
        using var context = TestDbContextFactory.Create();
        var repository = new GenericRp<Customer>(context);
        var customer = CreateCustomer();

        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        // Act 
        repository.Delete(customer);
        await context.SaveChangesAsync();

        // Assert 
        var result = await repository.GetAllAsync();
        result.Should().BeEmpty(); // Kết quả: Danh sách khách hàng bây giờ phải trống rỗng (đã xóa hết)
    }

    // ==========================================
    // NHÓM TEST: TÌM KIẾM & KIỂM TRA TỒN TẠI
    // ==========================================

    // TEST 6: Tìm kiếm theo điều kiện -> Hệ thống phải lọc và trả về đúng những khách hàng khớp với điều kiện tìm kiếm
    [Fact]
    public async Task FindAsync_Should_Return_Matching_Customers()
    {
        // Arrange (
        using var context = TestDbContextFactory.Create();
        var repository = new GenericRp<Customer>(context);

        await repository.AddAsync(CreateCustomer());
        await repository.AddAsync(new Customer
        {
            CustomerCode = "KH002",
            FullName = "Tran Van B"
        });
        await context.SaveChangesAsync();

        // Act 
        var result = await repository.FindAsync(x => x.CustomerCode == "KH001");

        // Assert 
        result.Should().HaveCount(1); 
        result.First().FullName.Should().Be("Nguyen Van A"); 
    }

    // TEST 7: Kiểm tra tồn tại (Trường hợp CÓ) -> Phải trả về TRUE khi tìm kiếm một khách hàng thực sự có trong DB
    [Fact]
    public async Task AnyAsync_Should_Return_True_When_Customer_Exists()
    {
        // Arrange 
        using var context = TestDbContextFactory.Create();
        var repository = new GenericRp<Customer>(context);

        await repository.AddAsync(CreateCustomer());
        await context.SaveChangesAsync();

        // Act 
        var exists = await repository.AnyAsync(x => x.CustomerCode == "KH001");

        // Assert 
        exists.Should().BeTrue(); // Kết quả: Hệ thống phải báo Đúng (True)
    }

    // TEST 8: Kiểm tra tồn tại (Trường hợp KHÔNG CÓ) -> Phải trả về FALSE khi tìm kiếm một mã khách hàng không tồn tại
    [Fact]
    public async Task AnyAsync_Should_Return_False_When_Customer_Does_Not_Exist()
    {
        // Arrange 
        using var context = TestDbContextFactory.Create();
        var repository = new GenericRp<Customer>(context);

        // Act 
        var exists = await repository.AnyAsync(x => x.CustomerCode == "KH999");

        // Assert 
        exists.Should().BeFalse(); // Kết quả: Hệ thống phải báo Sai/Không tồn tại (False)
    }
}