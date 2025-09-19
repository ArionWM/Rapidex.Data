using Rapidex.SignalHub;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;

/// <summary>
/// <![CDATA[<databaseOrTenantShortName>/<workspace>/<module>/<signal>/<signal type specific>]]>
/// </summary>
public class SignalTopic : ICloneable, IComparable<SignalTopic>, IEquatable<SignalTopic>
{
    public const string ANY = "+";
    public const string ANY_ALL_SECTIONS = "#";
    public string DatabaseOrTenant { get; set; }
    public string Workspace { get; set; }
    public string Module { get; set; }
    public string Event { get; set; }
    public string Entity { get; set; }
    public string EntityId { get; set; }
    public string Field { get; set; }

    public bool IsSystemLevel { get; set; }

    public bool IsSynchronous
    {
        get
        {
            return this.SignalDefinition?.IsSynchronous ?? false;
        }
    }

    public ISignalDefinition SignalDefinition { get; set; } = null;

    public string[] Sections { get; set; } = new string[0];
    public List<string> OtherSections { get; set; } = new List<string>();

    public int HandlerId { get; set; } = 0;

    public SignalTopic()
    {
        
    }

    public SignalTopic(string databaseOrTenantShortName, string workspace, string module, string @event, string entity = null, string entityId = null, string field = null)
    {
        this.DatabaseOrTenant = databaseOrTenantShortName;
        this.Workspace = workspace;
        this.Module = module;
        this.Event = @event;
        this.Entity = entity;
        this.EntityId = entityId;
        this.Field = field;
        this.Check();
    }

    public object Clone()
    {

        return new SignalTopic
        {
            DatabaseOrTenant = this.DatabaseOrTenant,
            Workspace = this.Workspace,
            Module = this.Module,
            Event = this.Event,
            Entity = this.Entity,
            EntityId = this.EntityId,
            Field = this.Field,
            IsSystemLevel = this.IsSystemLevel,
            SignalDefinition = this.SignalDefinition,
            Sections = (string[])this.Sections.Clone(),
            OtherSections = new List<string>(this.OtherSections)
        };
    }

    public int CompareTo(SignalTopic? other)
    {
        return string.Compare(this.ToString(), other?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(SignalTopic? other)
    {
        if (other == null)
            return false;
        return this.ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    protected void CheckSections()
    {
        if (this.Sections.IsNullOrEmpty())
        {
            List<string> secs = new List<string>
            {
                this.DatabaseOrTenant,
                this.Workspace,
                this.Module,
                this.Event
            };

            if (this.Entity.IsNOTNullOrEmpty())
            {
                secs.Add(this.Entity);
            }

            if (this.EntityId.IsNOTNullOrEmpty())
            {
                secs.Add(this.EntityId);
            }

            if (this.Field.IsNOTNullOrEmpty())
            {
                secs.Add(this.Field);
            }

            this.Sections = secs.ToArray();
        }
    }

    protected void Validate()
    {
        if (this.DatabaseOrTenant.IsNullOrEmpty())
            throw new ArgumentException("Tenant is required for SignalTopic");
        if (this.Workspace.IsNullOrEmpty())
            throw new ArgumentException("Workspace is required for SignalTopic");
        if (this.Module.IsNullOrEmpty())
            throw new ArgumentException("Module is required for SignalTopic");
        if (this.Event.IsNullOrEmpty())
            throw new ArgumentException("Signal is required for SignalTopic");
    }

    public SignalTopic Check()
    {
        this.Validate();
        this.CheckSections();

        return this;
    }

    public override string ToString()
    {
        string topic = $"{this.DatabaseOrTenant}/{this.Workspace}/{this.Module}/{this.Event}";
        if (this.Entity.IsNOTNullOrEmpty())
        {
            topic += $"/{this.Entity}";
        }

        if (this.EntityId.IsNOTNullOrEmpty())
        {
            topic += $"/{this.EntityId}";
        }

        if (this.Field.IsNOTNullOrEmpty())
        {
            topic += $"/{this.Field}";
        }

        if (this.OtherSections.Any())
        {
            topic += $"/{string.Join("/", this.OtherSections)}";
        }

        return topic;
    }

    public static implicit operator string(SignalTopic topic)
    {
        return topic.ToString();
    }

    public static implicit operator SignalTopic(string topicText)
    {
        TopicParser.TopicParseResult result = TopicParser.Parse(Rapidex.Signal.Hub, topicText);
        if (result.Valid)
        {
            return result.Topic;
        }
        else
        {
            throw new ValidationException($"Invalid topic: {topicText}");
        }
    }

    public static SignalTopic Create(string databaseOrTenantShortName, string workspace, string module, string @event, string entity = null, string entityId = null, string field = null)
    {
        return new SignalTopic(databaseOrTenantShortName, workspace, module, @event, entity, entityId, field);
    }
}
