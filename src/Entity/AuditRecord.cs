using System.Net;
using Tlabs.Data.Entity.Intern;

namespace Tlabs.Data.Entity {
  ///<summary>Defines a record that contains information about an entity to be imported into the system usin an specific loyalty operation.</summary>
  public class AuditRecord : EditableEntity {
    ///<summary>Action method</summary>
    public string? Method { get; set; }
    ///<summary>Name of the action being executed</summary>
    public string? ActionName { get; set; }
    ///<summary>Full url</summary>
    public string? URL { get; set; }
    ///<summary>Payload of the request body (if any)</summary>
    public string? BodyData { get; set; }
    ///<summary>Error information.</summary>
    public string? Error { get; set; }
    ///<summary>IP Address of the caller</summary>
    public string? IPAddress { get; set; }
    ///<summary>Http status code</summary>
    public string? StatusCode { get; set; }

    ///<summary>Request was succesfull</summary>
    public bool Success {
      get { return "200" == StatusCode; }
    }

  }
}
