# PgQuoteIdent

A faithful C# port of PostgreSQL's `quote_identifier()` function from [`ruleutils.c`](https://github.com/postgres/postgres/blob/master/src/backend/utils/adt/ruleutils.c).

Quotes SQL identifiers only when needed, matching PostgreSQL's exact behavior including keyword detection. Distributed as a **source-only** NuGet package (no runtime dependency).

## Installation

```
dotnet add package PgQuoteIdent
```

## Usage

```csharp
using PgQuoteIdent;

// Safe identifiers are returned as-is
PgQuoteIdentifier.QuoteIdentifier("my_table");     // "my_table"
PgQuoteIdentifier.QuoteIdentifier("foo");           // "foo"

// Reserved keywords are quoted
PgQuoteIdentifier.QuoteIdentifier("select");        // "\"select\""
PgQuoteIdentifier.QuoteIdentifier("table");         // "\"table\""

// Unsafe characters trigger quoting
PgQuoteIdentifier.QuoteIdentifier("My Table");      // "\"My Table\""
PgQuoteIdentifier.QuoteIdentifier("123abc");        // "\"123abc\""

// Embedded double quotes are escaped
PgQuoteIdentifier.QuoteIdentifier("say\"hi");       // "\"say\"\"hi\""

// Qualified identifiers (schema.table)
PgQuoteIdentifier.QuoteQualifiedIdentifier("public", "my_table");
// "public.my_table"

PgQuoteIdentifier.QuoteQualifiedIdentifier("select", "My Col");
// "\"select\".\"My Col\""

// Force-quote everything (like PostgreSQL's quote_all_identifiers GUC)
PgQuoteIdentifier.QuoteAllIdentifiers = true;
PgQuoteIdentifier.QuoteIdentifier("foo");           // "\"foo\""
```

## How it works

The quoting logic follows PostgreSQL's rules exactly:

1. An identifier is **safe** (no quoting) if it starts with `a-z` or `_`, contains only `a-z`, `0-9`, `_`, and is not a non-unreserved SQL keyword.
2. Non-unreserved keywords (`RESERVED_KEYWORD`, `COL_NAME_KEYWORD`, `TYPE_FUNC_NAME_KEYWORD`) always require quoting.
3. Unreserved keywords (like `begin`, `commit`, `text`) do **not** require quoting.
4. When quoting, embedded `"` characters are doubled (`""`) per SQL standard.

## Source-only package

This package contains no compiled assembly. The source file is compiled directly into your project, so there is zero runtime dependency. This makes it suitable for libraries that want to avoid transitive dependency chains.

## Versioning

This package follows **PostgreSQL-aligned versioning**:

- **Major version** = PostgreSQL major version the keyword list is derived from
- **Minor/Patch** = own changes (bug fixes, improvements)

For example:
- `19.0.0` — initial release based on PostgreSQL 19 (master) keyword list
- `19.0.1` — bug fix in C# code, same keyword list
- `19.1.0` — new feature, same keyword list
- `20.0.0` — updated keyword list for PostgreSQL 20

## License

This project is licensed under the [MIT License](LICENSE).

The identifier quoting logic is ported from PostgreSQL, which is licensed under the [PostgreSQL License](https://www.postgresql.org/about/licence/).
