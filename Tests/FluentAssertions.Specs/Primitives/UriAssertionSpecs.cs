using System;
using Xunit;
using Xunit.Sdk;

namespace FluentAssertions.Specs.Primitives;

public class UriAssertionSpecs
{
    private static readonly Uri Absolute = new("https://api.example.com:8443/orders/42?page=2&q=a%20b&flag#top");

    [Fact]
    public void Chained_assertions_on_an_absolute_uri_succeed()
    {
        Absolute.Should()
            .BeAbsolute()
            .And.HaveScheme("HTTPS")
            .And.HaveHost("API.example.com")
            .And.HavePort(8443)
            .And.HavePath("/orders/42")
            .And.HaveFragment("top")
            .And.HaveQueryParameter("page", "2")
            .And.HaveQueryParameter("q", "a b")
            .And.NotHaveQueryParameter("other")
            .And.NotHaveScheme("http")
            .And.NotHaveHost("example.com")
            .And.NotHavePort(80)
            .And.NotHavePath("/orders")
            .And.NotHaveFragment("bottom");
    }

    [Fact]
    public void Be_compares_the_original_string()
    {
        new Uri("https://example.com/a").Should().Be("https://example.com/a");
        new Uri("https://example.com/a").Should().NotBe("https://example.com/b");

        Action act = () => new Uri("https://example.com/a").Should().Be("https://example.com/b", "of {0}", "reasons");

        act.Should().Throw<XunitException>().WithMessage(
            "Expected URI to be \"https://example.com/b\" because of reasons, but found https://example.com/a.");
    }

    [Fact]
    public void Be_relative_and_absolute_distinguish_the_kinds()
    {
        new Uri("/a/b", UriKind.Relative).Should().BeRelative();
        Absolute.Should().NotBeNull();

        Action act = () => new Uri("/a/b", UriKind.Relative).Should().BeAbsolute();

        act.Should().Throw<XunitException>().WithMessage("Expected URI to be absolute, but found /a/b.");
        ((Action)(() => Absolute.Should().BeRelative())).Should().Throw<XunitException>();
    }

    [Fact]
    public void A_failing_scheme_assertion_shows_the_uri()
    {
        Action act = () => new Uri("http://example.com/").Should().HaveScheme("https");

        act.Should().Throw<XunitException>().WithMessage(
            "Expected URI http://example.com/ to have scheme \"https\", but found \"http\".");
    }

    [Fact]
    public void A_failing_host_port_and_path_assertion_throws()
    {
        ((Action)(() => Absolute.Should().HaveHost("other.com"))).Should().Throw<XunitException>();
        ((Action)(() => Absolute.Should().HavePort(443))).Should().Throw<XunitException>();
        ((Action)(() => Absolute.Should().HavePath("/orders"))).Should().Throw<XunitException>();
        ((Action)(() => Absolute.Should().HaveFragment("x"))).Should().Throw<XunitException>();
    }

    [Fact]
    public void Query_parameter_assertions_report_missing_and_mismatching_values()
    {
        ((Action)(() => Absolute.Should().HaveQueryParameter("missing")))
            .Should().Throw<XunitException>().WithMessage("*query parameter \"missing\", but it was not found.");

        ((Action)(() => Absolute.Should().HaveQueryParameter("page", "3")))
            .Should().Throw<XunitException>().WithMessage("*with value \"3\", but found {\"2\"}.");

        ((Action)(() => Absolute.Should().HaveQueryParameter("missing", "3")))
            .Should().Throw<XunitException>().WithMessage("*the parameter was not found.");

        ((Action)(() => Absolute.Should().NotHaveQueryParameter("page")))
            .Should().Throw<XunitException>();
    }

    [Fact]
    public void Have_query_parameter_exposes_the_value_through_which()
    {
        Absolute.Should().HaveQueryParameter("page").Which.Should().Be("2");
        Absolute.Should().HaveQueryParameter("flag").Which.Should().BeEmpty();
    }

    [Fact]
    public void Be_base_of_checks_the_prefix()
    {
        new Uri("https://example.com/api/").Should().BeBaseOf(new Uri("https://example.com/api/orders"));
        new Uri("https://example.com/api/").Should().NotBeBaseOf(new Uri("https://other.com/api/orders"));

        ((Action)(() => new Uri("https://example.com/api/").Should().BeBaseOf(new Uri("https://other.com/"))))
            .Should().Throw<XunitException>();

        ((Action)(() => Absolute.Should().BeBaseOf(null))).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Absolute_only_assertions_on_a_relative_uri_fail_with_a_clear_message()
    {
        Action act = () => new Uri("/a", UriKind.Relative).Should().HaveHost("example.com");

        act.Should().Throw<XunitException>().WithMessage("Expected URI to be an absolute URI with a host, but found /a.");
    }

    [Fact]
    public void Null_uri_fails_instead_of_throwing()
    {
        Uri subject = null;

        ((Action)(() => subject.Should().HaveScheme("https"))).Should().Throw<XunitException>();
        ((Action)(() => subject.Should().BeAbsolute())).Should().Throw<XunitException>();
    }
}
