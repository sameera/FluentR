using DotNext;
using FluentAssertions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FluentR.Tests;

public class EnumerableOfResults
{
    [Fact]
    public void Route_all_successes_through_a_handler()
    {
        int callCounter = 0;
        var values = new int[5];

        GetTheEnumerables(5, 2).ThenForSuccesses(i => {
            values[callCounter++] = i;
        });
        callCounter.Should().Be(5);
        values.Should().BeEquivalentTo(new[] { 0, 1, 2, 3, 4 });
    }

    [Fact]
    public void Route_all_failures_through_a_handler()
    {
        int callCounter = 0;
        var values = new string[2];

        GetTheEnumerables(5, 2).ThenForFailures(e => {
            values[callCounter++] = e.Message;
        });
        callCounter.Should().Be(2);
        values.Should().BeEquivalentTo(new[] { "Error 0", "Error 1" });
    }

    [Fact]
    public void Both_success_and_failures_can_be_handled_separately()
    {
        int successCounter = 0, failuresCounter = 0;
        GetTheEnumerables(5, 4)
            .ThenForSuccesses(_ => successCounter++)
            .ThenForFailures(_ => failuresCounter++);

        successCounter.Should().Be(5);
        failuresCounter.Should().Be(4);
    }

    [Fact]
    public void Catches_if_any_error_occured()
    {
        IEnumerable<Exception>? errors = null;

        var finalResult = GetTheEnumerables(5, 2)
            .Catch(caughtErrors => errors = caughtErrors);

        finalResult.IsSuccessful.Should().BeFalse();
        finalResult.Error.Should().NotBeNull();
        finalResult.Error.Should().BeOfType<AggregateException>();
        errors.Should().NotBeNull();
        errors!.Count().Should().Be(2);
    }

    [Fact]
    public void Any_exception_in_Catch_block_is_caught()
    {
        var finalResult = GetTheEnumerables(5, 2)
                            .Catch(_ => throw new Exception("Error X"));

        finalResult.IsSuccessful.Should().BeFalse();
        finalResult.Error.Should().NotBeNull();
        finalResult.Error!.Message.Should().Be("Error X");
    }

    [Fact]
    public void Then_block_will_execute_if_there_were_erors_in_prior_setp()
    {
        bool thenHandlerWasInvoked = false;

        var finalResult = GetTheEnumerables(5, 2)
                    .Then(_ => thenHandlerWasInvoked = true);

        thenHandlerWasInvoked.Should().BeFalse();

        finalResult.IsSuccessful.Should().BeFalse();
        finalResult.Error.Should().NotBeNull();
        finalResult.Error.Should().BeOfType<AggregateException>();
    }

    private IEnumerable<Result<int>> GetTheEnumerables(int successCount, int failCount = 0)
    {
        for (int i = 0; i < successCount; i++)
        {
            yield return i;
        }

        for (int i = 0; i < failCount; i++)
        {
            yield return Result.FromException<int>(new Exception($"Error {i}"));
        }
    }

    public EnumerableOfResults()
    {
        
    }
}