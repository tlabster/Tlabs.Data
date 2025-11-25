using System.ComponentModel.DataAnnotations.Schema;

namespace Tlabs.Data.Model {
  /// <summary>
  /// Model 
  /// </summary>
  public class RelationalTableInfo {
    /// <summary>
    /// Ctor. from <paramref name="tableName"/> and optional <paramref name="schema"/>
    /// </summary>
    public RelationalTableInfo(string? tableName, string? schema = null) {
      TableName = tableName;
      Schema = schema;
    }

    /// <summary>
    /// Table name used in the Storage
    /// </summary>
    /// <remarks>Will be <see langword="null"/>if entity is not mapped to a table</remarks>
    public string? TableName { get; set; }

    /// <summary>
    /// Schema used 
    /// </summary>
    /// <remarks>Will be <see langword="null"/>if entity is not stored under a schema</remarks>
    public string? Schema { get; set; }
  }
}