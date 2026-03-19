// A faithful C# port of PostgreSQL's quote_identifier() from
// src/backend/utils/adt/ruleutils.c
//
// Portions Copyright (c) 1996-2026, PostgreSQL Global Development Group
// Portions Copyright (c) 1994, Regents of the University of California
//
// PostgreSQL license: https://www.postgresql.org/about/licence/

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace PgQuoteIdent;

/// <summary>
/// Provides PostgreSQL-compatible identifier quoting, faithfully porting the
/// <c>quote_identifier()</c> and <c>quote_qualified_identifier()</c> functions
/// from PostgreSQL's <c>ruleutils.c</c>.
/// </summary>
public static class PgQuoteIdentifier
{
  /// <summary>
  /// When set to <c>true</c>, forces all identifiers to be quoted regardless
  /// of whether they need it. Mirrors PostgreSQL's <c>quote_all_identifiers</c> GUC.
  /// </summary>
  public static bool QuoteAllIdentifiers { get; set; }

  /// <summary>
  /// Quote a PostgreSQL identifier only if needed.
  /// <para>
  /// An identifier is safe (no quoting required) when it starts with a lowercase
  /// letter or underscore, contains only lowercase letters, digits, and underscores,
  /// and is not a reserved/column-name/type-func-name SQL keyword.
  /// </para>
  /// </summary>
  /// <param name="ident">The identifier to potentially quote.</param>
  /// <returns>The identifier, possibly wrapped in double quotes.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="ident"/> is null.</exception>
  public static string QuoteIdentifier(string ident)
  {
    if (ident == null)
    {
      throw new ArgumentNullException(nameof(ident));
    }

    // Can avoid quoting if ident starts with a lowercase letter or underscore
    // and contains only lowercase letters, digits, and underscores, *and* is
    // not any SQL keyword.  Otherwise, supply quotes.
    int nquotes = 0;
    bool safe = ident.Length > 0 &&
                ((ident[0] >= 'a' && ident[0] <= 'z') || ident[0] == '_');

    for (int i = 0; i < ident.Length; i++)
    {
      char ch = ident[i];

      if ((ch >= 'a' && ch <= 'z') ||
          (ch >= '0' && ch <= '9') ||
          ch == '_')
      {
        // okay
      }
      else
      {
        safe = false;
        if (ch == '"')
        {
          nquotes++;
        }
      }
    }

    if (QuoteAllIdentifiers)
    {
      safe = false;
    }

    if (safe)
    {
      // Check for keyword.  We quote keywords except for unreserved ones.
      if (s_nonUnreservedKeywords.Contains(ident))
      {
        safe = false;
      }
    }

    if (safe)
    {
      return ident; // no change needed
    }

    StringBuilder sb = new(ident.Length + nquotes + 2);
    sb.Append('"');
    for (int i = 0; i < ident.Length; i++)
    {
      char ch = ident[i];
      if (ch == '"')
      {
        sb.Append('"'); // double the quote
      }

      sb.Append(ch);
    }

    sb.Append('"');

    return sb.ToString();
  }

  /// <summary>
  /// Quote a possibly-qualified identifier (e.g., <c>schema.table</c>).
  /// Returns <c>qualifier.ident</c> if qualifier is non-null, or just <c>ident</c>
  /// if qualifier is null, quoting each component if necessary.
  /// </summary>
  /// <param name="qualifier">The optional schema/qualifier, or null.</param>
  /// <param name="ident">The identifier name.</param>
  /// <returns>The quoted qualified identifier string.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="ident"/> is null.</exception>
  public static string QuoteQualifiedIdentifier(string? qualifier, string ident)
  {
    if (ident == null)
    {
      throw new ArgumentNullException(nameof(ident));
    }

    if (qualifier != null)
    {
      return QuoteIdentifier(qualifier) + "." + QuoteIdentifier(ident);
    }

    return QuoteIdentifier(ident);
  }

  // Non-unreserved keywords from PostgreSQL's src/include/parser/kwlist.h
  // These are keywords with category: RESERVED_KEYWORD, COL_NAME_KEYWORD,
  // or TYPE_FUNC_NAME_KEYWORD — all of which require quoting when used
  // as identifiers.
  //
  // Generated from PostgreSQL master branch kwlist.h (2026).
  private static readonly HashSet<string> s_nonUnreservedKeywords = new(StringComparer.Ordinal)
  {
    // RESERVED_KEYWORD
    "all",
    "analyse",
    "analyze",
    "and",
    "any",
    "array",
    "as",
    "asc",
    "asymmetric",
    "both",
    "case",
    "cast",
    "check",
    "collate",
    "column",
    "constraint",
    "create",
    "current_catalog",
    "current_date",
    "current_role",
    "current_time",
    "current_timestamp",
    "current_user",
    "default",
    "deferrable",
    "desc",
    "distinct",
    "do",
    "else",
    "end",
    "except",
    "false",
    "fetch",
    "for",
    "foreign",
    "from",
    "grant",
    "group",
    "having",
    "in",
    "initially",
    "intersect",
    "into",
    "lateral",
    "leading",
    "limit",
    "localtime",
    "localtimestamp",
    "not",
    "null",
    "offset",
    "on",
    "only",
    "or",
    "order",
    "placing",
    "primary",
    "references",
    "returning",
    "select",
    "session_user",
    "some",
    "symmetric",
    "system_user",
    "table",
    "then",
    "to",
    "trailing",
    "true",
    "union",
    "unique",
    "user",
    "using",
    "variadic",
    "when",
    "where",
    "window",
    "with",

    // COL_NAME_KEYWORD
    "between",
    "bigint",
    "bit",
    "boolean",
    "char",
    "character",
    "coalesce",
    "dec",
    "decimal",
    "exists",
    "extract",
    "float",
    "graph_table",
    "greatest",
    "grouping",
    "inout",
    "int",
    "integer",
    "interval",
    "json",
    "json_array",
    "json_arrayagg",
    "json_exists",
    "json_object",
    "json_objectagg",
    "json_query",
    "json_scalar",
    "json_serialize",
    "json_table",
    "json_value",
    "least",
    "merge_action",
    "national",
    "nchar",
    "none",
    "normalize",
    "nullif",
    "numeric",
    "out",
    "overlay",
    "position",
    "precision",
    "real",
    "row",
    "setof",
    "smallint",
    "substring",
    "time",
    "timestamp",
    "treat",
    "trim",
    "values",
    "varchar",
    "xmlattributes",
    "xmlconcat",
    "xmlelement",
    "xmlexists",
    "xmlforest",
    "xmlnamespaces",
    "xmlparse",
    "xmlpi",
    "xmlroot",
    "xmlserialize",
    "xmltable",

    // TYPE_FUNC_NAME_KEYWORD
    "authorization",
    "binary",
    "collation",
    "concurrently",
    "cross",
    "current_schema",
    "freeze",
    "full",
    "ilike",
    "inner",
    "is",
    "isnull",
    "join",
    "left",
    "like",
    "natural",
    "notnull",
    "outer",
    "overlaps",
    "right",
    "similar",
    "tablesample",
    "verbose",
  };
}