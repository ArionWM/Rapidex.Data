using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public class ValidationResult : IValidationResult //From ProCore
    {
        public static ValidationResult Ok { get { return new ValidationResult(); } }

        public List<IValidationResultItem> Errors { get; protected set; } = new List<IValidationResultItem>();

        public List<IValidationResultItem> Warnings { get; protected set; } = new List<IValidationResultItem>();

        public List<IValidationResultItem> Infos { get; protected set; } = new List<IValidationResultItem>();

        public bool Success { get { return !this.Errors.Any(); } set { } }
        public string Description { get; set; }

        public ValidationResult()
        {
            
        }

        public ValidationResult(IValidationResult vr)
        {
            if (vr != null)
            {
                this.Description = vr.Description;
                this.Errors.AddRange(vr.Errors);
                this.Warnings.AddRange(vr.Warnings);
                this.Infos.AddRange(vr.Infos);
            }

        }


        public void MergeErrors(IEnumerable<IValidationResultItem> errors)
        {
            this.Errors.AddRange(errors);
        }

        public void MergeWarnings(IEnumerable<IValidationResultItem> warnings)
        {
            this.Warnings.AddRange(warnings);
        }

        public void MergeInfos(IEnumerable<IValidationResultItem> infos)
        {
            this.Infos.AddRange(infos);
        }

    }
}
