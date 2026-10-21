using CSharpFunctionalExtensions;
using Shared.Kernel;

namespace Shared.Core.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();

    UnitResult<Error> Rollback();
}