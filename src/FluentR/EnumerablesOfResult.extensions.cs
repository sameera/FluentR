using DotNext;

namespace FluentR;

public static class EnumerablesOfResultExtensions
{
	/// <summary>
	/// Executes the <paramref name="action"/> for of the successful results. 
	/// NOTE: The <paramref name="action"/> is not guarded: implement your own exception handling 
	/// if this action can potentially throw errors.
	/// </summary>
    public static IEnumerable<Result<T>> ThenForSuccesses<T>(
		this IEnumerable<Result<T>> results, 
		Action<T> action)
    {			
		foreach (var result in results)
		{
			if (result.IsSuccessful) action(result.Value);
		}
		return results;
    }

    /// <summary>
    /// Executes the <paramref name="handler"/> for of the failed results. 
    /// NOTE: The <paramref name="handler"/> is not guarded: implement your own exception handling 
    /// if this action can potentially throw errors.
    /// </summary>
    public static IEnumerable<Result<T>> ThenForFailures<T>(
        this IEnumerable<Result<T>> results,
        Action<Exception> handler)
    {
        foreach (var result in results)
        {
            if (!result.IsSuccessful) handler(result.Error);
        }
        return results;
    }

    /// <summary>
    /// Catches all exceptions thrown in the prior step and invokes <paramref name="handler"/>
    /// to handle them.
    /// </summary>
    /// <returns>
    /// A failed result that has a <see cref="AggregateException"/> as <see cref="Result{T}.Error"/>.
    /// If <paramref name="handler"/> throws an exception, a failed result with that exception is returned.
    /// If there were no errors in the previous step, the results are returned as an enumerable.
    /// </returns>
    public static Result<IEnumerable<T>> Catch<T>(
            this IEnumerable<Result<T>> results,
            Action<IEnumerable<Exception>> handler
        )
    {
        var allExceptions = from r in results
                            where !r.IsSuccessful
                            select r.Error;

        if (!allExceptions.Any() ) 
        { 
            return Result.FromValue(results.Select(r => r.Value).AsEnumerable()); 
        }

        try
        {
            handler(allExceptions.AsEnumerable());
            return Result.FromException<IEnumerable<T>>(new AggregateException(allExceptions));
        }
	    catch (Exception ex)
        {
            return Result.FromException<IEnumerable<T>>(ex);
        }
    }

    /// <summary>
    /// Execute the <paramref name="action"/> only if there were no errors during the
    /// previous step.
    /// </summary>
    public static Result<IEnumerable<T>> Then<T>(
            this IEnumerable<Result<T>> results,
            Action<IEnumerable<T>> action
        )
    {
        var allExceptions = from r in results
                            where !r.IsSuccessful
                            select r.Error;

        if (allExceptions.Any())
        {
            return Result.FromException<IEnumerable<T>>(new AggregateException(allExceptions));
        }

        try
        {
            var itr = results.Where(r => r.IsSuccessful).Select(r => r.Value);
            action(itr.AsEnumerable());
            return Result.FromValue(itr.AsEnumerable());
        }
        catch (Exception ex)
        {
            return Result.FromException<IEnumerable<T>>(ex);
        }
    }
}
