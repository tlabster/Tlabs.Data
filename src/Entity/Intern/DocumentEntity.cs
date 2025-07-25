using System;
using System.Collections.Generic;

namespace Tlabs.Data.Entity.Intern {
  /// <summary>
  /// Document entity interface
  /// </summary>
  public interface IDocumentEntity {
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// Serialized 
    /// </summary>
    public IDictionary<string, object> Properties { get; set; }
  }

  /// <summary>
  /// Base document entity which stores a series of 
  /// </summary>
  public abstract class DocumentEntity : BaseEntity, IDocumentEntity {
    /// <inheritdoc/>
    public DateTime Created { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
  }

  /// <summary>
  /// Base document entity which stores a series of 
  /// </summary>
  public abstract class EditableDocumentEntity : EditableEntity, IDocumentEntity {
    /// <inheritdoc/>
    public DateTime Created { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
  }
}