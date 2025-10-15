using Superpower.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

internal enum FilterTokens
{
    None,
    String,
    //Number,

    [Token(Example = "'text'")]
    QuotedString,

    [Token(Example = "()")]
    OpenCloseParen,

    [Token(Example = "(")]
    LParen,

    [Token(Example = ")")]
    RParen,

    [Token(Example = ",")]
    Comma,

    [Token(Category = "logical", Example = "|")]
    Or,

    [Token(Category = "logical", Example = "&")]
    And,

    [Token(Category = "operator", Example = "~")]
    Like,

    [Token(Category = "operator", Example = "!~")]
    NotLike,

    [Token(Category = "operator", Example = "=")]
    Equal,

    [Token(Category = "operator", Example = "!=")]
    NotEqual,

    [Token(Category = "operator", Example = "<=")]
    LessThanOrEqual,

    [Token(Category = "operator", Example = ">=")]
    GreaterThanOrEqual,

    [Token(Category = "operator", Example = "<")]
    LessThan,

    [Token(Category = "operator", Example = ">")]
    GreaterThan,

    [Token(Category = "operator", Example = ":=")]
    In,

    [Token(Category = "operator", Example = "!:")]
    NotIn,


    [Token(Category = "operator", Example = "!")]
    Not,

    [Token(Category = "operator", Example = "between")]
    Between,

    [Token(Category = "operator", Example = "isnull")]
    IsNull,

    [Token(Category = "operator", Example = "isnotnull")]
    IsNotNull,

    //[Token(Example = ".")]
    //Dot,

    [Token(Example = " ")]
    Whitespace,
}

