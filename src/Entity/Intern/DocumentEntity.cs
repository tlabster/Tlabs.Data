using System;
using System.Collections.Generic;
using Tlabs.Data.Event;

namespace Tlabs.Data.Entity.Intern
{
  /// <summary>
  /// Document entity interface
  /// </summary>
  public interface IDocumentEntity
  {
    /// <summary>
    /// Serialized 
    /// </summary>
    public IDictionary<string, object> Properties { get; set; }
  }

  /// <summary>
  /// Base document entity which stores a series of 
  /// </summary>
  public abstract class DocumentEntity : EditableEntity, IDocumentEntity
  {
    /// <inheritdoc/>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
  }

  /// <summary>
  /// Base document entity which stores a series of 
  /// </summary>
  public abstract class EditableDocumentEntity : EditableEntity, IDocumentEntity
  {
    /// <inheritdoc/>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
  }
}