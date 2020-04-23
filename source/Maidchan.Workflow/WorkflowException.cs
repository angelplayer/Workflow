using System;

namespace Maidchan.Workflow
{
    public class WorkflowException : Exception
    {
        public const byte Warning = 50;

        public const byte Unknown = 255;

        public readonly int Code;

        public WorkflowException(int code, string message) : base(message)
        {
            this.Code = code;
        }
    }
}