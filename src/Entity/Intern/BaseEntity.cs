using System;
using Tlabs.Data.Event;

namespace Tlabs.Data.Entity.Intern {

  ///<summary>Common entity base.</summary>
  public abstract class BaseEntity {

    ///<summary>Indentifier.</summary>
    public int Id { get; set; }

    ///<summary>Check for entity equality.</summary>
    ///<remarks>
    ///<para>The override returns false if <paramref name="o"/> is null or the types of this and <paramref name="o"/> do not match exactly.</para>
    ///<para>Otherwise the <see cref="BaseEntity.Id"/> is checked for equality.
    /// (If this object's Id is the default value (0), it is assumed of not beeing stored yet and thus referenc equality is checked.)
    ///</para>
    ///</remarks>
    public override bool Equals(object? o) {
      if (o is not BaseEntity obj || GetType() != o.GetType())
        return false;

      return default(int) != Id
             ? Id == obj.Id
             : Object.ReferenceEquals(this, o);
    }

    ///<inheritdoc/>
    public override int GetHashCode() {
      return   default(int) != Id
             ? Id
             : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
    }

#if false
    /* These equality operators are messing up EF Linq queries, so let's disable them...
     */
    ///<summary>operator ==</summary>
    public static bool operator ==(BaseEntity a, BaseEntity b) {
      if (System.Object.ReferenceEquals(a, b)) return true;
      if (null == a) return false;
      return a.Equals(b);
    }

    ///<summary>operator !=</summary>
    public static bool operator !=(BaseEntity a, BaseEntity b) => !(a == b);
#endif

    ///<summary>New shallow copy of <typeparamref name="T"/> with <see cref="BaseEntity.Id"/> == 0.</summary>
    public T NewCopy<T>() where T : BaseEntity {
      var copy= (T)this.MemberwiseClone();
      copy.Id= default(int);
      return copy;
    }
  }
}
