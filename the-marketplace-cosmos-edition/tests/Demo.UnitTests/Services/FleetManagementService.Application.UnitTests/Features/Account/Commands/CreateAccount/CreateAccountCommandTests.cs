using FleetManagementService.Application.Features.Account.Commands.CreateAccount;
using FluentAssertions;

namespace FleetManagementService.Application.UnitTests.Features.Account.Commands.CreateAccount;

public class CreateAccountCommandTests
{
    [Fact]
    public void CreateAccountCommand_ShouldInitializeCorrectly()
    {
        // Arrange
        var companyName = "Test Company";
        var vatNumber = "VAT123";
        var billingAddressId = Guid.NewGuid();

        // Act
        var command = new CreateAccountCommand(companyName, vatNumber, billingAddressId);

        // Assert
        command.CompanyName.Should().Be(companyName);
        command.CompanyVatNumber.Should().Be(vatNumber);
        command.BillingAddressId.Should().Be(billingAddressId);
    }
}