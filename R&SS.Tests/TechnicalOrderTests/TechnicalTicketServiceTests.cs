using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using R_SS.BLL.Constants;
using R_SS.BLL.DTOs.TechnicalOrder;
using R_SS.BLL.Exceptions;
using R_SS.BLL.Interfaces;
using R_SS.BLL.Services;
using R_SS.BLL.Validators.TechnicalOrder;
using R_SS.DAL.Repositories.Interfaces;
using R_SS.DAL.UnitOfWork;
using R_SS.Models.Entities;

namespace R_SS.Tests.TechnicalOrderTests;

public class TechnicalTicketServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateTicketAndSendNotification_WhenRequestIsValid()
    {
        var mocks = CreateMocks();
        var customer = BuildCustomer();
        mocks.Customers.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(customer);
        var service = CreateService(mocks);

        var response = await service.CreateAsync(BuildCreateRequest());

        response.Status.Should().Be("Pending Reception");
        response.TicketCode.Should().StartWith("TT-");
        mocks.RepairOrders.Verify(repo => repo.AddAsync(It.Is<RepairOrder>(order =>
            order.RequestType == "Repair" && order.StatusHistories.Count == 1)), Times.Once);
        mocks.EmailSender.Verify(sender => sender.SendTechnicalTicketCreatedAsync(customer.Email!, customer.FullName, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRequiredDeviceInfoIsMissing()
    {
        var service = CreateService(CreateMocks());
        var request = BuildCreateRequest();
        request.DeviceType = string.Empty;

        var act = async () => await service.CreateAsync(request);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(error => error.PropertyName == nameof(CreateTechnicalTicketRequest.DeviceType));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthorizedException_WhenActorRoleIsCustomer()
    {
        var service = CreateService(CreateMocks());
        var request = BuildCreateRequest();
        request.ActorRole = RoleConstants.Client;

        var act = async () => await service.CreateAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Only Receptionist or Manager roles can perform this function.");
    }

    [Fact]
    public async Task GetTicketsAsync_ShouldReturnNoDataMessage_WhenNoTicketsFound()
    {
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetVisibleTicketsAsync(RoleConstants.Manager, 2, null))
            .ReturnsAsync(Array.Empty<RepairOrder>());
        var service = CreateService(mocks);

        var response = await service.GetTicketsAsync(new ViewTechnicalTicketsRequest { ActorUserId = 2, ActorRole = RoleConstants.Manager });

        response.Tickets.Should().BeEmpty();
        response.Message.Should().Be("No technical tickets found.");
    }

    [Fact]
    public async Task GetDetailsAsync_ShouldShowConfirmDeliveryButton_WhenCustomerOwnsPendingDeliveryTicket()
    {
        var ticket = BuildTicket();
        ticket.Status = "Pending Delivery Confirmation";
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var response = await service.GetDetailsAsync(ticket.RepairOrderId, new ViewTechnicalTicketsRequest
        {
            ActorUserId = 1,
            ActorRole = RoleConstants.Client,
            CustomerId = ticket.CustomerId
        });

        response.ShowConfirmDeliveryButton.Should().BeTrue();
    }

    [Fact]
    public async Task GetDetailsAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotOwnTicket()
    {
        var ticket = BuildTicket();
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var act = async () => await service.GetDetailsAsync(ticket.RepairOrderId, new ViewTechnicalTicketsRequest
        {
            ActorUserId = 99,
            ActorRole = RoleConstants.Client,
            CustomerId = 99
        });

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Technical ticket not found.");
    }

    [Fact]
    public async Task TrackProgressAsync_ShouldReturnStatusHistory_WhenTechnicianIsAssigned()
    {
        var ticket = BuildTicket();
        ticket.AssignedTechnicianId = 7;
        ticket.StatusHistories.Add(new RepairOrderStatusHistory { Status = "Pending Reception", UpdatedByUserId = 2, UpdatedAtUtc = DateTime.UtcNow });
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var response = await service.TrackProgressAsync(ticket.RepairOrderId, new ViewTechnicalTicketsRequest
        {
            ActorUserId = 7,
            ActorRole = RoleConstants.Technician
        });

        response.History.Should().ContainSingle();
        response.Ticket.AssignedTechnicianId.Should().Be(7);
    }

    [Fact]
    public async Task GetTechniciansAsync_ShouldReturnWorkload_WhenActorIsManager()
    {
        var mocks = CreateMocks();
        mocks.Users.Setup(repo => repo.GetTechniciansAsync()).ReturnsAsync(new[] { BuildTechnician() });
        mocks.RepairOrders.Setup(repo => repo.CountActiveByTechnicianAsync(7)).ReturnsAsync(3);
        var service = CreateService(mocks);

        var response = await service.GetTechniciansAsync(RoleConstants.Manager);

        response.Technicians.Should().ContainSingle(item => item.TechnicianId == 7 && item.ActiveTicketCount == 3);
    }

    [Fact]
    public async Task AssignTechnicianAsync_ShouldUpdateTicketAndRecordHistory_WhenRequestIsValid()
    {
        var ticket = BuildTicket();
        var technician = BuildTechnician();
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        mocks.Users.Setup(repo => repo.GetByIdAsync(technician.UserId)).ReturnsAsync(technician);
        var service = CreateService(mocks);

        var response = await service.AssignTechnicianAsync(new AssignTechnicianRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            TechnicianId = technician.UserId,
            AssignedByUserId = 2,
            ActorRole = RoleConstants.Receptionist
        });

        response.AssignedTechnicianId.Should().Be(technician.UserId);
        mocks.AssignmentHistories.Verify(repo => repo.AddAsync(It.Is<TechnicianAssignmentHistory>(history =>
            history.AssignedTechnicianId == technician.UserId)), Times.Once);
    }

    [Fact]
    public async Task AssignTechnicianAsync_ShouldThrowUnauthorizedException_WhenTechnicianIsLocked()
    {
        var ticket = BuildTicket();
        var technician = BuildTechnician();
        technician.AccountLockedUntilUtc = DateTime.UtcNow.AddMinutes(30);
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        mocks.Users.Setup(repo => repo.GetByIdAsync(technician.UserId)).ReturnsAsync(technician);
        var service = CreateService(mocks);

        var act = async () => await service.AssignTechnicianAsync(new AssignTechnicianRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            TechnicianId = technician.UserId,
            AssignedByUserId = 2,
            ActorRole = RoleConstants.Manager
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Technician cannot be assigned.");
    }

    [Fact]
    public async Task AssignTechnicianAsync_ShouldThrowInvalidOperationException_WhenTicketIsCompleted()
    {
        var ticket = BuildTicket();
        ticket.Status = "Completed";
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var act = async () => await service.AssignTechnicianAsync(new AssignTechnicianRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            TechnicianId = 7,
            AssignedByUserId = 2,
            ActorRole = RoleConstants.Manager
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Technical ticket does not exist or has been completed.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldDeductComponentStockAndSetPendingDeliveryConfirmation_WhenTechnicianCompletesRepair()
    {
        var ticket = BuildTicket();
        ticket.AssignedTechnicianId = 7;
        ticket.Customer = BuildCustomer();
        var component = new Product
        {
            ProductId = 9,
            ProductCode = "P009",
            ProductName = "Battery",
            SalePrice = 20,
            QuantityInStock = 5,
            IsActive = true
        };
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        mocks.Products.Setup(repo => repo.GetActiveProductByIdAsync(component.ProductId)).ReturnsAsync(component);
        var service = CreateService(mocks);

        var response = await service.UpdateAsync(new UpdateTechnicalTicketRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ActorUserId = 7,
            ActorRole = RoleConstants.Technician,
            RepairResult = "Replaced battery",
            WorkPerformed = "Battery replacement",
            ServiceFee = 30,
            StatusDecision = "Complete Repair",
            UsedComponents = new[] { new UsedComponentRequest { ProductId = component.ProductId, Quantity = 2 } }
        });

        response.Status.Should().Be("Pending Delivery Confirmation");
        component.QuantityInStock.Should().Be(3);
        ticket.TotalAmount.Should().Be(70);
        mocks.EmailSender.Verify(sender => sender.SendDeliveryConfirmationOtpAsync(ticket.Customer.Email!, ticket.Customer.FullName, ticket.RepairOrderCode, "123456"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenComponentStockIsInsufficient()
    {
        var ticket = BuildTicket();
        ticket.AssignedTechnicianId = 7;
        var component = new Product { ProductId = 9, ProductCode = "P009", ProductName = "Battery", QuantityInStock = 1, IsActive = true };
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        mocks.Products.Setup(repo => repo.GetActiveProductByIdAsync(component.ProductId)).ReturnsAsync(component);
        var service = CreateService(mocks);

        var act = async () => await service.UpdateAsync(new UpdateTechnicalTicketRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ActorUserId = 7,
            ActorRole = RoleConstants.Technician,
            UsedComponents = new[] { new UsedComponentRequest { ProductId = component.ProductId, Quantity = 2 } }
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Component stock is insufficient.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowUnauthorizedException_WhenTechnicianIsNotAssigned()
    {
        var ticket = BuildTicket();
        ticket.AssignedTechnicianId = 8;
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var act = async () => await service.UpdateAsync(new UpdateTechnicalTicketRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ActorUserId = 7,
            ActorRole = RoleConstants.Technician,
            InspectionResult = "Checked"
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Insufficient access rights.");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldCreatePaymentAndCompleteTicket_WhenTicketIsDelivered()
    {
        var ticket = BuildTicket();
        ticket.Status = "Delivered";
        ticket.TotalAmount = 120;
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var response = await service.ConfirmPaymentAsync(new ConfirmRepairPaymentRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ConfirmedByUserId = 2,
            ActorRole = RoleConstants.Receptionist,
            PaymentMethod = "Cash"
        });

        response.Status.Should().Be("Completed");
        mocks.Payments.Verify(repo => repo.AddAsync(It.Is<Payment>(payment => payment.Amount == 120 && payment.PaymentMethod == "Cash")), Times.Once);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldThrowInvalidOperationException_WhenCustomerHasNotConfirmedDelivery()
    {
        var ticket = BuildTicket();
        ticket.Status = "Pending Delivery Confirmation";
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var act = async () => await service.ConfirmPaymentAsync(new ConfirmRepairPaymentRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ConfirmedByUserId = 2,
            ActorRole = RoleConstants.Manager,
            PaymentMethod = "Bank Transfer"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Customer has not confirmed receipt of delivery, please wait for customer confirmation before payment.");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldThrowInvalidOperationException_WhenAlreadyPaid()
    {
        var ticket = BuildTicket();
        ticket.Status = "Delivered";
        ticket.Payments.Add(new Payment { PaymentStatus = "Completed", PaymentCode = "P001", PaymentMethod = "Cash" });
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var act = async () => await service.ConfirmPaymentAsync(new ConfirmRepairPaymentRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ConfirmedByUserId = 2,
            ActorRole = RoleConstants.Manager,
            PaymentMethod = "Cash"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Technical ticket has already been paid.");
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelTicketAndNotifyCustomer_WhenManagerCancelsAllowedStatus()
    {
        var ticket = BuildTicket();
        ticket.Status = "Under Repair";
        ticket.AssignedTechnicianId = 7;
        ticket.AssignedTechnician = BuildTechnician();
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var response = await service.CancelAsync(new CancelTechnicalTicketRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ActorUserId = 2,
            ActorRole = RoleConstants.Manager,
            Reason = "Duplicate ticket"
        });

        response.Status.Should().Be("Cancelled");
        mocks.StatusHistories.Verify(repo => repo.AddAsync(It.Is<RepairOrderStatusHistory>(history => history.Status == "Cancelled")), Times.Once);
        mocks.EmailSender.Verify(sender => sender.SendTechnicalTicketCancelledAsync(ticket.Customer!.Email!, ticket.Customer.FullName, ticket.RepairOrderCode, "Duplicate ticket"), Times.Once);
        mocks.EmailSender.Verify(sender => sender.SendTechnicianTicketCancelledAsync(ticket.AssignedTechnician!.Email, ticket.AssignedTechnician.FullName, ticket.RepairOrderCode, "Duplicate ticket"), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowUnauthorizedException_WhenReceptionistCancelsAssignedTicket()
    {
        var ticket = BuildTicket();
        ticket.Status = "Under Inspection";
        ticket.AssignedTechnicianId = 7;
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var act = async () => await service.CancelAsync(new CancelTechnicalTicketRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ActorUserId = 2,
            ActorRole = RoleConstants.Receptionist,
            Reason = "Wrong information"
        });

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Receptionist can only cancel unassigned technical tickets.");
    }

    [Fact]
    public async Task CancelAsync_ShouldThrowInvalidOperationException_WhenTicketIsPendingDeliveryConfirmation()
    {
        var ticket = BuildTicket();
        ticket.Status = "Pending Delivery Confirmation";
        var mocks = CreateMocks();
        mocks.RepairOrders.Setup(repo => repo.GetWithDetailsAsync(ticket.RepairOrderId)).ReturnsAsync(ticket);
        var service = CreateService(mocks);

        var act = async () => await service.CancelAsync(new CancelTechnicalTicketRequest
        {
            RepairOrderId = ticket.RepairOrderId,
            ActorUserId = 2,
            ActorRole = RoleConstants.Admin,
            Reason = "Too late"
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Technical ticket cannot be cancelled in its current status.");
    }

    private static TechnicalTicketService CreateService(TestMocks mocks)
    {
        return new TechnicalTicketService(
            mocks.UnitOfWork.Object,
            mocks.EmailSender.Object,
            mocks.OtpGenerator.Object,
            mocks.PasswordHasher.Object,
            new CreateTechnicalTicketRequestValidator(),
            new ViewTechnicalTicketsRequestValidator(),
            new AssignTechnicianRequestValidator(),
            new UpdateTechnicalTicketRequestValidator(),
            new ConfirmRepairPaymentRequestValidator(),
            new CancelTechnicalTicketRequestValidator(),
            Mock.Of<ILogger<TechnicalTicketService>>());
    }

    private static TestMocks CreateMocks()
    {
        var mocks = new TestMocks();
        mocks.UnitOfWork.SetupGet(unit => unit.Customers).Returns(mocks.Customers.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.RepairOrders).Returns(mocks.RepairOrders.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.RepairOrderStatusHistories).Returns(mocks.StatusHistories.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Users).Returns(mocks.Users.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Products).Returns(mocks.Products.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.Payments).Returns(mocks.Payments.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.TechnicianAssignmentHistories).Returns(mocks.AssignmentHistories.Object);
        mocks.UnitOfWork.SetupGet(unit => unit.SystemActivityLogs).Returns(mocks.SystemActivityLogs.Object);
        mocks.UnitOfWork.Setup(unit => unit.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Mock.Of<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>());
        mocks.UnitOfWork.Setup(unit => unit.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mocks.UnitOfWork.Setup(unit => unit.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mocks.UnitOfWork.Setup(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mocks.EmailSender.Setup(sender => sender.SendTechnicalTicketCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mocks.EmailSender.Setup(sender => sender.SendDeliveryConfirmationOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mocks.EmailSender.Setup(sender => sender.SendTechnicalTicketCompletedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mocks.EmailSender.Setup(sender => sender.SendTechnicalTicketCancelledAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mocks.EmailSender.Setup(sender => sender.SendTechnicianTicketCancelledAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        mocks.OtpGenerator.Setup(generator => generator.Generate(It.IsAny<int>())).Returns("123456");
        mocks.PasswordHasher.Setup(hasher => hasher.Hash(It.IsAny<string>())).Returns("hashed-otp");
        return mocks;
    }

    private static CreateTechnicalTicketRequest BuildCreateRequest() => new()
    {
        CustomerId = 1,
        ReceivedByUserId = 2,
        ActorRole = RoleConstants.Receptionist,
        CustomerEmail = "john@example.com",
        CustomerPhone = "0123456789",
        DeviceType = "Laptop",
        Brand = "Dell",
        DeviceModel = "XPS 13",
        RequestType = "Repair",
        IssueDescription = "No power",
        DeviceCondition = "Scratched lid"
    };

    private static Customer BuildCustomer() => new()
    {
        CustomerId = 1,
        CustomerCode = "C001",
        FullName = "John Doe",
        Email = "john@example.com",
        Phone = "0123456789"
    };

    private static RepairOrder BuildTicket() => new()
    {
        RepairOrderId = 5,
        RepairOrderCode = "TT-001",
        CustomerId = 1,
        Customer = BuildCustomer(),
        DeviceType = "Laptop",
        Brand = "Dell",
        RequestType = "Repair",
        IssueDescription = "No power",
        DeviceCondition = "Scratched lid",
        Status = "Pending Reception"
    };

    private static User BuildTechnician() => new()
    {
        UserId = 7,
        Username = "tech",
        PasswordHash = "hash",
        Email = "tech@example.com",
        FullName = "Tech One",
        Specialization = "Laptop",
        WorkStatus = "Working",
        IsActive = true
    };

    private sealed class TestMocks
    {
        public Mock<IUnitOfWork> UnitOfWork { get; } = new();
        public Mock<ICustomerRp> Customers { get; } = new();
        public Mock<IRepairOrderRp> RepairOrders { get; } = new();
        public Mock<IRepairOrderStatusHistoryRp> StatusHistories { get; } = new();
        public Mock<IUserRp> Users { get; } = new();
        public Mock<IProductRp> Products { get; } = new();
        public Mock<IPaymentRp> Payments { get; } = new();
        public Mock<ITechnicianAssignmentHistoryRp> AssignmentHistories { get; } = new();
        public Mock<ISystemActivityLogRp> SystemActivityLogs { get; } = new();
        public Mock<IEmailSender> EmailSender { get; } = new();
        public Mock<IOtpGenerator> OtpGenerator { get; } = new();
        public Mock<IPasswordHasher> PasswordHasher { get; } = new();
    }
}
