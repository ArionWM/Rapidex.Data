using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Sample.App1.ConcreteEntities;

public class Contact : DbConcreteEntityBase
{
    public Enumeration<ContactTypeSample> Type { get; set; }

    public string FullName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
    public int Age { get; set; }
}

internal class ContactImplementer : IConcreteEntityImplementer<Contact>
{
    protected static void CalculateContactValues(Contact contact)
    {
        if (contact.BirthDate.IsNOTNullOrEmpty())
        {
            DateTimeOffset now = DateTimeOffset.Now;
            int age = now.Year - contact.BirthDate.Value.Year;
            if (now.DayOfYear < contact.BirthDate.Value.DayOfYear)
                age--;
            contact.Age = age;
        }

        if (contact.FullName.IsNullOrEmpty())
        {
            contact.FullName = (contact.FirstName + " " + contact.LastName).Trim();
        }
    }

    protected static ISignalHandlingResult Validate(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();
        IValidationResult validationResult = new ValidationResult();

        // ....

        if (contact.FirstName.IsNullOrEmpty())
            validationResult.Error("FirstName", "First name is required.");

        return args.CreateHandlingValidationResult(contact, validationResult);
    }

    protected static ISignalHandlingResult ExecLogic(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();

        CalculateContactValues(contact);
        return args.CreateHandlingResult(contact);
    }

    protected static ISignalHandlingResult BeforeSave(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();
        CalculateContactValues(contact);
        return args.CreateHandlingResult();
    }

    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false)
            .MarkOnlyBaseSchema();

        //See: SignalHub.md
        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_BEFORESAVE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.BeforeSave);

        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_VALIDATE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.Validate);

        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_EXEC_LOGIC,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.ExecLogic);
    }
}
