using System;

namespace Tlabs.Data {

  ///<summary>Interface of a data seeding campaign.</summary>
  public interface IDataSeed {
    ///<summary>Name of the data seeding campaign.</summary>
    string Campaign { get; }
    ///<summary>Perform the data seeding campaign.</summary>    
    void Perform();
  }

  ///<summary>Interface of a master-data seeding campaign.</summary>
  public interface IMasterDataSeed : IDataSeed { }

  ///<summary>Abstract data seeding campaign.</summary>
  public abstract class AbstractDataSeed : IDataSeed {
    ///<summary>Valid-from date for default master data.</summary>
    public static readonly DateTime DEFAULT_VALIDFROM= new DateTime(1900, 1, 1).AddMonths(-1);
    ///<summary><see cref="IDataStore"/> used for data seeding.</summary>
    protected IDataStore store;
    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public AbstractDataSeed(IDataStore store) {
      this.store= store;
    }
    ///<inheritdoc/>
    public abstract string Campaign { get; }
    ///<inheritdoc/>
    public abstract void Perform();
  }

  ///<summary>Abstract master-data seeding campaign.</summary>
  public abstract class AbstractMasterDataSeed : AbstractDataSeed, IMasterDataSeed {
    ///<summary>Ctor from <paramref name="store"/>.</summary>
    public AbstractMasterDataSeed(IDataStore store) : base(store) { }
  }
}