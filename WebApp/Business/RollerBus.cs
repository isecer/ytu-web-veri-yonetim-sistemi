using System;
using System.Linq;
using BiskaUtil;
using Database;
using WebApp.Models;

namespace WebApp.Business
{
    public static class RollerBus
    {
        public static void UpdateRoles()
        {
            var roleAttrs = Membership.Roles();
            using (var db = new VysDBEntities())
            {
                RemoveRoles();
                var dbRoller = db.Rollers.ToArray();
                foreach (var attr in roleAttrs)
                {
                    var dbrole = dbRoller.FirstOrDefault(p => p.RolID == attr.RolID || p.RolAdi == attr.RolAdi);

                    if (dbrole == null)
                    {

                        db.Rollers.Add(new Roller
                        {
                            RolID = attr.RolID,
                            GorunurAdi = attr.GorunurAdi,
                            Aciklama = attr.Aciklama,
                            Kategori = attr.Kategori,
                            RolAdi = attr.RolAdi
                        });
                    }
                    else
                    {

                        //dbrole.RolID = attr.RolID;
                        dbrole.GorunurAdi = attr.GorunurAdi;
                        dbrole.Aciklama = attr.Aciklama;
                        dbrole.Kategori = attr.Kategori;
                        dbrole.RolAdi = attr.RolAdi;
                        if (dbrole.RolID != attr.RolID)
                        {
                            try
                            {

                                db.sp_RollKeyUpdate(dbrole.RolID, attr.RolID);
                            }
                            catch (Exception e)
                            {
                               SistemBilgilendirmeBus.SistemBilgisiKaydet(e.Message,e.StackTrace,BilgiTipi.Kritik);
                            } 
                        }
                    }

                }

                db.SaveChanges();
            }
        }

        //private static void ChangeKeyUpdateRoles(int dbRoleId, RoleAttribute roleAttribute)
        //{
        //    using (var db = new VysDBEntities())
        //    {
        //        var role = db.Rollers.First(f => f.RolID == dbRoleId);

        //        var roluOlanKullanicilar = role.Kullanicilars.ToList();
        //        var roluBulunanYetkiGruplari = role.YetkiGrupRolleris.ToList();
        //        var roleAitMenuRolleri = role.Menulers.ToList(); 
        //        db.Rollers.Remove(role);
        //        db.SaveChanges();
        //        var newRole = new Roller
        //        {
        //            RolID = roleAttribute.RolID,
        //            GorunurAdi = roleAttribute.GorunurAdi,
        //            Aciklama = roleAttribute.Aciklama,
        //            RolAdi = roleAttribute.RolAdi,
        //            Kategori = roleAttribute.Kategori
        //        };

        //        var addedRole = db.Rollers.Add(newRole);
        //        db.SaveChanges();
        //        db.YetkiGrupRolleris.AddRange(roluBulunanYetkiGruplari.Select(s => new YetkiGrupRolleri
        //        { RolID = newRole.RolID, YetkiGrupID = s.YetkiGrupID }));
        //        foreach (var itemMenu in roleAitMenuRolleri)
        //        {
        //            itemMenu.Rollers.Add(addedRole);
        //        }
        //        foreach (var itemMenu in roluOlanKullanicilar)
        //        {
        //            itemMenu.Rollers.Add(addedRole);
        //        }

        //        db.SaveChanges();
        //    }
        //}
        private static void RemoveRoles()
        {
            using (var db = new VysDBEntities())
            {
                var roleAttrs = Membership.Roles();
                var dbRoller = db.Rollers.ToArray();
                var silinenRoller = dbRoller.Where(p => roleAttrs.All(a => a.RolID != p.RolID && a.RolAdi != p.RolAdi)).ToList();
                var silinenRolIds = silinenRoller.Select(s => s.RolID).ToList();
                var silinecekKullaniciRolleris = db.Kullanicilars
                    .Where(p => p.Rollers.Any(a => silinenRolIds.Contains(a.RolID))).ToList();
                foreach (var kul in silinecekKullaniciRolleris)
                {
                    foreach (var rol in silinenRoller)
                    {
                        kul.Rollers.Remove(rol);
                    }
                }
                var yetkiGrupRolleris = db.YetkiGrupRolleris.Where(p => silinenRolIds.Contains(p.RolID)).ToList();
                db.YetkiGrupRolleris.RemoveRange(yetkiGrupRolleris);
                foreach (var rol in silinenRoller)
                {
                    db.Rollers.Remove(rol);
                }

                db.SaveChanges();
            }
        }

        public static Roller[] GetAllRoles(bool tumuOrYetkiIstenenler = true)
        {
            using (VysDBEntities db = new VysDBEntities())
            {
                //return db.Rollers.ToArray();//-----------------------------------------------------------<<<Silinecek
                var roller = db.Rollers.AsQueryable();
                if (!tumuOrYetkiIstenenler)
                {
                    roller = db.Rollers.Where(p => p.Menulers.Any(a => a.YetkisizErisim.HasValue && a.YetkisizErisim.Value == false));
                }
                return roller.OrderBy(o => o.Kategori).ThenBy(t => t.RolAdi).ToArray();

            }
        }
    }
}