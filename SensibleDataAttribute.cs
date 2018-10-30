using System;

namespace Tlabs.Data {
  /// <summary>
  /// Attribute to decorate class properties that will be pseudonymised on export
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class SensibleDataAttribute : Attribute {
    /// <summary>
    /// Default Ctor
    /// </summary>
    public SensibleDataAttribute() {
    }
  }
}