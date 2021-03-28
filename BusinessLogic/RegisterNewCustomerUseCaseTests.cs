using FluentAssertions;
using System;
using Xunit;
namespace BusinessLogic
{
    public class RegisterNewCustomerUseCaseTests
    {
        private static readonly CustomerRegistration FredFlintstoneRego = new("Fred", "Flintstone", "fred.flintstone@gmail.com");
        private static readonly Customer FredFlintstone = new("Fred", "Flintstone", "fred.flintstone@gmail.com");
        private RegisterNewCustomerUseCase _useCase;
        private MockCustomerRepository _mockCustomerRepository;
        private readonly MockCustomerNotifier _mockCustomerNotifier;
        public RegisterNewCustomerUseCaseTests()
        {
            _mockCustomerRepository = new MockCustomerRepository();
            _mockCustomerNotifier = new MockCustomerNotifier();
            _useCase = new RegisterNewCustomerUseCase(_mockCustomerRepository, _mockCustomerNotifier);
        }
        [Fact]
        public void
            Given_CustomerRegistration_Is_Null_When_Call_Register_Then_Throw_MissingCustomerRegistration_Exception()
        {
            Action register = () => _useCase.Register(null);
            register.Should().ThrowExactly<MissingCustomerRegistration>()
                .WithMessage("Customer registration is missing.");
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData(" \r\n \t  ")]
        public void Given_Missing_FirstName_When_Call_Register_Then_Throw_MissingFirstName_Exception(string firstName)
        {
            Action register = () =>
                _useCase.Register(new CustomerRegistration(firstName, "Flintstone", "fred.flintstone@gmail.com"));
            register.Should().ThrowExactly<MissingFirstName>().WithMessage("Missing first name.");
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData(" \r\n \t  ")]
        public void Given_Missing_LastName_When_Call_Register_Then_Throw_MissingLastName_Exception(string lastName)
        {
            Action register = () =>
                _useCase.Register(new CustomerRegistration("Fred", lastName, "fred.flintstone@gmail.com"));
            register.Should().ThrowExactly<MissingLastName>().WithMessage("Missing last name.");
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData(" \r\n \t  ")]
        public void Given_Missing_EmailAddress_When_Call_Register_Then_Throw_MissingEmailAddress_Exception(
            string emailAddress)
        {
            Action register = () => _useCase.Register(new CustomerRegistration("Fred", "Flintstone", emailAddress));
            register.Should().ThrowExactly<MissingEmailAddress>().WithMessage("Missing email address.");
        }
        [Theory]
        [InlineData("fred.gmail")]
        [InlineData("fredgmail")]
        [InlineData("@fred.gmail")]
        public void Given_EmailAddress_Invalid_When_Call_Register_Then_Throw_InvalidEmailAddress_Exception(string emailAddress)
        {
            Action register = () =>
                _useCase.Register(new CustomerRegistration("Fred", "Flintstone", emailAddress));
            register.Should().ThrowExactly<InvalidEmailAddress>();
        }
        [Theory]
        [InlineData("fred.flintstone@gmail.com")]
        [InlineData("fred@flintstones.com")]
        [InlineData("fred.f@hannabarbera.net")]
        public void When_Call_Register_Then_Call_GetCustomer_By_EmailAddress_On_CustomerRepository(string emailAddress)
        {
            _useCase.Register(new CustomerRegistration("Fred", "Flintstone", emailAddress));
            _mockCustomerRepository.WasGetCustomerByEmailAddressCalled.Should().BeTrue();
            _mockCustomerRepository.PassedInEmailAddress.Should().Be(emailAddress);
        }
        [Fact]
        public void Given_Existing_Customer_When_Call_Register_Then_Throw_DuplicateCustomer_Exception()
        {
            _mockCustomerRepository = new MockCustomerRepository(FredFlintstone);
            _useCase = new RegisterNewCustomerUseCase(_mockCustomerRepository, _mockCustomerNotifier);
            Action register = () => _useCase.Register(FredFlintstoneRego);
            register.Should().ThrowExactly<DuplicateCustomer>();
        }
        [Theory]
        [InlineData("Fred", "Flintstone", "fred.flintstone@gmail.com")]
        [InlineData("Fred", "Flintstone", "fred.f@hannabarbera.net")]
        public void Given_RegisterCustomer_Valid_When_Call_Register_Then_ConvertRegisterCustomer_ToCustomer(
            string firstName,
            string lastName, string emailAddress)
        {
            var customerRegistration = new CustomerRegistration(firstName, lastName, emailAddress);
            var result = _useCase.Register(customerRegistration);
            result.EmailAddress.Should().Be(customerRegistration.EmailAddress);
            result.FirstName.Should().Be(customerRegistration.FirstName);
            result.LastName.Should().Be(customerRegistration.LastName);
            result.Id.Should().NotBeEmpty();
        }
        [Fact]
        public void When_Call_Register_Then_Call_SaveCustomer_On_CustomerRepository()
        {
            _useCase.Register(new CustomerRegistration("Fred", "Flintstone", "fred.flintstone@gmail.com"));
            _mockCustomerRepository.WasSaveCustomerCalled.Should().BeTrue();
            _mockCustomerRepository.SavedCustomerId.Should().NotBeEmpty();
        }
        [Fact]
        public void When_Call_Register_ThenCallNotifyCustomer_OnCustomerNotify()
        {
            _useCase.Register(new CustomerRegistration("Fred", "Flintstone", "fred.flintstone@gmail.com"));
            _mockCustomerNotifier.SendWelcomeMessageCalled.Should().BeTrue();
        }
    }
    public class MockCustomerNotifier : ICustomerNotifier
    {
        public bool SendWelcomeMessageCalled;
        public void SendWelcomeMessage(Customer customer)
        {
            SendWelcomeMessageCalled = true;
        }
    }
    public class MockCustomerRepository : ICustomerRepository
    {
        public bool WasGetCustomerByEmailAddressCalled;
        public string PassedInEmailAddress;
        public bool WasSaveCustomerCalled;
        public Guid SavedCustomerId;
        private readonly Customer _customerToReturn;
        public MockCustomerRepository(Customer customer = null)
        {
            _customerToReturn = customer;
        }
        public Customer GetCustomer(string emailAddress)
        {
            WasGetCustomerByEmailAddressCalled = true;
            PassedInEmailAddress = emailAddress;
            return _customerToReturn;
        }
        public void SaveCustomer(Customer customer)
        {
            WasSaveCustomerCalled = true;
            SavedCustomerId = customer.Id;
        }
    }
}