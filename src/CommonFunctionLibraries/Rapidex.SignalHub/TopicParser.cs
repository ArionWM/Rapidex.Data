using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rapidex.SignalHub;
public class TopicParser
{
    public static readonly string Wildcard = "+";
    public static readonly char[] WildcardChars = new[] { '+', '#' };
    public static readonly string[] WildcardStrs = new[] { "+", "#" };

    private static readonly Regex MqttTopicRegex = new Regex(
        @"^(\/?([a-zA-Z0-9_\-]+|(\+)))*(\/(\#))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );



    public struct TopicParseResult
    {
        public bool Valid { get; set; }
        public bool Match { get; set; }
        public SignalTopic Topic { get; set; }

        public string Description { get; set; }

        public static TopicParseResult Invalid(string desc)
        {
            return new TopicParseResult
            {
                Valid = false,
                Match = false,
                Topic = null,
                Description = desc
            };
        }

        public static TopicParseResult Unmatched(string desc)
        {
            return new TopicParseResult
            {
                Valid = true,
                Match = false,
                Topic = null,
                Description = desc
            };
        }
    }

    public static TopicParseResult Parse(ISignalHub hub, string topicText)
    {

        topicText = topicText.TrimEnd('/');
        if (!MqttTopicRegex.IsMatch(topicText))
        {
            return TopicParseResult.Invalid("Topic not match regular structure");
        }

        //<tenantShortName>/<workspace>/<module>/<signal>/<signal type specific>
        List<string> parts = topicText.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (parts.Count < 4)
        {
            return TopicParseResult.Invalid("Topic must have 4 section (min)");
        }

        SignalTopic topic = new SignalTopic();
        topic.Sections = parts.ToArray();

        topic.DatabaseOrTenant = parts[0];
        parts.RemoveAt(0);

        if (TopicParser.WildcardStrs.Contains(topic.DatabaseOrTenant))
        {
            topic.IsSystemLevel = true;
        }

        //Different tenant is system level

        topic.Workspace = parts[0];
        parts.RemoveAt(0);

        topic.Module = parts[0];
        parts.RemoveAt(0);

        topic.Event = parts[0];
        parts.RemoveAt(0);

        //if (TopicParser.WildcardStrs.Contains(topic.Event))
        //{
        //    return TopicParseResult.Invalid($"'Signal' section can't have any wildcard ('{topic.Event}')");
        //}

        ISignalDefinition sdef = hub?.Definitions.Find(topic.Event);
        bool isMatch = sdef != null;

        topic.SignalDefinition = sdef;

        if (sdef != null && sdef.IsEntityReleated)
        {
            if (parts.Count > 0)
            {
                topic.Entity = parts[0];
                parts.RemoveAt(0);
            }

            if (parts.Count > 0)
            {
                topic.EntityId = parts[0];
                parts.RemoveAt(0);
            }

            if (parts.Count > 0)
            {
                topic.Field = parts[0];
                parts.RemoveAt(0);
            }
        }

        if (parts.Count > 0)
        {
            topic.OtherSections.AddRange(parts);
        }

        topic.Check();

        return new TopicParseResult
        {
            Match = isMatch,
            Valid = true,
            Topic = topic
        };

    }
}
