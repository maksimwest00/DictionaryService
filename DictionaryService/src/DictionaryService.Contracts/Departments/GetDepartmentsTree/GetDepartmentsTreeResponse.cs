namespace DictionaryService.Contracts.Departments.GetDepartmentsTree;

public record GetDepartmentsTreeResponse(
    Guid Id,        // id uuid
    string Name,    // name varchar(150)
    string Slug,    // identifier varchar(150)
    string Path,    // path ltree
    int Depth,      // depth int
    Guid? ParentId, // parent_id uuid
    bool HasChildren);