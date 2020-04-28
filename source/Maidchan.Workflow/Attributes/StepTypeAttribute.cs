using System;

namespace Maidchan.Workflow.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StepTypeAttribute : System.Attribute
    {
        public string Description { get; set; }

        public StepTypeAttribute(string descriiption) => this.Description = descriiption;

        public StepTypeAttribute() { }
    }

    public enum DataKind
    {
        Number, Boolean, Text
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class InputAttribute : System.Attribute
    {
        public string HelpText { get; set; }
        public DataKind Kind { get; set; } = DataKind.Text;

        public InputAttribute() { }
        public InputAttribute(DataKind kind, string helpText = default(string))
        {
            this.Kind = kind;
            this.HelpText = helpText;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OutputAttribute : System.Attribute
    {
    }
}