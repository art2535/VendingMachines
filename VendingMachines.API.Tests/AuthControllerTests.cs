using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VendingMachines.API.Controllers;
using VendingMachines.API.DTOs.Auth;
using VendingMachines.API.Tests.Context;
using VendingMachines.API.Tests.Helpers;
using VendingMachines.API.Tests.TestsData;
using VendingMachines.Infrastructure.Data;

namespace VendingMachines.API.Tests;

public class AuthControllerTests : IDisposable
{
    private readonly VendingMachinesContext _context;
    private readonly IConfiguration _configuration;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<VendingMachinesContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _context = new TestVendingMachinesContext(options);
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Jwt:Key", "your_secret_key_32_chars_long_at_least"),
                new KeyValuePair<string, string>("Jwt:Issuer", "https://0.0.0.0:7270;http://0.0.0.0:5321"),
                new KeyValuePair<string, string>("Jwt:Audience", "*")
            }!)
            .Build();

        _configuration = config;
        
        _controller = new AuthController(_context, _configuration)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Theory]
    [MemberData(nameof(AuthControllerTestsData.GetInvalidRegisterRequests),
        MemberType = typeof(AuthControllerTestsData))]
    public async Task RegisterAsync_DataNotValid_ReturnsBadRequest(RegisterRequest request,
        string errorMessage)
    {
        _controller.ValidateModel(request);
        
        var result = await _controller.RegisterAsync(request);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be(errorMessage);
    }

    [Theory]
    [MemberData(nameof(AuthControllerTestsData.GetValidRegisterRequests), 
        MemberType = typeof(AuthControllerTestsData))]
    public async Task RegisterAsync_DataValid_ReturnsOk(RegisterRequest request)
    {
        _controller.ValidateModel(request);
        
        var result = await _controller.RegisterAsync(request);

        result.Should().BeOfType<OkObjectResult>();

        var createdUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        createdUser.Should().NotBeNull("потому что пользователь должен быть создан");

        createdUser.Email.Should().Be(request.Email);
        createdUser.LastName.Should().Be(request.LastName);
        createdUser.FirstName.Should().Be(request.FirstName);
        createdUser.MiddleName.Should().Be(request.MiddleName);
        createdUser.Phone.Should().Be(request.Phone);
        createdUser.RoleId.Should().Be(request.RoleId);
        createdUser.CompanyId.Should().Be(request.CompanyId);
        createdUser.Language.Should().Be(request.Language);
        createdUser.HashedPassword.Should().Be(request.Password);
    }
}