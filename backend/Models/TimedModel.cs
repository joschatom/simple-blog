namespace backend.Models;

public abstract class TimedModel
{
    /// <summary>
    /// Date and time when the record was created.
    /// </summary>
    /// 
    /// <value>
    /// Auto-generated upon record creation.
    /// </value>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Date and time when the record was last updated.
    /// </summary>
    /// 
    /// <value>
    /// Auto-updated upon record modification.
    /// </value>
    public DateTime? UpdatedAt { get; set; }
}
