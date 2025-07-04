namespace Govor.Core.Repositories.Groups;

public interface IGroupsRepository : IGroupsReader
{
    // This interface will combine IGroupsReader and IGroupsWriter (once created)
    // For now, it only includes IGroupsReader.
}
