using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Rapidex
{
    public class Result : IResult
    {
        [JsonPropertyOrder(-9999)]
        public bool Success { get; set; }
        public string Description { get; set; }

        public static Result Ok(string desc = null)
        {
            return new Result() { Success = true, Description = desc };
        }

        public static Result Failure(string message)
        {
            return new Result() { Success = false, Description = message };
        }
    }

    public class Result<T> : Result, IResult<T>
    {
        [JsonPropertyOrder(100)]
        public T Content { get; set; }

        public Result()
        {
        }

        public Result(T content)
        {
            Content = content;
        }

        public Result(bool success, string message)
        {
            base.Success = success;
            Description = message;
        }

        public Result(bool success, string message, T content)
        {
            base.Success = success;
            Description = message;
            Content = content;
        }

        public static Result<T> Ok(T content)
        {
            return new Result<T>(true, string.Empty, content);
        }

        public static new Result<T> Failure(string message)
        {
            return new Result<T>(false, message);
        }
    }
}
