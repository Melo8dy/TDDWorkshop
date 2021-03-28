using System;
namespace BusinessLogic
{
    public class RegisterNewCustomerUseCase
    {
        private ICustomerRepository CustomerRepository { get; }
        private ICustomerNotifier CustomerNotifier { get; }
        public RegisterNewCustomerUseCase(ICustomerRepository customerCustomerRepository,
            ICustomerNotifier customerNotifier)
        {
            CustomerRepository = customerCustomerRepository;
            CustomerNotifier = customerNotifier;
        }
        public Customer Register(CustomerRegistration registration)
        {
            Validate(registration);
            var customer = registration.ToCustomer();
            CustomerRepository.SaveCustomer(customer);
            CustomerNotifier.SendWelcomeMessage(customer);
            return customer;
        }
        private void Validate(CustomerRegistration registration)
        {
            if (registration == null)
                throw new MissingCustomerRegistration();
            registration.Validate();
            var existingCustomer = CustomerRepository.GetCustomer(registration.EmailAddress);
            if (existingCustomer != null)
                throw new DuplicateCustomer();
        }
    }
    public class CustomerRegistration
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string EmailAddress { get; }
        public CustomerRegistration(string firstName, string lastName, string emailAddress)
        {
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
        }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                throw new MissingFirstName();
            if (string.IsNullOrWhiteSpace(LastName))
                throw new MissingLastName();
            if (string.IsNullOrWhiteSpace(EmailAddress))
                throw new MissingEmailAddress();
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(EmailAddress);
            }
            catch
            {
                throw new InvalidEmailAddress();
            }
        }
        public Customer ToCustomer()
        {
            return new(FirstName, LastName, EmailAddress);
        }
    }
    public class Customer
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string EmailAddress { get; }
        public Guid Id { get; }
        public Customer(string firstName, string lastName, string emailAddress)
        {
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            Id = Guid.NewGuid();
        }
    }
    public class MissingCustomerRegistration : Exception
    {
        public MissingCustomerRegistration() : base("Customer registration is missing.")
        {
        }
    }
    public class MissingFirstName : Exception
    {
        public MissingFirstName() : base("Missing first name.")
        {
        }
    }
    public class MissingLastName : Exception
    {
        public MissingLastName() : base("Missing last name.")
        {
        }
    }
    public class MissingEmailAddress : Exception
    {
        public MissingEmailAddress() : base("Missing email address.")
        {
        }
    }
    public class DuplicateCustomer : Exception
    {
    }
    public class InvalidEmailAddress : Exception
    {
    }
    public interface ICustomerRepository
    {
        Customer GetCustomer(string emailAddress);
        void SaveCustomer(Customer customer);
    }
    public interface ICustomerNotifier
    {
        void SendWelcomeMessage(Customer customer);
    }
}