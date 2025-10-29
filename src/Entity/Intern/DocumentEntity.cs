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
  public abstract class DocumentEntity : BaseEntity, IDocumentEntity
  {
    /// <inheritdoc/>
    public DateTime Created { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    static DocumentEntity()
    {
      DataStoreEvent<DocumentEntity>.Inserting += setCreated;
    }

    private static void setCreated(Event.IEvent<DocumentEntity> ev)
    {
      DocumentEntity ent = ev.Entity;
      ent.Created = App.TimeInfo.Now;
    }
  }

  /// <summary>
  /// Base document entity which stores a series of 
  /// </summary>
  public abstract class EditableDocumentEntity : EditableEntity, IDocumentEntity
  {
    /// <inheritdoc/>
    public DateTime Created { get; set; }

    /// <inheritdoc/>
    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    static EditableDocumentEntity()
    {
      DataStoreEvent<EditableDocumentEntity>.Inserting += setCreated;
    }

    private static void setCreated(Event.IEvent<EditableDocumentEntity> ev)
    {
      EditableDocumentEntity ent = ev.Entity;
      ent.Created = App.TimeInfo.Now;
    }
  }
}