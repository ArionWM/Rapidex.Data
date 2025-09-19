using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class ValidationResultItem : IValidationResultItem
    {
        public bool MarkProblem { get; set; }
        public string ParentName { get; set; }
        public string MemberName { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            if (this.MemberName.IsNullOrEmpty())
                return this.Description;

            return $"{this.MemberName}: {this.Description}";
        }
    }
}
