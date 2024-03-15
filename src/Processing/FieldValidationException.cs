using System;

using Tlabs.Data.Entity;

namespace Tlabs.Data.Processing {

  /// <summary>Document validation exception, thrown during validation.</summary>
  public class DocumentValidationException : GeneralException {
    /// <summary>Rule for which validation failed.</summary>
    public DocumentSchema.ValidationRule? Rule { get; }
    /// <summary>Default ctor</summary>
    public DocumentValidationException() : base() { }
    /// <summary>Ctor from message</summary>
    public DocumentValidationException(string message) : base(message) { }
    /// <summary>Ctor from message and inner exception.</summary>
    public DocumentValidationException(string message, Exception e) : base(message, e) { }
    /// <summary>Default ctor</summary>
    public DocumentValidationException(DocumentSchema.ValidationRule rule, Exception e) : base(e.Message, e) {
      this.Rule= rule;
    }
  }

  /// <summary>Field validation exception, thrown if a validation returned false.</summary>
  public class FieldValidationException : DocumentValidationException {
    /// <summary>Ctor from <paramref name="offendingRule"/>.</summary>
    public FieldValidationException(DocumentSchema.ValidationRule offendingRule) : base ($"{offendingRule.Key} - {offendingRule.Description}") { }
  }

}