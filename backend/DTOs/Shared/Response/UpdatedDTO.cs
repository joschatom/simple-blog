using System.Runtime.InteropServices.JavaScript;

namespace backend.DTOs.Shared.Response;

public class UpdatedDTO
{
    /// <summary>
    /// Indicates whether the update operation actually modified any data.
    /// </summary>
    public required bool Updated { get; set; }

    /// <summary>
    /// The field names that were updated. Or null if unspecified.
    /// </summary>
    public string[]? UpdatedFields { get; set; }
}

public class UpdatedDTOBuilder<T>
{
    private readonly List<string> fieldNames;
    private bool updatedOverride;
    private bool fieldsTracked = false;
    public UpdatedDTOBuilder()
    {
        var totalMembers = typeof(T).GetProperties().Length;

        fieldNames = new List<string>(totalMembers);
        updatedOverride = false;
    }
    public UpdatedDTOBuilder<T> ForceUpdated(bool updated)
    {
        updatedOverride = updated;
        return this;
    }
    public UpdatedDTOBuilder<T> AddFields(params string[] fields)
    {
        fieldNames.AddRange(fields);
        fieldsTracked = true;
        return this;
    }
    public UpdatedDTO Build()
    {
        return new UpdatedDTO() { 
            Updated = updatedOverride || fieldNames.Count > 0,
            UpdatedFields = fieldsTracked ? [.. fieldNames] : null
        };
    }
}