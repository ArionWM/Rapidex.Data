using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Helpers;
public static class ComponentExtensions
{
    public static void SetStatus<T>(this T component, RunnableComponentStatus status, string message = "") where T : IOneTimeRunnableComponent
    {
        component.Status = status;
        component.StatusMessage = message;
    }

    public static void SetMessage<T>(this T component, string message) where T : IOneTimeRunnableComponent
    {
        component.StatusMessage = message;
    }

    public static void SetProgress<T>(this T component, double progressPercent, string message = "") where T : IOneTimeRunnableComponent
    {
        component.Status = RunnableComponentStatus.Running;
        if (message.IsNOTNullOrEmpty())
            component.StatusMessage = $"{progressPercent}% {message}";
        else
            component.StatusMessage = $"{progressPercent}% completed.";
        component.ProgressPercentage = progressPercent;
    }

    public static void SetRunning<T>(this T component, string message = "") where T : IOneTimeRunnableComponent
    {
        component.Status = RunnableComponentStatus.Running;
        component.StatusMessage = message;
        component.ProgressPercentage = 0;
    }

    public static void SetCompleted<T>(this T component, string message = "") where T : IOneTimeRunnableComponent
    {
        component.Status = RunnableComponentStatus.Completed;
        component.StatusMessage = message;
        component.ProgressPercentage = 100;
    }

    public static void SetFailure<T>(this T component, string message = "") where T : IOneTimeRunnableComponent
    {
        component.Status = RunnableComponentStatus.Failure;
        component.StatusMessage = message;
    }

    public static void SetFailure<T>(this T component, Exception ex, string message = "") where T : IOneTimeRunnableComponent
    {
        ex.Log();
        component.Status = RunnableComponentStatus.Failure;
        component.StatusMessage = ex.Message + "\r\n" + message;
    }




}
