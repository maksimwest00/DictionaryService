using DictionaryService.Domain.Departments;
using DictionaryService.Domain.Positions;

namespace DictionaryService.Domain.DepartmentPositions;

public class DepartmentPosition
{
    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid PositionId { get; private set; }
}