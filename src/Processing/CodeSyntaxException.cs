using System;
using System.Collections.Generic;

namespace Tlabs.Data.Processing {

  /// <summary>Code syntax exception.</summary>
  public class CodeSyntaxException : AppConfigException {
    /// <summary>Syntax errors.</summary>
    public readonly IList<CodeSyntaxException> SyntaxErrors;

    /// <summary>Default ctor</summary>
    public CodeSyntaxException() : base() { }

    /// <summary>Ctor from message</summary>
    public CodeSyntaxException(string message) : base(message) { }

    /// <summary>Ctor from message and inner exception.</summary>
    public CodeSyntaxException(string message, Exception e) : base(message, e) { }

    /// <summary>Ctor from message and inner exception.</summary>
    public CodeSyntaxException(IList<CodeSyntaxException> syntaxErrors) : base($"Compilation failed after {syntaxErrors.Count} formula code syntax error(s) detected.") {
      this.SyntaxErrors= syntaxErrors;
    }
  }

}
