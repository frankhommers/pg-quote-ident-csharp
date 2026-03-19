using System;
using PgQuoteIdent;
using Xunit;

namespace PgQuoteIdent.Tests;

public class QuoteIdentifierTests : IDisposable
{
  public QuoteIdentifierTests()
  {
    // Reset global state before each test
    PgQuoteIdentifier.QuoteAllIdentifiers = false;
  }

  public void Dispose()
  {
    PgQuoteIdentifier.QuoteAllIdentifiers = false;
  }

  // --- Safe identifiers (no quoting needed) ---

  [Theory]
  [InlineData("foo")]
  [InlineData("bar")]
  [InlineData("_private")]
  [InlineData("my_table")]
  [InlineData("a1")]
  [InlineData("column1")]
  [InlineData("test123")]
  [InlineData("_")]
  [InlineData("_123")]
  public void SafeIdentifiers_ReturnedUnquoted(string ident)
  {
    Assert.Equal(ident, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- Identifiers needing quoting due to characters ---

  [Theory]
  [InlineData("Foo", "\"Foo\"")] // uppercase
  [InlineData("FOO", "\"FOO\"")] // all uppercase
  [InlineData("my table", "\"my table\"")] // space
  [InlineData("my-col", "\"my-col\"")] // hyphen
  [InlineData("123abc", "\"123abc\"")] // starts with digit
  [InlineData("", "\"\"")] // empty string
  [InlineData("my.col", "\"my.col\"")] // dot
  [InlineData("col@name", "\"col@name\"")] // special char
  public void UnsafeCharacters_ReturnQuoted(string ident, string expected)
  {
    Assert.Equal(expected, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- Embedded double quotes are doubled ---

  [Theory]
  [InlineData("say\"hello", "\"say\"\"hello\"")]
  [InlineData("a\"b\"c", "\"a\"\"b\"\"c\"")]
  [InlineData("\"", "\"\"\"\"")] // single quote becomes ""
  public void EmbeddedDoubleQuotes_AreDoubled(string ident, string expected)
  {
    Assert.Equal(expected, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- Reserved keywords must be quoted ---

  [Theory]
  [InlineData("select", "\"select\"")]
  [InlineData("table", "\"table\"")]
  [InlineData("where", "\"where\"")]
  [InlineData("from", "\"from\"")]
  [InlineData("and", "\"and\"")]
  [InlineData("or", "\"or\"")]
  [InlineData("not", "\"not\"")]
  [InlineData("null", "\"null\"")]
  [InlineData("true", "\"true\"")]
  [InlineData("false", "\"false\"")]
  [InlineData("primary", "\"primary\"")]
  [InlineData("foreign", "\"foreign\"")]
  [InlineData("unique", "\"unique\"")]
  [InlineData("default", "\"default\"")]
  [InlineData("check", "\"check\"")]
  [InlineData("constraint", "\"constraint\"")]
  public void ReservedKeywords_AreQuoted(string ident, string expected)
  {
    Assert.Equal(expected, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- COL_NAME_KEYWORD must be quoted ---

  [Theory]
  [InlineData("integer", "\"integer\"")]
  [InlineData("varchar", "\"varchar\"")]
  [InlineData("boolean", "\"boolean\"")]
  [InlineData("numeric", "\"numeric\"")]
  [InlineData("smallint", "\"smallint\"")]
  [InlineData("bigint", "\"bigint\"")]
  [InlineData("real", "\"real\"")]
  [InlineData("float", "\"float\"")]
  [InlineData("decimal", "\"decimal\"")]
  [InlineData("timestamp", "\"timestamp\"")]
  [InlineData("interval", "\"interval\"")]
  public void ColNameKeywords_AreQuoted(string ident, string expected)
  {
    Assert.Equal(expected, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- TYPE_FUNC_NAME_KEYWORD must be quoted ---

  [Theory]
  [InlineData("authorization", "\"authorization\"")]
  [InlineData("cross", "\"cross\"")]
  [InlineData("join", "\"join\"")]
  [InlineData("left", "\"left\"")]
  [InlineData("right", "\"right\"")]
  [InlineData("inner", "\"inner\"")]
  [InlineData("outer", "\"outer\"")]
  [InlineData("like", "\"like\"")]
  [InlineData("ilike", "\"ilike\"")]
  [InlineData("similar", "\"similar\"")]
  [InlineData("natural", "\"natural\"")]
  [InlineData("full", "\"full\"")]
  public void TypeFuncNameKeywords_AreQuoted(string ident, string expected)
  {
    Assert.Equal(expected, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- Unreserved keywords should NOT be quoted ---

  [Theory]
  [InlineData("abort")]
  [InlineData("begin")]
  [InlineData("commit")]
  [InlineData("rollback")]
  [InlineData("index")]
  [InlineData("view")]
  [InlineData("trigger")]
  [InlineData("function")]
  [InlineData("sequence")]
  [InlineData("schema")]
  [InlineData("owner")]
  [InlineData("role")]
  [InlineData("replace")]
  [InlineData("cascade")]
  [InlineData("restrict")]
  [InlineData("text")]
  [InlineData("name")]
  public void UnreservedKeywords_NotQuoted(string ident)
  {
    Assert.Equal(ident, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- Keywords are case-sensitive (Postgres lowercases before lookup) ---

  [Theory]
  [InlineData("SELECT", "\"SELECT\"")] // uppercase => needs quoting (has uppercase chars)
  [InlineData("Select", "\"Select\"")] // mixed case => needs quoting
  [InlineData("TABLE", "\"TABLE\"")]
  public void UppercaseKeywords_QuotedDueToCase(string ident, string expected)
  {
    // These get quoted because they contain uppercase, not because of keyword matching.
    // This matches PostgreSQL behavior where identifiers are case-folded before comparison.
    Assert.Equal(expected, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- QuoteAllIdentifiers ---

  [Theory]
  [InlineData("foo", "\"foo\"")]
  [InlineData("my_table", "\"my_table\"")]
  [InlineData("_private", "\"_private\"")]
  public void QuoteAllIdentifiers_ForcesQuoting(string ident, string expected)
  {
    PgQuoteIdentifier.QuoteAllIdentifiers = true;
    Assert.Equal(expected, PgQuoteIdentifier.QuoteIdentifier(ident));
  }

  // --- QuoteQualifiedIdentifier ---

  [Fact]
  public void QualifiedIdentifier_WithSchema()
  {
    Assert.Equal(
      "public.my_table",
      PgQuoteIdentifier.QuoteQualifiedIdentifier("public", "my_table"));
  }

  [Fact]
  public void QualifiedIdentifier_NullQualifier()
  {
    Assert.Equal(
      "my_table",
      PgQuoteIdentifier.QuoteQualifiedIdentifier(null, "my_table"));
  }

  [Fact]
  public void QualifiedIdentifier_BothNeedQuoting()
  {
    Assert.Equal(
      "\"My Schema\".\"My Table\"",
      PgQuoteIdentifier.QuoteQualifiedIdentifier("My Schema", "My Table"));
  }

  [Fact]
  public void QualifiedIdentifier_KeywordSchema()
  {
    Assert.Equal(
      "\"select\".my_col",
      PgQuoteIdentifier.QuoteQualifiedIdentifier("select", "my_col"));
  }

  // --- Null argument ---

  [Fact]
  public void QuoteIdentifier_NullThrows()
  {
    Assert.Throws<ArgumentNullException>(() => PgQuoteIdentifier.QuoteIdentifier(null!));
  }

  [Fact]
  public void QuoteQualifiedIdentifier_NullIdentThrows()
  {
    Assert.Throws<ArgumentNullException>(() => PgQuoteIdentifier.QuoteQualifiedIdentifier("schema", null!));
  }
}