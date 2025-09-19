using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Rapidex
{
    public static class ValidationExtensions
    {
        public static void Error(this IValidationResult result, string message)
        {
            result.Errors.Add(new ValidationResultItem() { Description = message });
            result.Success = false;
        }

        public static void Error(this IValidationResult result, string memberName, string message)
        {
            result.Errors.Add(new ValidationResultItem() { MemberName = memberName, Description = message });
            result.Success = false;
        }

        public static void Error(this IValidationResult result, Exception ex)
        {
            result.Errors.Add(new ValidationResultItem() { Description = ex.Message });
            result.Success = false;
        }

        public static void Warning(this IValidationResult result, string message)
        {
            result.Warnings.Add(new ValidationResultItem() { Description = message });
        }

        public static void Warning(this IValidationResult result, string memberName, string message)
        {
            result.Warnings.Add(new ValidationResultItem() { MemberName = memberName, Description = message });
        }

        public static void Info(this IValidationResult result, string message)
        {
            result.Infos.Add(new ValidationResultItem() { Description = message });
        }

        public static void Info(this IValidationResult result, string memberName, string message)
        {
            result.Infos.Add(new ValidationResultItem() { MemberName = memberName, Description = message });
        }

        public static void Merge(this IValidationResult result, IValidationResult other)
        {
            if (other == null)
                return;

            if (other.Errors != null)
                result.Errors.AddRange(other.Errors);

            if (other.Warnings != null)
                result.Warnings.AddRange(other.Warnings);

            if (other.Infos != null)
                result.Infos.AddRange(other.Infos);

            if (!other.Success)
                result.Success = false;
        }

        public static string CreateErrorDescription(this IValidationResult res, bool onlyErrors = false)
        {
            StringBuilder builder = new StringBuilder();

            if (res.Description.IsNOTNullOrEmpty())
            {
                builder.AppendLine(res.Description);
                builder.AppendLine();
            }

            foreach (object obj in res.Errors)
            {
                builder.AppendLine("ERROR: " + obj.ToString());
            }

            if (!onlyErrors)
            {
                foreach (object obj in res.Warnings)
                {
                    builder.AppendLine("WARNING: " + obj.ToString());
                }

                foreach (object obj in res.Infos)
                {
                    builder.AppendLine("INFO: " + obj.ToString());
                }
            }
            return builder.ToString();
        }


    }
}
