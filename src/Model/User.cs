#pragma warning disable CS1591

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tlabs.Data.Model {
  public class User {
    public User() { }

    public User(Tlabs.Data.Entity.User u) {
      this.Status= u.Status;
      this.Modified= u.Modified;
      this.Username= u.UserName;
      this.FirstName= u.Firstname;
      this.LastName= u.Lastname;
      this.Email= u.Email;
      this.Lang= u.Locale?.Lang;
      this.RoleIds= u.Roles?.Select(x => x.Role?.Name).Where(r => null != r).ToList()!;
      this.Roles= u.Roles?.Where(r => null != r.Role).Select(r => new Role(r.Role!)).ToList();
    }

    public Tlabs.Data.Entity.User AsEntity(ICachedRepo<Tlabs.Data.Entity.Locale>? locRepo= null) {
      var loc= resolveLocale(locRepo);
      return new Tlabs.Data.Entity.User {
        Status= this.Status,
        UserName= this.Username,
        Firstname= this.FirstName,
        Lastname= this.LastName,
        Email= this.Email,
        Locale= loc
      };
    }

    public Tlabs.Data.Entity.User CopyTo(Tlabs.Data.Entity.User ent, ICachedRepo<Tlabs.Data.Entity.Locale>? locRepo= null) {
      var loc= resolveLocale(locRepo);
      ent.Status= this.Status ?? ent.Status;
      ent.UserName= this.Username ?? ent.UserName;
      ent.Firstname= this.FirstName ?? ent.Firstname;
      ent.Lastname= this.LastName ?? ent.Lastname;
      ent.Email= this.Email ?? ent.Email;
      ent.Locale= loc ?? ent.Locale;
      return ent;
    }

    public User CopyTo(User usr) {
      usr.Status= this.Status ?? usr.Status;
      usr.Username= this.Username ?? usr.Username;
      usr.FirstName= this.FirstName ?? usr.FirstName;
      usr.LastName= this.LastName ?? usr.LastName;
      usr.Email= this.Email ?? usr.Email;
      usr.Modified= this.Modified ?? usr.Modified;
      usr.Lang= this.Lang ?? usr.Lang;
      return usr;
    }

    private Tlabs.Data.Entity.Locale? resolveLocale(ICachedRepo<Tlabs.Data.Entity.Locale>? locRepo) {
      var loc=   !string.IsNullOrEmpty(this.Lang) && null != locRepo
                ? locRepo.AllUntracked().Where(l => this.Lang.Equals(l.Lang, StringComparison.OrdinalIgnoreCase)).SingleOrDefault()
                : null;
      if (null != loc) loc= locRepo?.Attach(loc); // mark as attached (to avoid insert of new Locale)
      return loc;
    }

    public string? Status;
    public DateTime? Modified;
    public string? Username;
    public string? Password;
    public string? FirstName;
    public string? LastName;
    public string? Email;
    public string? Lang;
    public IList<string>? RoleIds;
    public IList<Role>? Roles;
  }
}