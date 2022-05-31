using LinqKit;
using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POS_Server.Classes;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;




namespace POS_Server.Controllers
{
    [RoutePrefix("api/Statistics")]
    public class StatisticsController : ApiController
    {

        public List<int> AllowedBranchsId(int mainBranchId, int userId)
        {
            BranchesController branchc = new BranchesController();
            List<branches> branchesList = new List<branches>();
            branchesList = branchc.BrListByBranchandUser(mainBranchId, userId);
            List<int> bridlist = new List<int>();
            // Calculate calc = new Calculate();
            bridlist.AddRange(branchesList.Select(x => x.branchId).ToList());
            return bridlist;
        }

        private decimal getupValues(int miniuId, int maxiuId, int itemId)
        {
            decimal val = 1;
            using (incposdbEntities entity = new incposdbEntities())
            {
                var iulist = entity.itemsUnits.Where(I => I.itemId == itemId).Select(I => new { I.unitId, I.itemId, I.subUnitId, I.itemUnitId, I.unitValue, });

                while (miniuId != maxiuId)
                {
                    var minitem = iulist.Where(I => I.itemUnitId == miniuId).FirstOrDefault();

                    var upitem = iulist.Where(I => I.subUnitId == minitem.unitId).FirstOrDefault();
                    val = val * Convert.ToDecimal(upitem.unitValue);
                    miniuId = upitem.itemUnitId;
                }
                if (val != 0)
                {
                    val = 1 / val;
                }

            }
            return val;
        }
        private decimal getItemSubUnitAmount(int itemUnitId, int branchId)
        {
            decimal amount = 0;
            int subUnit = 0;
            int itemId = 0;
            int nextunitID = 0;
            int nextIUid = 0;
            decimal vale = 0;
            if (itemUnitId > 0)
            {




                using (incposdbEntities entity = new incposdbEntities())
                {
                    var nextunit = entity.itemsUnits.Where(I => I.subUnitId == itemUnitId)
                        .Select(x => new { x.itemUnitId, x.subUnitId, x.itemId, x.unitValue, x.unitId }).FirstOrDefault();

                    var itemInLocs = (from b in entity.branches
                                      where b.branchId == branchId
                                      join s in entity.sections on b.branchId equals s.branchId
                                      join l in entity.locations on s.sectionId equals l.sectionId
                                      join il in entity.itemsLocations on l.locationId equals il.locationId
                                      where il.itemUnitId == nextunit.itemUnitId && il.quantity > 0
                                      select new
                                      {
                                          il.itemsLocId,
                                          il.quantity,
                                          il.itemUnitId,
                                          il.locationId,
                                          s.sectionId,
                                      }).ToList();
                    if (itemInLocs.Count > 0)
                    {
                        for (int i = 0; i < itemInLocs.Count; i++)
                        {
                            amount += Convert.ToDecimal(itemInLocs[i].quantity);
                        }

                        vale = getupValues(itemUnitId, nextunit.itemUnitId, (int)nextunit.itemId);
                        amount = amount * vale;

                        var itemunit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.itemUnitId, x.subUnitId, x.itemId, x.unitValue, x.unitId }).FirstOrDefault();
                        if (itemunit != null)
                        {

                            subUnit = (int)itemunit.unitId;
                            itemId = (int)itemunit.itemId;
                            nextunitID = (int)itemunit.subUnitId;

                        }

                        var nextUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.unitId == nextunitID).Select(x => new { x.unitId, x.itemId, x.subUnitId, x.itemUnitId, x.unitValue, }).FirstOrDefault();
                        if (nextUnit != null)
                        {
                            nextIUid = nextUnit.itemUnitId;
                        }
                        var downUnit = entity.itemsUnits.Where(x => x.itemId == itemId && x.subUnitId == subUnit).Select(x => new { x.unitId, x.itemId, x.subUnitId, x.itemUnitId, x.unitValue, }).FirstOrDefault();


                        if (downUnit != null)
                        {
                            if (Convert.ToDecimal(downUnit.unitValue) > 0)
                            {

                                // amount += Convert.ToDecimal(getItemSubUnitAmount(nextIUid, branchId)) / Convert.ToDecimal(downUnit.unitValue);
                                amount = amount + Convert.ToDecimal(getItemSubUnitAmount(nextIUid, branchId)) / Convert.ToDecimal(downUnit.unitValue);
                            }

                            return amount;
                        }
                        else
                        {
                            return 0;
                        }

                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            else
            {
                return 0;
            }
        }

        private int getItemUnitAmount(int itemUnitId, int branchId)
        {

            int amount = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemInLocs = (from b in entity.branches
                                  where b.branchId == branchId
                                  join s in entity.sections on b.branchId equals s.branchId
                                  join l in entity.locations on s.sectionId equals l.sectionId
                                  join il in entity.itemsLocations on l.locationId equals il.locationId
                                  where il.itemUnitId == itemUnitId && il.quantity > 0
                                  select new
                                  {
                                      il.itemsLocId,
                                      il.quantity,
                                      il.itemUnitId,
                                      il.locationId,
                                      s.sectionId,
                                  }).ToList();
                for (int i = 0; i < itemInLocs.Count; i++)
                {
                    amount += (int)itemInLocs[i].quantity;
                }
                //amount  الكمية في الفرع لعنصر ووحدة قياس واحدة
                // جلب معرف الوحدة ومعرف العنصر
                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                //جلب الوحدة الاعلى معرف الوحدة الاعلى وقيمتها بالنسبة للوحدة الادنى 
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit != null)
                    //جلب الكمية للوحدة الاعلى في الفرع وضربها بقيمة الوحدة
                    amount += (int)upperUnit.unitValue * getItemUnitAmount(upperUnit.itemUnitId, branchId);
                return amount;
            }

        }

        // item quantity in location GetItemQtyInBranches()
        //        [HttpPost]
        //        [Route("GetItemQtyInBranches")]
        //        public IHttpActionResult GetItemQtyInBranches(int itemId, int UnitId, string BranchType)
        //        {
        //           
        //            
        //            string token = "";
        //            int iuId = 0;
        //            int subUnit = 0;
        //            int subitemunitId = 0;
        //            string unitName = "";
        //            if (headers.Contains("APIKey"))
        //            {
        //                token = headers.GetValues("APIKey").First();
        //            }
        //            Validation validation = new Validation();
        //            bool valid = validation.CheckApiKey(token);

        //            if (valid) // APIKey is valid
        //            {
        //                using (incposdbEntities entity = new incposdbEntities())
        //                {

        //                    var itemunit = entity.itemsUnits.Where(x => x.itemId == itemId && x.unitId == UnitId).Select(x => new { x.itemUnitId, x.subUnitId }).FirstOrDefault();
        //                    if (itemunit != null)
        //                    {
        //                        iuId = itemunit.itemUnitId;//ref iuid
        //                        subUnit = (int)itemunit.subUnitId;
        //                    }
        //                    if (subUnit > 0)
        //                    {
        //                        var subitemunit = entity.itemsUnits.Where(x => x.itemId == itemId && x.unitId == subUnit).Select(x => new { x.itemUnitId, x.subUnitId }).FirstOrDefault();
        //                        if (subitemunit != null)
        //                        {
        //                            subitemunitId = subitemunit.itemUnitId;
        //                        }
        //                    }
        //                    var unit = entity.units.Where(x => x.unitId == UnitId).Select(x => new { x.name }).FirstOrDefault();
        //                    if (unit != null)
        //                    {
        //                        unitName = unit.name;
        //                    }

        //                    var brList = (from IL in entity.itemsLocations
        //                                  from B in entity.branches
        //                                  from S in entity.sections.Where(x => x.branchId == B.branchId)
        //                                  from L in entity.locations.Where(x => x.locationId == IL.locationId && x.sectionId == S.sectionId)
        //                                      //   join L in entity.locations on IL.locationId equals L.locationId into JL
        //                                  join IU in entity.itemsUnits on IL.itemUnitId equals IU.itemUnitId
        //                                  join I in entity.items on IU.itemId equals I.itemId
        //                                  join U in entity.units on IU.unitId equals U.unitId

        //                                  // join L in entity.locations on S.sectionId equals L.sectionId into JLS
        //                                  join SS in entity.sections on B.branchId equals SS.branchId


        //                                  //    from e in db.Emails.Where(x => x.id_contact == c.id).DefaultIfEmpty()

        //                                  where IU.itemId == itemId
        //                                  group new { IL, S, B, L, IU, I, U, SS } by (B.branchId) into g


        //                                  select new
        //                                  {

        //                                      itemsLocId = g.Select(i => i.IL.itemsLocId).FirstOrDefault(),
        //                                      branchName = g.Select(i => i.B.name).FirstOrDefault(),
        //                                      branchId = g.Select(i => i.B.branchId).FirstOrDefault(),
        //                                      sectionId = g.Select(i => i.S.sectionId).FirstOrDefault(),
        //                                      itemName = g.Select(i => i.I.name).FirstOrDefault(),
        //                                      unitName = g.Select(i => i.I.name).FirstOrDefault(),
        //                                      //   quantity = g.Sum(i => i.IL.quantity),
        //                                      // quantity= getItemUnitAmount(iuId, g.FirstOrDefault().B.branchId),
        //                                      // quantity = getItemUnitAmount(iuId, 7),
        //                                      count = g.Count(),
        //                                      itemUnitId = iuId,


        //                                      /*
        //                                      B.branchId,
        //                                      B.name,
        //                                          IL.itemsLocId,
        //                                          IL.locationId,
        //                                          IL.itemUnitId,
        //                                          IU.unitValue,
        //                                          IU.subUnitId,
        //                                            IU.itemId,
        //                                            IU.unitId,
        //                                         itemName= I.name,
        //                                         sectionName=S.name,
        //                                         branchName= B.name,
        //                                         IL.quantity,
        //                                        S.sectionId,
        //                                        */

        //                                  });

        //                    var groupitems = (from item in brList.AsEnumerable()
        //                                      select new
        //                                      {

        //                                          item.itemsLocId,
        //                                          item.branchName,
        //                                          item.branchId,
        //                                          item.itemName,
        //                                          item.itemUnitId,
        //                                          item.count,
        //                                          unitName,
        //                                          /*
        //                                        //  item.count,
        //                                          item.itemUnitId,
        //                                          item.subUnitId,
        //                                          item.unitValue,
        //                                          item.unitId,
        //                                          item.itemId,
        //                                          // item.quantity,
        //                                          */
        //                                          //  quantity = getItemUnitAmount(iuId, item.branchId) + getItemSubUnitAmount(subitemunitId, item.branchId),
        //                                          quantity = getItemSubUnitAmount(iuId, item.branchId),
        //                                      }).ToList();

        //                    /*
        //                     var items =  myContext.Select(i => new {
        //                   Value1 = item.Value1,
        //                   Value2 = item.Value2
        //               })
        //               .AsEnumerable()
        //               .Select(i => new {
        //                   Value1 = TweakValue(item.Value1),
        //                   Value2 = TweakValue(item.Value2)
        //                });


        //                    var query = from item in myContext
        //            where item.Foo == bar
        //            orderby item.Something
        //            select new { item.Value1, item.Value2 };

        //var items = from item in query.AsEnumerable()
        //            select new {
        //                Value1 = TweakValue(item.Value1),
        //                Value2 = TweakValue(item.Value2)
        //            };

        //                     * */
        //                    if (groupitems == null)
        //                        return NotFound();
        //                    else
        //                        return Ok(groupitems);
        //                }
        //            }
        //            //else
        //            return NotFound();
        //        }

        // for report 
        //  فواتير المشتريات بكل انواعها بكل فرع
        [HttpPost]
        [Route("GetPurinv")]
        public string GetPurinv(string token)
        {
            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var invListm = (from I in entity.invoices
                                        join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()

                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "p" || I.invType == "pb" || I.invType == "pd" || I.invType == "pbd")
                                        // (branchType == "all" ? true : JBB.type == branchType)
                                        //   && System.DateTime.Compare((DateTime)startDate, (DateTime)I.invDate) <= 0
                                        //  && System.DateTime.Compare((DateTime)endDate, (DateTime)I.invDate) >= 0
                                        // I.invType == invtype
                                        //     && branchType == "all" ? true : JBB.type == branchType

                                        //  && startDate <= I.invDate && endDate >= I.invDate
                                        // &&  System.DateTime.Compare((DateTime)startDate,  I.invDate) <= 0 && System.DateTime.Compare((DateTime)endDate, I.invDate) >= 0
                                        //   group new { I, JBB } by (I.branchId) into g
                                        select new
                                        {
                                            I.invoiceId,
                                            I.invNumber,
                                           I.invBarcode,
                                            I.agentId,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = ((I.discountType == "1" || I.discountType == null) || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,
                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            branchName = JBB.name,

                                            branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,
                                            agentName = JAA.name,
                                            agentCode = JAA.code,
                                            agentType = JAA.type,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,

                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
                //return TokenManager.GenerateToken("0");
            }






            //           
            //            
            //            string token = "";
            //            if (headers.Contains("APIKey"))
            //            {
            //                token = headers.GetValues("APIKey").First();
            //            }
            //            Validation validation = new Validation();
            //            bool valid = validation.CheckApiKey(token);

            //            if (valid) // APIKey is valid
            //            {
            //                List<int> brIds = AllowedBranchsId(mainBranchId, userId);
            //                using (incposdbEntities entity = new incposdbEntities())
            //                {
            //                    var invListm = (from I in entity.invoices
            //                                    join B in entity.branches on I.branchId equals B.branchId into JB
            //                                    join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
            //                                    join A in entity.agents on I.agentId equals A.agentId into JA
            //                                    join U in entity.users on I.createUserId equals U.userId into JU
            //                                    join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
            //                                    join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
            //                                    join P in entity.pos on I.posId equals P.posId into JP

            //                                    from JBB in JB
            //                                    from JPP in JP.DefaultIfEmpty()
            //                                    from JUU in JU.DefaultIfEmpty()
            //                                    from JUPUS in JUPUSR.DefaultIfEmpty()
            //                                    from JIMM in JIM.DefaultIfEmpty()
            //                                    from JAA in JA.DefaultIfEmpty()
            //                                    from JBCC in JBC.DefaultIfEmpty()

            //                                    where (brIds.Contains(JBCC.branchId)) && (I.invType == "p" || I.invType == "pb" || I.invType == "pd" || I.invType == "pbd")
            //                                    // (branchType == "all" ? true : JBB.type == branchType)
            //                                    //   && System.DateTime.Compare((DateTime)startDate, (DateTime)I.invDate) <= 0
            //                                    //  && System.DateTime.Compare((DateTime)endDate, (DateTime)I.invDate) >= 0
            //                                    // I.invType == invtype
            //                                    //     && branchType == "all" ? true : JBB.type == branchType

            //                                    //  && startDate <= I.invDate && endDate >= I.invDate
            //                                    // &&  System.DateTime.Compare((DateTime)startDate,  I.invDate) <= 0 && System.DateTime.Compare((DateTime)endDate, I.invDate) >= 0
            //                                    //   group new { I, JBB } by (I.branchId) into g
            //                                    select new
            //                                    {
            //                                        I.invoiceId,
            //                                        I.invNumber,
            //                                        I.agentId,
            //                                        I.posId,
            //                                        I.invType,
            //                                        I.total,
            //                                        I.totalNet,
            //                                        I.paid,
            //                                        I.deserved,
            //                                        I.deservedDate,
            //                                        I.invDate,
            //                                        I.invoiceMainId,
            //                                        I.invCase,
            //                                        I.invTime,
            //                                        I.notes,
            //                                        I.vendorInvNum,
            //                                        I.vendorInvDate,
            //                                        I.createUserId,
            //                                        I.updateDate,
            //                                        I.updateUserId,
            //                                        I.branchId,
            //                                        discountValue = ((I.discountType == "1" || I.discountType == null) || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
            //                                        I.discountType,
            //                                        I.tax,
            //                                        I.name,
            //                                        I.isApproved,
            //                                        //
            //                                        I.branchCreatorId,
            //                                        branchCreatorName = JBCC.name,
            //                                        //
            //                                        branchName = JBB.name,

            //                                        branchType = JBB.type,
            //                                        posName = JPP.name,
            //                                        posCode = JPP.code,
            //                                        agentName = JAA.name,
            //                                        agentCode = JAA.code,
            //                                        agentType = JAA.type,
            //                                        cuserName = JUU.name,
            //                                        cuserLast = JUU.lastname,
            //                                        cUserAccName = JUU.username,
            //                                        uuserName = JUPUS.name,
            //                                        uuserLast = JUPUS.lastname,
            //                                        uUserAccName = JUPUS.username,
            //                                        agentCompany = JAA.company,

            //                                        //username

            //                                        //  I.invoiceId,
            //                                        //    JBB.name
            //                                    }).ToList();

            //                    /*
            //          if(S.(I.discountType == "1" || I.discountType ==null ))
            //{
            //    return S.I.discountValue;
            //}else if(S.I.discountType == "2")
            //{
            //   return (S.I.discountValue / 100);
            //}
            //else
            //{
            //    return 0;
            //}
            //*/



            //                    if (invListm == null)
            //                        return NotFound();
            //                    else
            //                        return Ok(invListm);
            //                }

            //            }

            //            //else
            //            return NotFound();




        }

        // العناصر في الفواتير

        [HttpPost]
        [Route("GetPuritem")]
        public string GetPuritem(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                        join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                        join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "p" || I.invType == "pw" || I.invType == "pb")

                                        select new
                                        {
                                            ITitemName = ITEM.name,
                                            ITunitName = UNIT.name,
                                            ITitemsTransId = IT.itemsTransId,
                                            ITitemUnitId = IT.itemUnitId,

                                            ITitemId = IU.itemId,
                                            ITunitId = IU.unitId,
                                            ITquantity = IT.quantity,

                                            ITcreateDate = IT.createDate,
                                            ITupdateDate = IT.updateDate,
                                            ITcreateUserId = IT.createUserId,
                                            ITupdateUserId = IT.updateUserId,
                                            ITnotes = IT.notes,
                                            ITprice = IT.price,
                                            ITbarcode = IU.barcode,
                                            ITCreateuserName = ITCUSER.name,
                                            ITCreateuserLName = ITCUSER.lastname,
                                            ITCreateuserAccName = ITCUSER.username,

                                            ITUpdateuserName = ITUPUSER.name,
                                            ITUpdateuserLName = ITUPUSER.lastname,
                                            ITUpdateuserAccName = ITUPUSER.username,
                                            I.invoiceId,
                                            I.invNumber,
                                           I.invBarcode,
                                            I.agentId,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? ((I.discountValue / 100) * I.total) : 0),
                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            branchName = JBB.name,

                                            branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,
                                            agentName = JAA.name,
                                            agentCode = JAA.code,
                                            agentType = JAA.type,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,
                                            subTotal = (IT.price * IT.quantity),
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }
                //return TokenManager.GenerateToken("0");


                //           
                //            
                //            string token = "";
                //            if (headers.Contains("APIKey"))
                //            {
                //                token = headers.GetValues("APIKey").First();
                //            }
                //            Validation validation = new Validation();
                //            bool valid = validation.CheckApiKey(token);

                //            if (valid) // APIKey is valid
                //            {
                //                List<int> brIds = AllowedBranchsId(mainBranchId, userId);

                //                using (incposdbEntities entity = new incposdbEntities())
                //                {
                //                    var invListm = (from IT in entity.itemsTransfer
                //                                    from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                //                                    from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                //                                    join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                //                                    join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                //                                    join ITEM in entity.items on IU.itemId equals ITEM.itemId
                //                                    join UNIT in entity.units on IU.unitId equals UNIT.unitId
                //                                    join B in entity.branches on I.branchId equals B.branchId into JB
                //                                    join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                //                                    join A in entity.agents on I.agentId equals A.agentId into JA
                //                                    join U in entity.users on I.createUserId equals U.userId into JU
                //                                    join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                //                                    join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                //                                    join P in entity.pos on I.posId equals P.posId into JP

                //                                    from JBB in JB
                //                                    from JPP in JP.DefaultIfEmpty()
                //                                    from JUU in JU.DefaultIfEmpty()
                //                                    from JUPUS in JUPUSR.DefaultIfEmpty()
                //                                    from JIMM in JIM.DefaultIfEmpty()
                //                                    from JAA in JA.DefaultIfEmpty()
                //                                    from JBCC in JBC.DefaultIfEmpty()
                //                                    where (brIds.Contains(JBCC.branchId)) && (I.invType == "p" || I.invType == "pw" || I.invType == "pb")

                //                                    select new
                //                                    {
                //                                        ITitemName = ITEM.name,
                //                                        ITunitName = UNIT.name,
                //                                        ITitemsTransId = IT.itemsTransId,
                //                                        ITitemUnitId = IT.itemUnitId,

                //                                        ITitemId = IU.itemId,
                //                                        ITunitId = IU.unitId,
                //                                        ITquantity = IT.quantity,

                //                                        ITcreateDate = IT.createDate,
                //                                        ITupdateDate = IT.updateDate,
                //                                        ITcreateUserId = IT.createUserId,
                //                                        ITupdateUserId = IT.updateUserId,
                //                                        ITnotes = IT.notes,
                //                                        ITprice = IT.price,
                //                                        ITbarcode = IU.barcode,
                //                                        ITCreateuserName = ITCUSER.name,
                //                                        ITCreateuserLName = ITCUSER.lastname,
                //                                        ITCreateuserAccName = ITCUSER.username,

                //                                        ITUpdateuserName = ITUPUSER.name,
                //                                        ITUpdateuserLName = ITUPUSER.lastname,
                //                                        ITUpdateuserAccName = ITUPUSER.username,
                //                                        I.invoiceId,
                //                                        I.invNumber,
                //                                        I.agentId,
                //                                        I.posId,
                //                                        I.invType,
                //                                        I.total,
                //                                        I.totalNet,
                //                                        I.paid,
                //                                        I.deserved,
                //                                        I.deservedDate,
                //                                        I.invDate,
                //                                        I.invoiceMainId,
                //                                        I.invCase,
                //                                        I.invTime,
                //                                        I.notes,
                //                                        I.vendorInvNum,
                //                                        I.vendorInvDate,
                //                                        I.createUserId,
                //                                        I.updateDate,
                //                                        I.updateUserId,
                //                                        I.branchId,
                //                                        discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? ((I.discountValue / 100) * I.total) : 0),
                //                                        I.discountType,
                //                                        I.tax,
                //                                        I.name,
                //                                        I.isApproved,

                //                                        //
                //                                        I.branchCreatorId,
                //                                        branchCreatorName = JBCC.name,
                //                                        //
                //                                        branchName = JBB.name,

                //                                        branchType = JBB.type,
                //                                        posName = JPP.name,
                //                                        posCode = JPP.code,
                //                                        agentName = JAA.name,
                //                                        agentCode = JAA.code,
                //                                        agentType = JAA.type,
                //                                        cuserName = JUU.name,
                //                                        cuserLast = JUU.lastname,
                //                                        cUserAccName = JUU.username,
                //                                        uuserName = JUPUS.name,
                //                                        uuserLast = JUPUS.lastname,
                //                                        uUserAccName = JUPUS.username,
                //                                        agentCompany = JAA.company,
                //                                        subTotal = (IT.price * IT.quantity),
                //                                        //username

                //                                        //  I.invoiceId,
                //                                        //    JBB.name
                //                                    }).ToList();

                //                    /*
                //   public int ITitemsTransId { get; set; }
                //        public Nullable<int> ITitemId { get; set; }
                //        public string ITitemName { get; set; }
                //        public Nullable<long> ITquantity { get; set; }
                //        public Nullable<int> ITinvoiceId { get; set; }
                //        public string ITinvNumber { get; set; }
                //        public Nullable<int> ITlocationIdNew { get; set; }
                //        public Nullable<int> ITlocationIdOld { get; set; }
                //        public Nullable<System.DateTime> ITcreateDate { get; set; }
                //        public Nullable<System.DateTime> ITupdateDate { get; set; }
                //        public Nullable<int> ITcreateUserId { get; set; }
                //        public Nullable<int> ITupdateUserId { get; set; }
                //        public string ITnotes { get; set; }
                //        public Nullable<decimal> ITprice { get; set; }
                //        public Nullable<int> ITitemUnitId { get; set; }

                //        public string ITunitName { get; set; }
                //        public string ITbarcode { get; set; }

                //ITitemsTransId
                //ITitemId
                //ITitemName
                //ITquantity
                //ITinvoiceId
                //ITinvNumber
                //ITcreateDate
                //ITupdateDate
                //ITcreateUserId
                //ITupdateUserId
                //ITnotes
                //ITprice
                //ITitemUnitId
                //ITunitName
                //ITbarcode


                //*/



                //                    if (invListm == null)
                //                        return NotFound();
                //                    else
                //                        return Ok(invListm);
                //                }

                //            }

                //            //else
                //            return NotFound();
            }
        }
        //عدد العناصر في كل فاتورة
        [HttpPost]
        [Route("GetPuritemcount")]
        public string GetPuritemcount(string token)
        {


            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-2).ToShortDateString());
                    DateTime dt1 = Convert.ToDateTime(DateTime.Today.AddDays(-1).ToShortDateString());
                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var invListm = (from I in entity.invoices

                                        join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "p" || I.invType == "pw" || I.invType == "pb" || I.invType == "pbd" || I.invType == "pd" || I.invType == "pbw")

                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                            I.invBarcode,
                                            I.agentId,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            //I.paid,
                                            //I.deserved,
                                            //I.deservedDate,
                                            //I.invDate,
                                            //I.invoiceMainId,
                                            //I.invCase,
                                            //I.invTime,
                                            //I.notes,
                                            //I.vendorInvNum,
                                            //I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            //I.updateUserId,
                                            I.branchId,
                                            I.discountType,
                                            I.tax,
                                            //I.name,
                                            //I.isApproved,
                                            discountValue = ((I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? ((I.discountValue / 100) * I.total) : 0))
                                                                         + ((I.manualDiscountType == "1" || I.discountType == null) ? I.manualDiscountValue : (I.manualDiscountType == "2" ? ((I.manualDiscountValue / 100) * I.total) : 0))
                                                                          ,
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            //branchName = JBB.name,
                                            //branchType = JBB.type,
                                            posName = JPP.name,
                                            //posCode = JPP.code,
                                            agentName = JAA.name,
                                            //agentCode = JAA.code,
                                            //agentType = JAA.type,
                                            //cuserName = JUU.name,
                                            //cuserLast = JUU.lastname,
                                            //cUserAccName = JUU.username,
                                            //uuserName = JUPUS.name,
                                            //uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,

                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                            archived = ((DateTime)I.updateDate >= ((I.invType == "pbd" || I.invType == "pd") ? dt : dt1)) ? 0 : 1,
                                        }).ToList();

                        // archived = ((DateTime)I.updateDate >= dt) ?0:1,
                        // DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-2).ToShortDateString());

                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");

                }

            }

        }


        // عدد العناصر في كل فاتورة


        //        [HttpPost]
        //        [Route("GetPurinvwithCount")]
        //        public IHttpActionResult GetPurinvwithCount()
        //        {
        //           
        //            
        //            string token = "";
        //            if (headers.Contains("APIKey"))
        //            {
        //                token = headers.GetValues("APIKey").First();
        //            }
        //            Validation validation = new Validation();
        //            bool valid = validation.CheckApiKey(token);

        //            if (valid) // APIKey is valid
        //            {
        //                using (incposdbEntities entity = new incposdbEntities())
        //                {
        //                    var invListm = (from I in entity.invoices
        //                                    join B in entity.branches on I.branchId equals B.branchId into JB
        //                                    join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
        //                                    join A in entity.agents on I.agentId equals A.agentId into JA
        //                                    join U in entity.users on I.createUserId equals U.userId into JU
        //                                    join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
        //                                    join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
        //                                    join P in entity.pos on I.posId equals P.posId into JP
        //                                    join IT in entity.itemsTransfer on I.invoiceId equals IT.invoiceId into JIT
        //                                    from JBB in JB.DefaultIfEmpty()
        //                                    from JPP in JP.DefaultIfEmpty()
        //                                    from JUU in JU.DefaultIfEmpty()
        //                                    from JUPUS in JUPUSR.DefaultIfEmpty()
        //                                    from JIMM in JIM.DefaultIfEmpty()
        //                                    from JAA in JA.DefaultIfEmpty()
        //                                    from JITT in JIT.DefaultIfEmpty()
        //                                    from JBCC in JBC.DefaultIfEmpty()

        //                                    where (I.invType == "p" || I.invType == "pb" || I.invType == "pd" || I.invType == "pbd")
        //                                    // (branchType == "all" ? true : JBB.type == branchType)
        //                                    //   && System.DateTime.Compare((DateTime)startDate, (DateTime)I.invDate) <= 0
        //                                    //  && System.DateTime.Compare((DateTime)endDate, (DateTime)I.invDate) >= 0
        //                                    // I.invType == invtype
        //                                    //     && branchType == "all" ? true : JBB.type == branchType

        //                                    //  && startDate <= I.invDate && endDate >= I.invDate
        //                                    // &&  System.DateTime.Compare((DateTime)startDate,  I.invDate) <= 0 && System.DateTime.Compare((DateTime)endDate, I.invDate) >= 0
        //                                    group new { I, JBB, JPP, JUU, JUPUS, JIMM, JAA, JITT } by (I.invoiceId) into g
        //                                    select new
        //                                    {
        //                                        countP = g.Select(S => S.JITT.invoiceId).Count(),
        //                                        invoiceId = g.Select(S => S.I.invoiceId).FirstOrDefault(),
        //                                        invNumber = g.Select(S => S.I.invNumber).FirstOrDefault(),
        //                                        agentId = g.Select(S => S.I.agentId).FirstOrDefault(),

        //                                        invType = g.Select(S => S.I.invType).FirstOrDefault(),
        //                                        total = g.Select(S => S.I.total).FirstOrDefault(),
        //                                        totalNet = g.Select(S => S.I.totalNet).FirstOrDefault(),
        //                                        paid = g.Select(S => S.I.paid).FirstOrDefault(),
        //                                        deserved = g.Select(S => S.I.deserved).FirstOrDefault(),
        //                                        deservedDate = g.Select(S => S.I.deservedDate).FirstOrDefault(),
        //                                        invDate = g.Select(S => S.I.invDate).FirstOrDefault(),
        //                                        invoiceMainId = g.Select(S => S.I.invoiceMainId).FirstOrDefault(),
        //                                        invCase = g.Select(S => S.I.invCase).FirstOrDefault(),
        //                                        invTime = g.Select(S => S.I.invTime).FirstOrDefault(),
        //                                        notes = g.Select(S => S.I.notes).FirstOrDefault(),
        //                                        vendorInvNum = g.Select(S => S.I.vendorInvNum).FirstOrDefault(),
        //                                        vendorInvDate = g.Select(S => S.I.vendorInvDate).FirstOrDefault(),
        //                                        createUserId = g.Select(S => S.I.createUserId).FirstOrDefault(),
        //                                        updateDate = g.Select(S => S.I.updateDate).FirstOrDefault(),
        //                                        updateUserId = g.Select(S => S.I.updateUserId).FirstOrDefault(),
        //                                        branchId = g.Select(S => S.I.branchId).FirstOrDefault(),
        //                                        // discountValue=g.Select(S=>S.I.discountValue).FirstOrDefault(),
        //                                        discountType = g.Select(S => S.I.discountType).FirstOrDefault(),
        //                                        tax = g.Select(S => S.I.tax).FirstOrDefault(),
        //                                        name = g.Select(S => S.I.name).FirstOrDefault(),
        //                                        isApproved = g.Select(S => S.I.isApproved).FirstOrDefault(),
        //                                        branchName = g.Select(S => S.JBB.name).FirstOrDefault(),
        //                                        branchType = g.Select(S => S.JBB.type).FirstOrDefault(),
        //                                        posName = g.Select(S => S.JPP.name).FirstOrDefault(),
        //                                        posCode = g.Select(S => S.JPP.code).FirstOrDefault(),
        //                                        agentName = g.Select(S => S.JAA.name).FirstOrDefault(),
        //                                        agentCode = g.Select(S => S.JAA.code).FirstOrDefault(),
        //                                        agentType = g.Select(S => S.JAA.type).FirstOrDefault(),
        //                                        cuserName = g.Select(S => S.JUU.name).FirstOrDefault(),
        //                                        cuserLast = g.Select(S => S.JUU.lastname).FirstOrDefault(),
        //                                        cUserAccName = g.Select(S => S.JUU.username).FirstOrDefault(),
        //                                        uuserName = g.Select(S => S.JUPUS.name).FirstOrDefault(),
        //                                        uuserLast = g.Select(S => S.JUPUS.lastname).FirstOrDefault(),
        //                                        uUserAccName = g.Select(S => S.JUPUS.username).FirstOrDefault(),
        //                                        agentCompany = g.Select(S => S.JAA.company).FirstOrDefault(),
        //                                        posId = g.Select(S => S.JPP.posId).FirstOrDefault(),
        //                                        discountValue = g.Select(
        //                                            S => (S.I.discountType == "1" || S.I.discountType == null) ? S.I.discountValue
        //                                            : (S.I.discountType == "2" ? (S.I.discountValue / 100) : 0))
        //                                        .FirstOrDefault(),
        //                                        /*
        //                                        I.invoiceId,
        //                                        I.invNumber,
        //                                        I.agentId,

        //                                        I.invType,
        //                                        I.total,
        //                                        I.totalNet,
        //                                        I.paid,
        //                                        I.deserved,
        //                                        I.deservedDate,
        //                                        I.invDate,
        //                                        I.invoiceMainId,
        //                                        I.invCase,
        //                                        I.invTime,
        //                                        I.notes,
        //                                        I.vendorInvNum,
        //                                        I.vendorInvDate,
        //                                        I.createUserId,
        //                                        I.updateDate,
        //                                        I.updateUserId,
        //                                        I.branchId,
        //                                        I.discountValue,
        //                                        I.discountType,
        //                                        I.tax,
        //                                        I.name,
        //                                        I.isApproved,
        //                                        branchName = JBB.name,
        //                                        branchType = JBB.type,
        //                                        posName = JPP.name,
        //                                        posCode = JPP.code,
        //                                        agentName = JAA.name,
        //                                        agentCode = JAA.code,
        //                                        cuserName = JUU.name,
        //                                        cuserLast = JUU.lastname,
        //                                        cUserAccName = JUU.lastname,
        //                                        uuserName = JUPUS.name,
        //                                        uuserLast = JUPUS.lastname,
        //                                        uUserAccName = JUPUS.lastname,
        //                                        agentCompany = JAA.company,
        //                                        */
        //                                        //username

        //                                        //  I.invoiceId,
        //                                        //    JBB.name
        //                                    }).ToList();

        //                    /*
        //          if(S.(I.discountType == "1" || I.discountType ==null ))
        //{
        //    return S.I.discountValue;
        //}else if(S.I.discountType == "2")
        //{
        //   return (S.I.discountValue / 100);
        //}
        //else
        //{
        //    return 0;
        //}
        //*/



        //                    if (invListm == null)
        //                        return NotFound();
        //                    else
        //                        return Ok(invListm);
        //                }

        //            }

        //            //else
        //            return NotFound();
        //        }



        // item quantity in location GetItemQtyInBranches()
        // عدد الفواتير في كل فرع
        //[HttpPost]
        //[Route("GetinvInBranch")]
        //public IHttpActionResult GetinvInBranch()
        //{
        //   
        //    
        //    string token = "";
        //    if (headers.Contains("APIKey"))
        //    {
        //        token = headers.GetValues("APIKey").First();
        //    }
        //    Validation validation = new Validation();
        //    bool valid = validation.CheckApiKey(token);

        //    if (valid) // APIKey is valid
        //    {
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            var invListm = (from B in entity.branches.Distinct()
        //                            from I in entity.invoices.Where(x => x.branchCreatorId == B.branchId).Distinct()
        //                                // from P in entity.pos.Where(x => x.posId == I.posId)


        //                                // from IT in entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Distinct()
        //                                // from JBB in JB.DefaultIfEmpty()


        //                                //    from JIMM in JIM.DefaultIfEmpty()

        //                                //  from JITT in JIT.DefaultIfEmpty()

        //                            where (I.invType == "p" || I.invType == "pb" || I.invType == "pd" || I.invType == "pbd")
        //                            group new { I, B } by (B.branchId) into g
        //                            select new
        //                            {


        //                                count = g.Select(S => S.I.invoiceId).Count(),

        //                                invType = g.Select(S => S.I.invType).FirstOrDefault(),
        //                                total = g.Sum(S => S.I.total),
        //                                totalNet = g.Sum(S => S.I.totalNet),
        //                                paid = g.Sum(S => S.I.paid),
        //                                deserved = g.Sum(S => S.I.deserved),
        //                                //
        //                                branchCreatorId = g.Select(S => S.I.branchCreatorId).FirstOrDefault(),
        //                                branchCreatorName = g.Select(S => S.B.name).FirstOrDefault(),
        //                                //

        //                                discountType = g.Select(S => S.I.discountType).FirstOrDefault(),
        //                                tax = g.Select(S => S.I.tax).FirstOrDefault(),
        //                                discountValue = g.Select(
        //                                    S => (S.I.discountType == "1" || S.I.discountType == null) ? S.I.discountValue
        //                                    : (S.I.discountType == "2" ? (S.I.discountValue / 100) : 0))
        //                                .FirstOrDefault(),

        //                                name = g.Select(S => S.B.name).FirstOrDefault(),
        //                                branchType = g.Select(S => S.B.type).FirstOrDefault(),
        //                                //posName = g.Select(S => S.P.name).FirstOrDefault(),
        //                                //  posCode = g.Select(S => S.P.code).FirstOrDefault(),


        //                                //  count



        //                            }).ToList();




        //            if (invListm == null)
        //                return NotFound();
        //            else
        //                return Ok(invListm);
        //        }

        //    }

        //    //else
        //    return NotFound();
        //}



        //  الفواتير بكل نقطة عددPOs
        // 
        //        [HttpPost]
        //        [Route("GetPoswithInvCount")]
        //        public IHttpActionResult GetPoswithInvCount()
        //        {
        //           
        //            
        //            string token = "";
        //            if (headers.Contains("APIKey"))
        //            {
        //                token = headers.GetValues("APIKey").First();
        //            }
        //            Validation validation = new Validation();
        //            bool valid = validation.CheckApiKey(token);

        //            if (valid) // APIKey is valid
        //            {
        //                using (incposdbEntities entity = new incposdbEntities())
        //                {
        //                    var invListm = (from I in entity.invoices
        //                                    join B in entity.branches on I.branchId equals B.branchId into JB
        //                                    join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
        //                                    join A in entity.agents on I.agentId equals A.agentId into JA
        //                                    join U in entity.users on I.createUserId equals U.userId into JU
        //                                    join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
        //                                    join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
        //                                    join P in entity.pos on I.posId equals P.posId into JP

        //                                    from JBB in JB.DefaultIfEmpty()
        //                                    from JPP in JP.DefaultIfEmpty()
        //                                    from JUU in JU.DefaultIfEmpty()
        //                                    from JUPUS in JUPUSR.DefaultIfEmpty()
        //                                    from JIMM in JIM.DefaultIfEmpty()
        //                                    from JAA in JA.DefaultIfEmpty()
        //                                    from JBCC in JBC.DefaultIfEmpty()

        //                                    where (I.invType == "p" || I.invType == "pb" || I.invType == "pd" || I.invType == "pbd")

        //                                    // &&  System.DateTime.Compare((DateTime)startDate,  I.invDate) <= 0 && System.DateTime.Compare((DateTime)endDate, I.invDate) >= 0
        //                                    group new { I, JBB, JPP, JUU, JUPUS, JIMM, JAA, JBCC } by (JPP.posId) into g
        //                                    select new
        //                                    {
        //                                        countP = g.Select(S => S.I.invoiceId).Count(),
        //                                        invoiceId = g.Select(S => S.I.invoiceId).FirstOrDefault(),
        //                                        invNumber = g.Select(S => S.I.invNumber).FirstOrDefault(),
        //                                        agentId = g.Select(S => S.I.agentId).FirstOrDefault(),
        //                                        //
        //                                        branchCreatorId = g.Select(S => S.I.branchCreatorId).FirstOrDefault(),
        //                                        branchCreatorName = g.Select(S => S.JBCC.name).FirstOrDefault(),
        //                                        //
        //                                        invType = g.Select(S => S.I.invType).FirstOrDefault(),
        //                                        total = g.Select(S => S.I.total).FirstOrDefault(),
        //                                        totalNet = g.Select(S => S.I.totalNet).FirstOrDefault(),
        //                                        paid = g.Select(S => S.I.paid).FirstOrDefault(),
        //                                        deserved = g.Select(S => S.I.deserved).FirstOrDefault(),
        //                                        deservedDate = g.Select(S => S.I.deservedDate).FirstOrDefault(),
        //                                        invDate = g.Select(S => S.I.invDate).FirstOrDefault(),
        //                                        invoiceMainId = g.Select(S => S.I.invoiceMainId).FirstOrDefault(),
        //                                        invCase = g.Select(S => S.I.invCase).FirstOrDefault(),
        //                                        invTime = g.Select(S => S.I.invTime).FirstOrDefault(),
        //                                        notes = g.Select(S => S.I.notes).FirstOrDefault(),
        //                                        vendorInvNum = g.Select(S => S.I.vendorInvNum).FirstOrDefault(),
        //                                        vendorInvDate = g.Select(S => S.I.vendorInvDate).FirstOrDefault(),
        //                                        createUserId = g.Select(S => S.I.createUserId).FirstOrDefault(),
        //                                        updateDate = g.Select(S => S.I.updateDate).FirstOrDefault(),
        //                                        updateUserId = g.Select(S => S.I.updateUserId).FirstOrDefault(),
        //                                        branchId = g.Select(S => S.I.branchId).FirstOrDefault(),
        //                                        discountValue = g.Select(
        //                                            S => (S.I.discountType == "1" || S.I.discountType == null) ? S.I.discountValue
        //                                            : (S.I.discountType == "2" ? (S.I.discountValue / 100) : 0))
        //                                        .FirstOrDefault(),
        //                                        discountType = g.Select(S => S.I.discountType).FirstOrDefault(),
        //                                        tax = g.Select(S => S.I.tax).FirstOrDefault(),
        //                                        name = g.Select(S => S.I.name).FirstOrDefault(),
        //                                        isApproved = g.Select(S => S.I.isApproved).FirstOrDefault(),
        //                                        branchName = g.Select(S => S.JBB.name).FirstOrDefault(),
        //                                        branchType = g.Select(S => S.JBB.type).FirstOrDefault(),
        //                                        posName = g.Select(S => S.JPP.name).FirstOrDefault(),
        //                                        posCode = g.Select(S => S.JPP.code).FirstOrDefault(),
        //                                        agentName = g.Select(S => S.JAA.name).FirstOrDefault(),
        //                                        agentCode = g.Select(S => S.JAA.code).FirstOrDefault(),
        //                                        agentType = g.Select(S => S.JAA.type).FirstOrDefault(),

        //                                        cuserName = g.Select(S => S.JUU.name).FirstOrDefault(),
        //                                        cuserLast = g.Select(S => S.JUU.lastname).FirstOrDefault(),
        //                                        cUserAccName = g.Select(S => S.JUU.username).FirstOrDefault(),
        //                                        uuserName = g.Select(S => S.JUPUS.name).FirstOrDefault(),
        //                                        uuserLast = g.Select(S => S.JUPUS.lastname).FirstOrDefault(),
        //                                        uUserAccName = g.Select(S => S.JUPUS.username).FirstOrDefault(),
        //                                        agentCompany = g.Select(S => S.JAA.company).FirstOrDefault(),
        //                                        posId = g.Select(S => S.JPP.posId).FirstOrDefault(),
        //                                        /*

        //                                        */
        //                                        //username

        //                                        //  I.invoiceId,
        //                                        //    JBB.name
        //                                    }).ToList();

        //                    /*
        //          if(S.(I.discountType == "1" || I.discountType ==null ))
        //{
        //    return S.I.discountValue;
        //}else if(S.I.discountType == "2")
        //{
        //   return (S.I.discountValue / 100);
        //}
        //else
        //{
        //    return 0;
        //}
        //*/



        //                    if (invListm == null)
        //                        return NotFound();
        //                    else
        //                        return Ok(invListm);
        //                }

        //            }

        //            //else
        //            return NotFound();
        //        }

        // الفواتير في كل نقطة

        //        [HttpPost]
        //        [Route("GetPoswithInv")]
        //        public IHttpActionResult GetPoswithInv()
        //        {
        //           
        //            
        //            string token = "";
        //            if (headers.Contains("APIKey"))
        //            {
        //                token = headers.GetValues("APIKey").First();
        //            }
        //            Validation validation = new Validation();
        //            bool valid = validation.CheckApiKey(token);

        //            if (valid) // APIKey is valid
        //            {
        //                using (incposdbEntities entity = new incposdbEntities())
        //                {
        //                    var invListm = (from I in entity.invoices
        //                                    join B in entity.branches on I.branchId equals B.branchId into JB
        //                                    join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
        //                                    join A in entity.agents on I.agentId equals A.agentId into JA
        //                                    join U in entity.users on I.createUserId equals U.userId into JU
        //                                    join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
        //                                    join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
        //                                    join P in entity.pos on I.posId equals P.posId into JP

        //                                    from JBB in JB.DefaultIfEmpty()
        //                                    from JPP in JP.DefaultIfEmpty()
        //                                    from JUU in JU.DefaultIfEmpty()
        //                                    from JUPUS in JUPUSR.DefaultIfEmpty()
        //                                    from JIMM in JIM.DefaultIfEmpty()
        //                                    from JAA in JA.DefaultIfEmpty()
        //                                    from JBCC in JBC.DefaultIfEmpty()

        //                                    where (I.invType == "p" || I.invType == "pb" || I.invType == "pd" || I.invType == "pbd")
        //                                    // (branchType == "all" ? true : JBB.type == branchType)
        //                                    //   && System.DateTime.Compare((DateTime)startDate, (DateTime)I.invDate) <= 0
        //                                    //  && System.DateTime.Compare((DateTime)endDate, (DateTime)I.invDate) >= 0
        //                                    // I.invType == invtype
        //                                    //     && branchType == "all" ? true : JBB.type == branchType

        //                                    //  && startDate <= I.invDate && endDate >= I.invDate
        //                                    // &&  System.DateTime.Compare((DateTime)startDate,  I.invDate) <= 0 && System.DateTime.Compare((DateTime)endDate, I.invDate) >= 0

        //                                    select new
        //                                    {

        //                                        I.invoiceId,
        //                                        I.invNumber,
        //                                        I.agentId,
        //                                        I.invType,
        //                                        I.total,
        //                                        I.totalNet,
        //                                        I.paid,
        //                                        I.deserved,
        //                                        I.deservedDate,
        //                                        I.invDate,
        //                                        I.invoiceMainId,
        //                                        I.invCase,
        //                                        I.invTime,
        //                                        I.notes,
        //                                        I.vendorInvNum,
        //                                        I.vendorInvDate,
        //                                        I.createUserId,
        //                                        I.updateDate,
        //                                        I.updateUserId,
        //                                        I.branchId,
        //                                        discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
        //                                        I.discountType,
        //                                        I.tax,
        //                                        I.name,
        //                                        I.isApproved,
        //                                        //
        //                                        I.branchCreatorId,
        //                                        brancCreatorhName = JBCC.name,
        //                                        //
        //                                        branchName = JBB.name,
        //                                        branchType = JBB.type,
        //                                        posName = JPP.name,
        //                                        posCode = JPP.code,
        //                                        agentName = JAA.name,
        //                                        agentCode = JAA.code,
        //                                        agentType = JAA.type,
        //                                        cuserName = JUU.name,
        //                                        cuserLast = JUU.lastname,
        //                                        cUserAccName = JUU.username,
        //                                        uuserName = JUPUS.name,
        //                                        uuserLast = JUPUS.lastname,
        //                                        uUserAccName = JUPUS.username,
        //                                        agentCompany = JAA.company,
        //                                        JPP.posId,


        //                                        //username

        //                                        //  I.invoiceId,
        //                                        //    JBB.name
        //                                    }).ToList();

        //                    /*
        //          if(S.(I.discountType == "1" || I.discountType ==null ))
        //{
        //    return S.I.discountValue;
        //}else if(S.I.discountType == "2")
        //{
        //   return (S.I.discountValue / 100);
        //}
        //else
        //{
        //    return 0;
        //}
        //*/



        //                    if (invListm == null)
        //                        return NotFound();
        //                    else
        //                        return Ok(invListm);
        //                }

        //            }

        //            //else
        //            return NotFound();
        //        }


        // عدد فواتير المشتريات ومرتجع المشتريات ومسودات كل فرع
        //        [HttpPost]
        //        [Route("GetinvCountByBranch")]
        //        public IHttpActionResult GetinvCountByBranch()
        //        {
        //           
        //            
        //            string token = "";
        //            if (headers.Contains("APIKey"))
        //            {
        //                token = headers.GetValues("APIKey").First();
        //            }
        //            Validation validation = new Validation();
        //            bool valid = validation.CheckApiKey(token);

        //            if (valid) // APIKey is valid
        //            {
        //                using (incposdbEntities entity = new incposdbEntities())
        //                {
        //                    var invListm = (from I in entity.invoices
        //                                    join B in entity.branches on I.branchId equals B.branchId into JB
        //                                    join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
        //                                    from JBB in JB.DefaultIfEmpty()
        //                                    from JBCC in JBC.DefaultIfEmpty()
        //                                    where (JBB.branchId != 1)
        //                                 && (I.invType == "p" || I.invType == "pb" || I.invType == "pd" || I.invType == "pbd")

        //                                    // &&  System.DateTime.Compare((DateTime)startDate,  I.invDate) <= 0 && System.DateTime.Compare((DateTime)endDate, I.invDate) >= 0
        //                                    group new { I, JBB, JBCC } by (I.branchCreatorId) into g
        //                                    select new
        //                                    {
        //                                        branchCreatorId = g.Key,
        //                                        branchCreatorName = g.Select(t => t.JBCC.name).FirstOrDefault(),

        //                                        countP = g.Where(t => t.I.invType == "p").Count(),
        //                                        countPb = g.Where(t => t.I.invType == "pb").Count(),
        //                                        //  countS = g.Where(t => t.I.invType == "s").Count(),
        //                                        countD = g.Where(t => t.I.invType == "pd" || t.I.invType == "pbd").Count(),
        //                                        //  totalS = g.Where(t => t.I.invType == "s").Sum(S => S.I.total),
        //                                        //totalNetS = g.Where(t => t.I.invType == "s").Sum(S => S.I.totalNet),
        //                                        /*
        //                                        totalP = g.Where(t => t.I.invType == "p").Sum(S => S.I.total),
        //                                        totalPb = g.Where(t => t.I.invType == "pb").Sum(S => S.I.total),
        //                                        totalD = g.Where(t => t.I.invType == "pd" || t.I.invType == "pbd").Sum(S => S.I.total),

        //                                        totalNetP = g.Where(t => t.I.invType == "p").Sum(S => S.I.totalNet),
        //                                        totalNetPb = g.Where(t => t.I.invType == "pb").Sum(S => S.I.totalNet),
        //                                        totalNetD = g.Where(t => t.I.invType == "pd" || t.I.invType == "pbd").Sum(S => S.I.totalNet),

        //                                        paid = g.Where(t => t.I.invType == "p").Sum(S => S.I.paid),
        //                                        deserved = g.Where(t => t.I.invType == "p").Sum(S => S.I.deserved),
        //                                        discountValue = g.Where(t => t.I.invType == "p").Sum(S => S.(I.discountType == "1" || I.discountType ==null ) ? S.I.discountValue : (S.I.discountType == "2" ? (S.I.discountValue / 100) : 0)),

        //                                        paidPb = g.Where(t => t.I.invType == "pb").Sum(S => S.I.paid),
        //                                        deservedPb = g.Where(t => t.I.invType == "pb").Sum(S => S.I.deserved),
        //                                        discountValuePb = g.Where(t => t.I.invType == "pb").Sum(S => S.(I.discountType == "1" || I.discountType ==null ) ? S.I.discountValue : (S.I.discountType == "2" ? (S.I.discountValue / 100) : 0)),
        //                                           */
        //                                    }).ToList();

        //                    /*
        //          if(S.(I.discountType == "1" || I.discountType ==null ))
        //{
        //    return S.I.discountValue;
        //}else if(S.I.discountType == "2")
        //{
        //   return (S.I.discountValue / 100);
        //}
        //else
        //{
        //    return 0;
        //}
        //*/



        //                    if (invListm == null)
        //                        return NotFound();
        //                    else
        //                        return Ok(invListm);
        //                }

        //            }

        //            //else
        //            return NotFound();
        //        }


        /*
        //  فواتير المشتريات ومرتجع المشتريات ومسودات كل فرع
        [HttpPost]
        [Route("GetinvTypeByBranch")]
        public IHttpActionResult GetinvTypeByBranch()
        {
           
            
            string token = "";
            if (headers.Contains("APIKey"))
            {
                token = headers.GetValues("APIKey").First();
            }
            Validation validation = new Validation();
            bool valid = validation.CheckApiKey(token);

            if (valid) // APIKey is valid
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var invListm = (from I in entity.invoices
                                    join B in entity.branches on I.branchId equals B.branchId into JB
                                    from JBB in JB.DefaultIfEmpty()
                                    where (JBB.branchId != 1)
                                 && (I.invType == "p" || I.invType == "pb" || I.invType == "pd" || I.invType == "pbd")

                                    // &&  System.DateTime.Compare((DateTime)startDate,  I.invDate) <= 0 && System.DateTime.Compare((DateTime)endDate, I.invDate) >= 0
                                   
                                    select new
                                    {
                                        branchId = g.Key,
                                        branchName = g.Select(t => t.JBB.name).FirstOrDefault(),

                                        countP = g.Where(t => t.I.invType == "p").Count(),
                                        countPb = g.Where(t => t.I.invType == "pb").Count(),
                                        //  countS = g.Where(t => t.I.invType == "s").Count(),
                                        countD = g.Where(t => t.I.invType == "pd" || t.I.invType == "pbd").Count(),
                                        //  totalS = g.Where(t => t.I.invType == "s").Sum(S => S.I.total),
                                        //totalNetS = g.Where(t => t.I.invType == "s").Sum(S => S.I.totalNet),
                                      
                                        totalP = g.Where(t => t.I.invType == "p").Sum(S => S.I.total),
                                        totalPb = g.Where(t => t.I.invType == "pb").Sum(S => S.I.total),
                                        totalD = g.Where(t => t.I.invType == "pd" || t.I.invType == "pbd").Sum(S => S.I.total),
                                       
                                        totalNetP = g.Where(t => t.I.invType == "p").Sum(S => S.I.totalNet),
                                        totalNetPb = g.Where(t => t.I.invType == "pb").Sum(S => S.I.totalNet),
                                        totalNetD = g.Where(t => t.I.invType == "pd" || t.I.invType == "pbd").Sum(S => S.I.totalNet),
                                      
                                        paid = g.Where(t => t.I.invType == "p").Sum(S => S.I.paid),
                                        deserved = g.Where(t => t.I.invType == "p").Sum(S => S.I.deserved),
                                        discountValue = g.Where(t => t.I.invType == "p").Sum(S => S.(I.discountType == "1" || I.discountType ==null ) ? S.I.discountValue : (S.I.discountType == "2" ? (S.I.discountValue / 100) : 0)),
                                       /*
                                        paidPb = g.Where(t => t.I.invType == "pb").Sum(S => S.I.paid),
                                        deservedPb = g.Where(t => t.I.invType == "pb").Sum(S => S.I.deserved),
                                        discountValuePb = g.Where(t => t.I.invType == "pb").Sum(S => S.(I.discountType == "1" || I.discountType ==null ) ? S.I.discountValue : (S.I.discountType == "2" ? (S.I.discountValue / 100) : 0)),
                                         
                                    }).ToList();

                    /*
          if(S.(I.discountType == "1" || I.discountType ==null ))
{
    return S.I.discountValue;
}else if(S.I.discountType == "2")
{
   return (S.I.discountValue / 100);
}
else
{
    return 0;
}




                    if (invListm == null)
                        return NotFound();
                    else
                        return Ok(invListm);
                }

            }

            //else
            return NotFound();
        }
        */
        // getinv in branch
        // المبيعات
        #region sales
        //  فواتير المبيعات بكل انواعها بكل فرع
        //[HttpPost]
        //[Route("GetSaleinv")]
        //public IHttpActionResult GetSaleinv()
        //{
        //   
        //    
        //    string token = "";
        //    if (headers.Contains("APIKey"))
        //    {
        //        token = headers.GetValues("APIKey").First();
        //    }
        //    Validation validation = new Validation();
        //    bool valid = validation.CheckApiKey(token);

        //    if (valid) // APIKey is valid
        //    {
        //        using (incposdbEntities entity = new incposdbEntities())
        //        {
        //            var invListm = (from I in entity.invoices
        //                                //   join B in entity.branches on I.branchId equals B.branchId into JB
        //                            join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
        //                            join A in entity.agents on I.agentId equals A.agentId into JA
        //                            join U in entity.users on I.createUserId equals U.userId into JU
        //                            join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
        //                            join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
        //                            join P in entity.pos on I.posId equals P.posId into JP

        //                            //   from JBB in JB
        //                            from JPP in JP.DefaultIfEmpty()
        //                            from JUU in JU.DefaultIfEmpty()
        //                            from JUPUS in JUPUSR.DefaultIfEmpty()
        //                            from JIMM in JIM.DefaultIfEmpty()
        //                            from JAA in JA.DefaultIfEmpty()
        //                            from JBCC in JBC.DefaultIfEmpty()
        //                            where (I.invType == "s" || I.invType == "sb" || I.invType == "sd" || I.invType == "sbd")

        //                            select new
        //                            {
        //                                I.invoiceId,
        //                                I.invNumber,
        //                                I.agentId,
        //                                I.posId,
        //                                I.invType,
        //                                I.total,
        //                                I.totalNet,
        //                                I.paid,
        //                                I.deserved,
        //                                I.deservedDate,
        //                                I.invDate,
        //                                I.invoiceMainId,
        //                                I.invCase,
        //                                I.invTime,
        //                                I.notes,
        //                                I.vendorInvNum,
        //                                I.vendorInvDate,
        //                                I.createUserId,
        //                                I.updateDate,
        //                                I.updateUserId,
        //                                I.branchId,
        //                                discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
        //                                I.discountType,
        //                                I.tax,
        //                                I.name,
        //                                I.isApproved,
        //                                //
        //                                I.branchCreatorId,
        //                                branchCreatorName = JBCC.name,
        //                                //
        //                                // branchName = JBB.name,

        //                                //  branchType = JBB.type,
        //                                posName = JPP.name,
        //                                posCode = JPP.code,
        //                                agentName = JAA.name,
        //                                agentCode = JAA.code,
        //                                agentType = JAA.type,
        //                                cuserName = JUU.name,
        //                                cuserLast = JUU.lastname,
        //                                cUserAccName = JUU.username,
        //                                uuserName = JUPUS.name,
        //                                uuserLast = JUPUS.lastname,
        //                                uUserAccName = JUPUS.username,
        //                                agentCompany = JAA.company,

        //                            }).ToList();



        //            if (invListm == null)
        //                return NotFound();
        //            else
        //                return Ok(invListm);
        //        }

        //    }

        //    //else
        //    return NotFound();
        //}


        // فواتير المبيعات مع العناصر
        [HttpPost]
        [Route("GetSaleitem")]
        public string GetSaleitem(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                        join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                        join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        //    join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        // from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" ||  I.invType == "ts" || I.invType == "ss")

                                        select new
                                        {

                                            ITitemName = ITEM.name,
                                            ITunitName = UNIT.name,
                                            ITitemsTransId = IT.itemsTransId,
                                            ITitemUnitId = IT.itemUnitId,

                                            ITitemId = IU.itemId,
                                            ITunitId = IU.unitId,
                                            ITquantity = IT.quantity,

                                            ITcreateDate = IT.createDate,
                                            ITupdateDate = IT.updateDate,
                                            ITcreateUserId = IT.createUserId,
                                            ITupdateUserId = IT.updateUserId,
                                            ITnotes = IT.notes,
                                            ITprice = IT.price,
                                            ITbarcode = IU.barcode,
                                            ITCreateuserName = ITCUSER.name,
                                            ITCreateuserLName = ITCUSER.lastname,
                                            ITCreateuserAccName = ITCUSER.username,

                                            ITUpdateuserName = ITUPUSER.name,
                                            ITUpdateuserLName = ITUPUSER.lastname,
                                            ITUpdateuserAccName = ITUPUSER.username,
                                            I.invoiceId,
                                            I.invNumber,
                                         I.invBarcode,
                                            I.agentId,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            //  branchName = JBB.name,

                                            //  branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,
                                            agentName = JAA.name,
                                            agentCode = JAA.code,
                                            agentType = JAA.type,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,
                                            subTotal = (IT.price * IT.quantity),
                                            categoryName= ITEM.categories.name,
                                            categoryId = ITEM.categoryId,
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();





                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            //var re = Request;
            //
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    List<int> brIds = AllowedBranchsId(mainBranchId, userId);

            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var invListm = (from IT in entity.itemsTransfer
            //                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

            //                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
            //                        join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
            //                        join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
            //                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
            //                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
            //                        //    join B in entity.branches on I.branchId equals B.branchId into JB
            //                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
            //                        join A in entity.agents on I.agentId equals A.agentId into JA
            //                        join U in entity.users on I.createUserId equals U.userId into JU
            //                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
            //                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
            //                        join P in entity.pos on I.posId equals P.posId into JP

            //                        // from JBB in JB
            //                        from JPP in JP.DefaultIfEmpty()
            //                        from JUU in JU.DefaultIfEmpty()
            //                        from JUPUS in JUPUSR.DefaultIfEmpty()
            //                        from JIMM in JIM.DefaultIfEmpty()
            //                        from JAA in JA.DefaultIfEmpty()
            //                        from JBCC in JBC.DefaultIfEmpty()
            //                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "sb")

            //                        select new
            //                        {

            //                            ITitemName = ITEM.name,
            //                            ITunitName = UNIT.name,
            //                            ITitemsTransId = IT.itemsTransId,
            //                            ITitemUnitId = IT.itemUnitId,

            //                            ITitemId = IU.itemId,
            //                            ITunitId = IU.unitId,
            //                            ITquantity = IT.quantity,

            //                            ITcreateDate = IT.createDate,
            //                            ITupdateDate = IT.updateDate,
            //                            ITcreateUserId = IT.createUserId,
            //                            ITupdateUserId = IT.updateUserId,
            //                            ITnotes = IT.notes,
            //                            ITprice = IT.price,
            //                            ITbarcode = IU.barcode,
            //                            ITCreateuserName = ITCUSER.name,
            //                            ITCreateuserLName = ITCUSER.lastname,
            //                            ITCreateuserAccName = ITCUSER.username,

            //                            ITUpdateuserName = ITUPUSER.name,
            //                            ITUpdateuserLName = ITUPUSER.lastname,
            //                            ITUpdateuserAccName = ITUPUSER.username,
            //                            I.invoiceId,
            //                            I.invNumber,
            //                            I.agentId,
            //                            I.posId,
            //                            I.invType,
            //                            I.total,
            //                            I.totalNet,
            //                            I.paid,
            //                            I.deserved,
            //                            I.deservedDate,
            //                            I.invDate,
            //                            I.invoiceMainId,
            //                            I.invCase,
            //                            I.invTime,
            //                            I.notes,
            //                            I.vendorInvNum,
            //                            I.vendorInvDate,
            //                            I.createUserId,
            //                            I.updateDate,
            //                            I.updateUserId,
            //                            I.branchId,
            //                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
            //                            I.discountType,
            //                            I.tax,
            //                            I.name,
            //                            I.isApproved,

            //                            //
            //                            I.branchCreatorId,
            //                            branchCreatorName = JBCC.name,
            //                            //
            //                            //  branchName = JBB.name,

            //                            //  branchType = JBB.type,
            //                            posName = JPP.name,
            //                            posCode = JPP.code,
            //                            agentName = JAA.name,
            //                            agentCode = JAA.code,
            //                            agentType = JAA.type,
            //                            cuserName = JUU.name,
            //                            cuserLast = JUU.lastname,
            //                            cUserAccName = JUU.username,
            //                            uuserName = JUPUS.name,
            //                            uuserLast = JUPUS.lastname,
            //                            uUserAccName = JUPUS.username,
            //                            agentCompany = JAA.company,
            //                            subTotal = (IT.price * IT.quantity),
            //                            //username

            //                            //  I.invoiceId,
            //                            //    JBB.name
            //                        }).ToList();





            //        if (invListm == null)
            //            return NotFound();
            //        else
            //            return Ok(invListm);
            //    }

            //}

            ////else
            //return NotFound();
        }

        //عدد العناصر في كل فاتورة
        [HttpPost]
        [Route("GetSaleitemcount")]
        public string GetSaleitemcount(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" ||  I.invType == "ts" || I.invType == "ss")

                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                         I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                       //     discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,
                                            discountValue=(I.couponsInvoices.Where(X=>X.InvoiceId==I.invoiceId).ToList().Count())==0?0:
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            // branchName = JBB.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            agentCode = JAA.code,
                                            //
                                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                            "unknown" : JAA.name,


                                            //   agentType = JAA.type,
                                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                            ? "c" : JAA.type,
                                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                            ? 0 : I.agentId,


                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                            "unknown" : JAA.company,





                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }

            }

            //var re = Request;
            //
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    List<int> brIds = AllowedBranchsId(mainBranchId, userId);

            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var invListm = (from I in entity.invoices

            //                            //  join B in entity.branches on I.branchId equals B.branchId into JB
            //                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
            //                        join A in entity.agents on I.agentId equals A.agentId into JA
            //                        join U in entity.users on I.createUserId equals U.userId into JU
            //                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
            //                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
            //                        join P in entity.pos on I.posId equals P.posId into JP

            //                        //   from JBB in JB
            //                        from JPP in JP.DefaultIfEmpty()
            //                        from JUU in JU.DefaultIfEmpty()
            //                        from JUPUS in JUPUSR.DefaultIfEmpty()
            //                        from JIMM in JIM.DefaultIfEmpty()
            //                        from JAA in JA.DefaultIfEmpty()
            //                        from JBCC in JBC.DefaultIfEmpty()
            //                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "sb")

            //                        select new
            //                        {

            //                            I.invoiceId,
            //                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
            //                            I.invNumber,

            //                            I.posId,
            //                            I.invType,
            //                            I.total,
            //                            I.totalNet,
            //                            I.paid,
            //                            I.deserved,
            //                            I.deservedDate,
            //                            I.invDate,
            //                            I.invoiceMainId,
            //                            I.invCase,
            //                            I.invTime,
            //                            I.notes,
            //                            I.vendorInvNum,
            //                            I.vendorInvDate,
            //                            I.createUserId,
            //                            I.updateDate,
            //                            I.updateUserId,
            //                            I.branchId,
            //                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
            //                            I.discountType,
            //                            I.tax,
            //                            I.name,
            //                            I.isApproved,

            //                            //
            //                            I.branchCreatorId,
            //                            branchCreatorName = JBCC.name,
            //                            //
            //                            // branchName = JBB.name,

            //                            //     branchType = JBB.type,
            //                            posName = JPP.name,
            //                            posCode = JPP.code,

            //                            agentCode = JAA.code,
            //                            //
            //                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb")) ?
            //                            "unknown" : JAA.name,


            //                            //   agentType = JAA.type,
            //                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
            //                            ? "c" : JAA.type,
            //                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
            //                            ? 0 : I.agentId,


            //                            cuserName = JUU.name,
            //                            cuserLast = JUU.lastname,
            //                            cUserAccName = JUU.username,
            //                            uuserName = JUPUS.name,
            //                            uuserLast = JUPUS.lastname,
            //                            uUserAccName = JUPUS.username,
            //                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb")) ?
            //                            "unknown" : JAA.company,





            //                            //username

            //                            //  I.invoiceId,
            //                            //    JBB.name
            //                        }).ToList();


            //        if (invListm == null)
            //            return NotFound();
            //        else
            //            return Ok(invListm);
            //    }

            //}

            ////else
            //return NotFound();
        }


        // عدد العناصر في فواتير الطلبات

        [HttpPost]
        [Route("Getorderitemcount")]
        public string Getorderitemcount(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "or")

                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                            I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            // branchName = JBB.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            agentCode = JAA.code,
                                            //
                                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss" || I.invType == "ts" || I.invType == "ss")) ?
                                            "unknown" : JAA.name,


                                            //   agentType = JAA.type,
                                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                            ? "c" : JAA.type,
                                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                            ? 0 : I.agentId,


                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                            "unknown" : JAA.company,





                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }
            //var re = Request;
            //
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    List<int> brIds = AllowedBranchsId(mainBranchId, userId);

            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var invListm = (from I in entity.invoices

            //                            //  join B in entity.branches on I.branchId equals B.branchId into JB
            //                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
            //                        join A in entity.agents on I.agentId equals A.agentId into JA
            //                        join U in entity.users on I.createUserId equals U.userId into JU
            //                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
            //                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
            //                        join P in entity.pos on I.posId equals P.posId into JP

            //                        //   from JBB in JB
            //                        from JPP in JP.DefaultIfEmpty()
            //                        from JUU in JU.DefaultIfEmpty()
            //                        from JUPUS in JUPUSR.DefaultIfEmpty()
            //                        from JIMM in JIM.DefaultIfEmpty()
            //                        from JAA in JA.DefaultIfEmpty()
            //                        from JBCC in JBC.DefaultIfEmpty()
            //                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "or")

            //                        select new
            //                        {

            //                            I.invoiceId,
            //                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
            //                            I.invNumber,

            //                            I.posId,
            //                            I.invType,
            //                            I.total,
            //                            I.totalNet,
            //                            I.paid,
            //                            I.deserved,
            //                            I.deservedDate,
            //                            I.invDate,
            //                            I.invoiceMainId,
            //                            I.invCase,
            //                            I.invTime,
            //                            I.notes,
            //                            I.vendorInvNum,
            //                            I.vendorInvDate,
            //                            I.createUserId,
            //                            I.updateDate,
            //                            I.updateUserId,
            //                            I.branchId,
            //                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
            //                            I.discountType,
            //                            I.tax,
            //                            I.name,
            //                            I.isApproved,

            //                            //
            //                            I.branchCreatorId,
            //                            branchCreatorName = JBCC.name,
            //                            //
            //                            // branchName = JBB.name,

            //                            //     branchType = JBB.type,
            //                            posName = JPP.name,
            //                            posCode = JPP.code,

            //                            agentCode = JAA.code,
            //                            //
            //                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb")) ?
            //                            "unknown" : JAA.name,


            //                            //   agentType = JAA.type,
            //                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
            //                            ? "c" : JAA.type,
            //                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
            //                            ? 0 : I.agentId,


            //                            cuserName = JUU.name,
            //                            cuserLast = JUU.lastname,
            //                            cUserAccName = JUU.username,
            //                            uuserName = JUPUS.name,
            //                            uuserLast = JUPUS.lastname,
            //                            uUserAccName = JUPUS.username,
            //                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb")) ?
            //                            "unknown" : JAA.company,





            //                            //username

            //                            //  I.invoiceId,
            //                            //    JBB.name
            //                        }).ToList();


            //        if (invListm == null)
            //            return NotFound();
            //        else
            //            return Ok(invListm);
            //    }

            //}

            ////else
            //return NotFound();
        }

        // عدد العناصر في فواتير الطلبات

        [HttpPost]
        [Route("GetPurorderitemcount")]
        public string GetPurorderitemcount(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //  join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "po")

                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                          I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            // branchName = JBB.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            agentCode = JAA.code,
                                            //
                                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                            "unknown" : JAA.name,


                                            //   agentType = JAA.type,
                                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                            ? "c" : JAA.type,
                                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                            ? 0 : I.agentId,


                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                            "unknown" : JAA.company,





                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            //var re = Request;
            //
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    List<int> brIds = AllowedBranchsId(mainBranchId, userId);

            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var invListm = (from I in entity.invoices

            //                            //  join B in entity.branches on I.branchId equals B.branchId into JB
            //                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
            //                        join A in entity.agents on I.agentId equals A.agentId into JA
            //                        join U in entity.users on I.createUserId equals U.userId into JU
            //                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
            //                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
            //                        join P in entity.pos on I.posId equals P.posId into JP

            //                        //   from JBB in JB
            //                        from JPP in JP.DefaultIfEmpty()
            //                        from JUU in JU.DefaultIfEmpty()
            //                        from JUPUS in JUPUSR.DefaultIfEmpty()
            //                        from JIMM in JIM.DefaultIfEmpty()
            //                        from JAA in JA.DefaultIfEmpty()
            //                        from JBCC in JBC.DefaultIfEmpty()
            //                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "po")

            //                        select new
            //                        {

            //                            I.invoiceId,
            //                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
            //                            I.invNumber,

            //                            I.posId,
            //                            I.invType,
            //                            I.total,
            //                            I.totalNet,
            //                            I.paid,
            //                            I.deserved,
            //                            I.deservedDate,
            //                            I.invDate,
            //                            I.invoiceMainId,
            //                            I.invCase,
            //                            I.invTime,
            //                            I.notes,
            //                            I.vendorInvNum,
            //                            I.vendorInvDate,
            //                            I.createUserId,
            //                            I.updateDate,
            //                            I.updateUserId,
            //                            I.branchId,
            //                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
            //                            I.discountType,
            //                            I.tax,
            //                            I.name,
            //                            I.isApproved,

            //                            //
            //                            I.branchCreatorId,
            //                            branchCreatorName = JBCC.name,
            //                            //
            //                            // branchName = JBB.name,

            //                            //     branchType = JBB.type,
            //                            posName = JPP.name,
            //                            posCode = JPP.code,

            //                            agentCode = JAA.code,
            //                            //
            //                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb")) ?
            //                            "unknown" : JAA.name,


            //                            //   agentType = JAA.type,
            //                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
            //                            ? "c" : JAA.type,
            //                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
            //                            ? 0 : I.agentId,


            //                            cuserName = JUU.name,
            //                            cuserLast = JUU.lastname,
            //                            cUserAccName = JUU.username,
            //                            uuserName = JUPUS.name,
            //                            uuserLast = JUPUS.lastname,
            //                            uUserAccName = JUPUS.username,
            //                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb")) ?
            //                            "unknown" : JAA.company,





            //                            //username

            //                            //  I.invoiceId,
            //                            //    JBB.name
            //                        }).ToList();


            //        if (invListm == null)
            //            return NotFound();
            //        else
            //            return Ok(invListm);
            //    }

            //}

            ////else
            //return NotFound();
        }



        // عدد العناصر في فواتير عرض السعر


        [HttpPost]
        [Route("GetQtitemcount")]
        public string GetQtitemcount(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //  join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "q")

                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                             I.invBarcode,                                          
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            // branchName = JBB.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            agentCode = JAA.code,
                                            //
                                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                            "unknown" : JAA.name,


                                            //   agentType = JAA.type,
                                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                            ? "c" : JAA.type,
                                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                            ? 0 : I.agentId,


                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                            "unknown" : JAA.company,





                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            //var re = Request;
            //
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    List<int> brIds = AllowedBranchsId(mainBranchId, userId);

            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var invListm = (from I in entity.invoices

            //                            //  join B in entity.branches on I.branchId equals B.branchId into JB
            //                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
            //                        join A in entity.agents on I.agentId equals A.agentId into JA
            //                        join U in entity.users on I.createUserId equals U.userId into JU
            //                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
            //                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
            //                        join P in entity.pos on I.posId equals P.posId into JP

            //                        //   from JBB in JB
            //                        from JPP in JP.DefaultIfEmpty()
            //                        from JUU in JU.DefaultIfEmpty()
            //                        from JUPUS in JUPUSR.DefaultIfEmpty()
            //                        from JIMM in JIM.DefaultIfEmpty()
            //                        from JAA in JA.DefaultIfEmpty()
            //                        from JBCC in JBC.DefaultIfEmpty()
            //                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "q")

            //                        select new
            //                        {

            //                            I.invoiceId,
            //                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
            //                            I.invNumber,

            //                            I.posId,
            //                            I.invType,
            //                            I.total,
            //                            I.totalNet,
            //                            I.paid,
            //                            I.deserved,
            //                            I.deservedDate,
            //                            I.invDate,
            //                            I.invoiceMainId,
            //                            I.invCase,
            //                            I.invTime,
            //                            I.notes,
            //                            I.vendorInvNum,
            //                            I.vendorInvDate,
            //                            I.createUserId,
            //                            I.updateDate,
            //                            I.updateUserId,
            //                            I.branchId,
            //                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
            //                            I.discountType,
            //                            I.tax,
            //                            I.name,
            //                            I.isApproved,

            //                            //
            //                            I.branchCreatorId,
            //                            branchCreatorName = JBCC.name,
            //                            //
            //                            // branchName = JBB.name,

            //                            //     branchType = JBB.type,
            //                            posName = JPP.name,
            //                            posCode = JPP.code,

            //                            agentCode = JAA.code,
            //                            //
            //                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb")) ?
            //                            "unknown" : JAA.name,


            //                            //   agentType = JAA.type,
            //                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
            //                            ? "c" : JAA.type,
            //                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
            //                            ? 0 : I.agentId,


            //                            cuserName = JUU.name,
            //                            cuserLast = JUU.lastname,
            //                            cUserAccName = JUU.username,
            //                            uuserName = JUPUS.name,
            //                            uuserLast = JUPUS.lastname,
            //                            uUserAccName = JUPUS.username,
            //                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb")) ?
            //                            "unknown" : JAA.company,





            //                            //username

            //                            //  I.invoiceId,
            //                            //    JBB.name
            //                        }).ToList();


            //        if (invListm == null)
            //            return NotFound();
            //        else
            //            return Ok(invListm);
            //    }

            //}

            ////else
            //return NotFound();
        }


        //
        //الفواتير التي فيها كوبون
        //  join IC in entity.couponsInvoices on I.invoiceId equals IC.InvoiceId 
        //  join C in entity.coupons on IC.couponId equals C.cId
        //  CopName=   C.name,
        //   CopId=   C.cId,
        //CopCode=   C.code,

        //GetSalecoupon

        [HttpPost]
        [Route("GetSalecoupon")]
        public string GetSalecoupon(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //  join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP
                                        join IC in entity.couponsInvoices on I.invoiceId equals IC.InvoiceId
                                        join C in entity.coupons on IC.couponId equals C.cId
                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")
                                        select new
                                        {
                                            //coupon
                                            Copname = C.name,//*
                                            CopcId = C.cId,
                                            Copcode = C.code,//*

                                            CopisActive = C.isActive,
                                            CopdiscountType = C.discountType,//*
                                            CopdiscountValue = C.discountValue,//*
                                            CopstartDate = C.startDate,
                                            CopendDate = C.endDate,
                                            //Copnotes = C.notes,
                                            Copquantity = C.quantity,
                                            //CopremainQ = C.remainQ,
                                            //CopinvMin = C.invMin,
                                            //CopinvMax = C.invMax,
                                            CopcreateDate = C.createDate,
                                            CopupdateDate = C.updateDate,
                                            CopcreateUserId = C.createUserId,
                                            CopupdateUserId = C.updateUserId,
                                            //Copbarcode = C.barcode,

                                            I.invoiceId,
                                            I.invNumber,//*
                                            I.invBarcode,
                                            I.agentId,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            //I.notes,
                                            //I.vendorInvNum,
                                            //I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            //I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                            couponTotalValue = (C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue),

                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            // discountValue = (I.discountType == "1" || I.discountType ==null ) ? I.discountValue : (I.discountType == "2" ? ((I.discountValue / 100) * I.total) : 0),

                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            //  branchName = JBB.name,

                                            //  branchType = JBB.type,
                                            //posName = JPP.name,
                                            //posCode = JPP.code,
                                            //agentName = JAA.name,
                                            //agentCode = JAA.code,
                                            //agentType = JAA.type,
                                            //cuserName = JUU.name,
                                            //cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            //uuserName = JUPUS.name,
                                            //uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            //agentCompany = JAA.company,

                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();


                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


            //var re = Request;
            //
            //string token = "";
            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}
            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid) // APIKey is valid
            //{
            //    List<int> brIds = AllowedBranchsId(mainBranchId, userId);

            //    using (incposdbEntities entity = new incposdbEntities())
            //    {
            //        var invListm = (from I in entity.invoices

            //                            //  join B in entity.branches on I.branchId equals B.branchId into JB
            //                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
            //                        join A in entity.agents on I.agentId equals A.agentId into JA
            //                        join U in entity.users on I.createUserId equals U.userId into JU
            //                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
            //                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
            //                        join P in entity.pos on I.posId equals P.posId into JP
            //                        join IC in entity.couponsInvoices on I.invoiceId equals IC.InvoiceId
            //                        join C in entity.coupons on IC.couponId equals C.cId
            //                        //   from JBB in JB
            //                        from JPP in JP.DefaultIfEmpty()
            //                        from JUU in JU.DefaultIfEmpty()
            //                        from JUPUS in JUPUSR.DefaultIfEmpty()
            //                        from JIMM in JIM.DefaultIfEmpty()
            //                        from JAA in JA.DefaultIfEmpty()
            //                        from JBCC in JBC.DefaultIfEmpty()
            //                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "sb")
            //                        select new
            //                        {

            //                            //coupon
            //                            Copname = C.name,
            //                            CopcId = C.cId,
            //                            Copcode = C.code,

            //                            CopisActive = C.isActive,
            //                            CopdiscountType = C.discountType,
            //                            CopdiscountValue = C.discountValue,
            //                            CopstartDate = C.startDate,
            //                            CopendDate = C.endDate,
            //                            Copnotes = C.notes,
            //                            Copquantity = C.quantity,
            //                            CopremainQ = C.remainQ,
            //                            CopinvMin = C.invMin,
            //                            CopinvMax = C.invMax,
            //                            CopcreateDate = C.createDate,
            //                            CopupdateDate = C.updateDate,
            //                            CopcreateUserId = C.createUserId,
            //                            CopupdateUserId = C.updateUserId,
            //                            Copbarcode = C.barcode,

            //                            I.invoiceId,
            //                            I.invNumber,
            //                            I.agentId,
            //                            I.posId,
            //                            I.invType,
            //                            I.total,
            //                            I.totalNet,
            //                            I.paid,
            //                            I.deserved,
            //                            I.deservedDate,
            //                            I.invDate,
            //                            I.invoiceMainId,
            //                            I.invCase,
            //                            I.invTime,
            //                            I.notes,
            //                            I.vendorInvNum,
            //                            I.vendorInvDate,
            //                            I.createUserId,
            //                            I.updateDate,
            //                            I.updateUserId,
            //                            I.branchId,
            //                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
            //                            couponTotalValue = (C.discountType == 1 || C.discountType == null) ? C.discountValue : (C.discountType == 2 ? ((C.discountValue / 100) * I.total) : 0),

            //                            I.discountType,
            //                            I.tax,
            //                            I.name,
            //                            I.isApproved,

            //                            // discountValue = (I.discountType == "1" || I.discountType ==null ) ? I.discountValue : (I.discountType == "2" ? ((I.discountValue / 100) * I.total) : 0),

            //                            I.branchCreatorId,
            //                            branchCreatorName = JBCC.name,
            //                            //
            //                            //  branchName = JBB.name,

            //                            //  branchType = JBB.type,
            //                            posName = JPP.name,
            //                            posCode = JPP.code,
            //                            agentName = JAA.name,
            //                            agentCode = JAA.code,
            //                            agentType = JAA.type,
            //                            cuserName = JUU.name,
            //                            cuserLast = JUU.lastname,
            //                            cUserAccName = JUU.username,
            //                            uuserName = JUPUS.name,
            //                            uuserLast = JUPUS.lastname,
            //                            uUserAccName = JUPUS.username,
            //                            agentCompany = JAA.company,

            //                            //username

            //                            //  I.invoiceId,
            //                            //    JBB.name
            //                        }).ToList();





            //        if (invListm == null)
            //            return NotFound();
            //        else
            //            return Ok(invListm);
            //    }

            //}

            ////else
            //return NotFound();
        }


        // فواتير المبيعات مع العناصر
        [HttpPost]
        [Route("GetSaleOffer")]
        public string GetSaleOffer(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                        join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                        join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        //    join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP
                                        join ITO in entity.itemTransferOffer on IT.itemsTransId equals ITO.itemTransId
                                        join O in entity.offers on ITO.offerId equals O.offerId

                                        //join ITOF in entity.itemsTransfer on ITO.itemTransId equals ITOF.itemsTransId 
                                        //   from  IUO in entity.itemsOffers.Where(X=> X.offerId == O.offerId).Distinct()


                                        // from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")
                                        orderby IT.itemsTransId
                                        select new
                                        {
                                            // offer

                                            Oname = O.name,
                                            OofferId = O.offerId,
                                            Oitemofferid = ITO.id,
                                            //Oquantity = IUO.quantity,
                                            Ocode = O.code,
                                            OisActive = O.isActive,
                                            OdiscountType = O.discountType,
                                            OdiscountValue = O.discountValue,
                                            OstartDate = O.startDate,
                                            OendDate = O.endDate,
                                            OcreateDate = O.createDate,
                                            OupdateDate = O.updateDate,
                                            OcreateUserId = O.createUserId,
                                            OupdateUserId = O.updateUserId,
                                            Onotes = O.notes,

                                            //itemtransfer
                                            ITitemName = ITEM.name,
                                            ITunitName = UNIT.name,
                                            ITitemsTransId = IT.itemsTransId,
                                            ITitemUnitId = IT.itemUnitId,

                                            ITitemId = IU.itemId,
                                            ITunitId = IU.unitId,
                                            ITquantity = IT.quantity,

                                            ITcreateDate = IT.createDate,
                                            ITupdateDate = IT.updateDate,
                                            ITcreateUserId = IT.createUserId,
                                            ITupdateUserId = IT.updateUserId,
                                            ITnotes = IT.notes,
                                            ITprice = IT.price,
                                            ITbarcode = IU.barcode,
                                            ITCreateuserName = ITCUSER.name,
                                            ITCreateuserLName = ITCUSER.lastname,
                                            ITCreateuserAccName = ITCUSER.username,

                                            ITUpdateuserName = ITUPUSER.name,
                                            ITUpdateuserLName = ITUPUSER.lastname,
                                            ITUpdateuserAccName = ITUPUSER.username,
                                            I.invoiceId,
                                            I.invNumber,
                                          I.invBarcode,                                         
                                            I.agentId,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                           discountType="1",
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            //  branchName = JBB.name,

                                            //  branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,
                                            agentName = JAA.name,
                                            agentCode = JAA.code,
                                            agentType = JAA.type,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,

                                            subTotal = (IT.price * IT.quantity),
                                            // couponTotalValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? ((I.discountValue / 100) * I.total) : 0),
                                            offerTotalValue = (O.discountType == "1" || O.discountType == null) ? (O.discountValue * (IT.quantity)) : (O.discountType == "2" ? ((O.discountValue / 100) * (IT.price * IT.quantity)) : 0),

                                        }).ToList();

                        /*
                            public int offerId { get; set; }
                                public string name { get; set; }
                                public string code { get; set; }
                                public Nullable<byte> isActive { get; set; }
                                public string discountType { get; set; }
                                public Nullable<decimal> discountValue { get; set; }
                                public Nullable<System.DateTime> startDate { get; set; }
                                public Nullable<System.DateTime> endDate { get; set; }
                                public Nullable<System.DateTime> createDate { get; set; }
                                public Nullable<System.DateTime> updateDate { get; set; }
                                public Nullable<int> createUserId { get; set; }
                                public Nullable<int> updateUserId { get; set; }
                                public string notes { get; set; }

    offerId
    name
    code
    isActive
    discountType
    discountValue
    startDate
    endDate
    createDate
    updateDate
    createUserId
    updateUserId
    notes

                         * */





                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            //           
            //            
            //            string token = "";
            //            if (headers.Contains("APIKey"))
            //            {
            //                token = headers.GetValues("APIKey").First();
            //            }
            //            Validation validation = new Validation();
            //            bool valid = validation.CheckApiKey(token);

            //            if (valid) // APIKey is valid
            //            {
            //                List<int> brIds = AllowedBranchsId(mainBranchId, userId);

            //                using (incposdbEntities entity = new incposdbEntities())
            //                {
            //                    var invListm = (from IT in entity.itemsTransfer
            //                                    from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

            //                                    from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
            //                                    join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
            //                                    join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
            //                                    join ITEM in entity.items on IU.itemId equals ITEM.itemId
            //                                    join UNIT in entity.units on IU.unitId equals UNIT.unitId
            //                                    //    join B in entity.branches on I.branchId equals B.branchId into JB
            //                                    join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
            //                                    join A in entity.agents on I.agentId equals A.agentId into JA
            //                                    join U in entity.users on I.createUserId equals U.userId into JU
            //                                    join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
            //                                    join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
            //                                    join P in entity.pos on I.posId equals P.posId into JP
            //                                    join ITO in entity.itemTransferOffer on IT.itemsTransId equals ITO.itemTransId
            //                                    join O in entity.offers on ITO.offerId equals O.offerId

            //                                    //join ITOF in entity.itemsTransfer on ITO.itemTransId equals ITOF.itemsTransId 
            //                                    //   from  IUO in entity.itemsOffers.Where(X=> X.offerId == O.offerId).Distinct()


            //                                    // from JBB in JB
            //                                    from JPP in JP.DefaultIfEmpty()
            //                                    from JUU in JU.DefaultIfEmpty()
            //                                    from JUPUS in JUPUSR.DefaultIfEmpty()
            //                                    from JIMM in JIM.DefaultIfEmpty()
            //                                    from JAA in JA.DefaultIfEmpty()
            //                                    from JBCC in JBC.DefaultIfEmpty()
            //                                    where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "sb")
            //                                    orderby IT.itemsTransId
            //                                    select new
            //                                    {
            //                                        // offer

            //                                        Oname = O.name,
            //                                        OofferId = O.offerId,
            //                                        Oitemofferid = ITO.id,
            //                                        //Oquantity = IUO.quantity,
            //                                        Ocode = O.code,
            //                                        OisActive = O.isActive,
            //                                        OdiscountType = O.discountType,
            //                                        OdiscountValue = O.discountValue,
            //                                        OstartDate = O.startDate,
            //                                        OendDate = O.endDate,
            //                                        OcreateDate = O.createDate,
            //                                        OupdateDate = O.updateDate,
            //                                        OcreateUserId = O.createUserId,
            //                                        OupdateUserId = O.updateUserId,
            //                                        Onotes = O.notes,

            //                                        //itemtransfer
            //                                        ITitemName = ITEM.name,
            //                                        ITunitName = UNIT.name,
            //                                        ITitemsTransId = IT.itemsTransId,
            //                                        ITitemUnitId = IT.itemUnitId,

            //                                        ITitemId = IU.itemId,
            //                                        ITunitId = IU.unitId,
            //                                        ITquantity = IT.quantity,

            //                                        ITcreateDate = IT.createDate,
            //                                        ITupdateDate = IT.updateDate,
            //                                        ITcreateUserId = IT.createUserId,
            //                                        ITupdateUserId = IT.updateUserId,
            //                                        ITnotes = IT.notes,
            //                                        ITprice = IT.price,
            //                                        ITbarcode = IU.barcode,
            //                                        ITCreateuserName = ITCUSER.name,
            //                                        ITCreateuserLName = ITCUSER.lastname,
            //                                        ITCreateuserAccName = ITCUSER.username,

            //                                        ITUpdateuserName = ITUPUSER.name,
            //                                        ITUpdateuserLName = ITUPUSER.lastname,
            //                                        ITUpdateuserAccName = ITUPUSER.username,
            //                                        I.invoiceId,
            //                                        I.invNumber,
            //                                        I.agentId,
            //                                        I.posId,
            //                                        I.invType,
            //                                        I.total,
            //                                        I.totalNet,
            //                                        I.paid,
            //                                        I.deserved,
            //                                        I.deservedDate,
            //                                        I.invDate,
            //                                        I.invoiceMainId,
            //                                        I.invCase,
            //                                        I.invTime,
            //                                        I.notes,
            //                                        I.vendorInvNum,
            //                                        I.vendorInvDate,
            //                                        I.createUserId,
            //                                        I.updateDate,
            //                                        I.updateUserId,
            //                                        I.branchId,
            //                                        discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
            //                                        I.discountType,
            //                                        I.tax,
            //                                        I.name,
            //                                        I.isApproved,

            //                                        //
            //                                        I.branchCreatorId,
            //                                        branchCreatorName = JBCC.name,
            //                                        //
            //                                        //  branchName = JBB.name,

            //                                        //  branchType = JBB.type,
            //                                        posName = JPP.name,
            //                                        posCode = JPP.code,
            //                                        agentName = JAA.name,
            //                                        agentCode = JAA.code,
            //                                        agentType = JAA.type,
            //                                        cuserName = JUU.name,
            //                                        cuserLast = JUU.lastname,
            //                                        cUserAccName = JUU.username,
            //                                        uuserName = JUPUS.name,
            //                                        uuserLast = JUPUS.lastname,
            //                                        uUserAccName = JUPUS.username,
            //                                        agentCompany = JAA.company,

            //                                        subTotal = (IT.price * IT.quantity),
            //                                        // couponTotalValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? ((I.discountValue / 100) * I.total) : 0),
            //                                        offerTotalValue = (O.discountType == "1" || O.discountType == null) ? (O.discountValue * (IT.price * IT.quantity)) : (O.discountType == "2" ? ((O.discountValue / 100) * (IT.price * IT.quantity)) : 0),

            //                                    }).ToList();

            //                    /*
            //                        public int offerId { get; set; }
            //                            public string name { get; set; }
            //                            public string code { get; set; }
            //                            public Nullable<byte> isActive { get; set; }
            //                            public string discountType { get; set; }
            //                            public Nullable<decimal> discountValue { get; set; }
            //                            public Nullable<System.DateTime> startDate { get; set; }
            //                            public Nullable<System.DateTime> endDate { get; set; }
            //                            public Nullable<System.DateTime> createDate { get; set; }
            //                            public Nullable<System.DateTime> updateDate { get; set; }
            //                            public Nullable<int> createUserId { get; set; }
            //                            public Nullable<int> updateUserId { get; set; }
            //                            public string notes { get; set; }

            //offerId
            //name
            //code
            //isActive
            //discountType
            //discountValue
            //startDate
            //endDate
            //createDate
            //updateDate
            //createUserId
            //updateUserId
            //notes

            //                     * */



            //                    if (invListm == null)
            //                        return NotFound();
            //                    else
            //                        return Ok(invListm);
            //                }

            //            }

            //            //else
            //            return NotFound();
        }

        //
        // فواتير المبيعات مع العناصر التي فيها offer
        [HttpPost]
        [Route("GetPromoOffer")]
        public string GetPromoOffer(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)

                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        //    join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR

                                        join P in entity.pos on I.posId equals P.posId into JP

                                        join O in entity.offers on IT.offerId equals O.offerId

                                        //join ITOF in entity.itemsTransfer on ITO.itemTransId equals ITOF.itemsTransId 
                                        //   from  IUO in entity.itemsOffers.Where(X=> X.offerId == O.offerId).Distinct()
                                        where (IT.offerId != null)

                                        // from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()

                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")

                                        select new
                                        {
                                            // offer

                                            Oname = O.name,//*
                                            OofferId = O.offerId,

                                            //Oquantity = IUO.quantity,
                                            Ocode = O.code,//*
                                            OisActive = O.isActive,
                                            OdiscountType = IT.offerType,//*
                                            OdiscountValue = IT.offerValue,//*
                                            OstartDate = O.startDate,
                                            OendDate = O.endDate,
                                            OcreateDate = O.createDate,
                                            OupdateDate = O.updateDate,
                                            OcreateUserId = O.createUserId,
                                            OupdateUserId = O.updateUserId,
                                            //Onotes = O.notes,

                                            //itemtransfer
                                            ITitemName = ITEM.name,
                                            ITunitName = UNIT.name,
                                            ITitemsTransId = IT.itemsTransId,
                                            ITitemUnitId = IT.itemUnitId,

                                            ITitemId = IU.itemId,
                                            ITunitId = IU.unitId,
                                            ITquantity = IT.quantity,//*


                                            //ITnotes = IT.notes,
                                            ITprice = IT.price,
                                            ITbarcode = IU.barcode,

                                            I.invoiceId,
                                            I.invNumber,//*
                                            I.invBarcode,
                                            I.agentId,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            //I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                            discountType="1",
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            //  branchName = JBB.name,

                                            //  branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,
                                            agentName = JAA.name,
                                            agentCode = JAA.code,
                                            agentType = JAA.type,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,

                                            subTotal = (IT.price * IT.quantity),//*
                                            // couponTotalValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? ((I.discountValue / 100) * I.total) : 0),
                                            offerTotalValue = (IT.offerType == 1 || IT.offerType == null) ? (IT.offerValue * (IT.quantity)) : (IT.offerType == 2 ? ((IT.offerValue / 100) * (IT.itemUnitPrice * IT.quantity)) : 0),

                                        }).ToList();



                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }


        // الاشتراكات
        //الاشتراكات في كل فاتورة
        [HttpPost]
        [Route("GetSaleMembership")]
        public string GetSaleMembership(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    List<SalesMembership> invListm = new List<SalesMembership>();
                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        invListm = (
                            from I in entity.invoices

                            join B in entity.branches on I.branchId equals B.branchId into JB
                            //join BC in entity.branches on I.branchId equals BC.branchId into JBC
                            join A in entity.agents on I.agentId equals A.agentId into JA
                            join U in entity.users on I.createUserId equals U.userId into JU
                            join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                            //join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                            join P in entity.pos on I.posId equals P.posId into JP

                            from JBB in JB.DefaultIfEmpty()
                            from JPP in JP.DefaultIfEmpty()
                            from JUU in JU.DefaultIfEmpty()
                            from JUPUS in JUPUSR.DefaultIfEmpty()
                                //from JIMM in JIM.DefaultIfEmpty()
                            from JAA in JA.DefaultIfEmpty()
                                //from JBCC in JBC.DefaultIfEmpty()
                            where (brIds.Contains(JBB.branchId)) && (I.invType == "s" || I.invType == "sb"
                            || I.invType == "ts" || I.invType == "ss") && (I.membershipId != null)

                            select new SalesMembership
                            {

                                invoiceId = I.invoiceId,
                                // count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                invNumber = I.invNumber,
                                invBarcode = I.invBarcode,
                                posId = I.posId,
                                invType = I.invType,
                                total = I.total,
                                totalNet = I.totalNet,
                                paid = I.paid,
                                deserved = I.deserved,
                                deservedDate = I.deservedDate,
                                invDate = I.invDate,
                                invoiceMainId = I.invoiceMainId,
                                invCase = I.invCase,
                                invTime = I.invTime,
                                notes = I.notes,
                                vendorInvNum = I.vendorInvNum,
                                vendorInvDate = I.vendorInvDate,
                                createUserId = I.createUserId,
                                updateDate = I.updateDate,
                                updateUserId = I.updateUserId,
                                branchId = I.branchId,
                                discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),

                                discountType = I.discountType,
                                tax = I.tax,
                                //  name= I.name,
                                isApproved = I.isApproved,

                                //
                                //I.branchCreatorId,
                                //     branchCreatorName = JBCC.name,
                                //
                                branchName = JBB.name,
                                posName = JPP.name,
                                posCode = JPP.code,

                                agentCode = JAA.code,
                                //
                                agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                "unknown" : JAA.name,


                                //   agentType = JAA.type,
                                agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                ? "c" : JAA.type,
                                agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                ? 0 : I.agentId,


                                //cuserName = JUU.name,
                                //cuserLast = JUU.lastname,
                                //cUserAccName = JUU.username,
                                uuserName = JUPUS.name,
                                uuserLast = JUPUS.lastname,
                                uUserAccName = JUPUS.username,
                                agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                "unknown" : JAA.company,

                                membershipId = I.membershipId,
                                membershipsName = I.memberships.name,
                                membershipsCode = I.memberships.code,
                                invoiceClassDiscountList = entity.invoiceClassDiscount.Where(X => X.invoiceId == I.invoiceId).Select(X => new invoicesClassModel
                                {
                                    name = X.invoicesClass.name,
                                    invClassDiscountId = X.invClassDiscountId,
                                    invoiceId = X.invoiceId,
                                    invClassId = (int)X.invClassId,
                                    discountType = X.discountType,
                                    discountValue = X.discountValue,
                                    finalDiscount = (decimal)X.discountType == 2 ? (X.discountValue / 100) * (I.total) : X.discountValue
                                }).ToList(),
                                CouponInvoiceList = entity.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId && X.forAgents == "pr").Select(X => new CouponInvoiceModel
                                {
                                    couponId = X.couponId,

                                    id = X.id,
                                    InvoiceId = X.InvoiceId,
                                    discountType = X.discountType,
                                    discountValue = X.discountValue,
                                    forAgents = X.forAgents,
                                    couponCode = X.coupons.code,
                                    name = X.coupons.name,
                                    finalDiscount = (decimal)X.discountType == 2 ? (X.discountValue / 100) * (I.total) : X.discountValue,
                                }).ToList(),

                                itemsTransferList = entity.itemsTransfer.Where(X => X.invoiceId == I.invoiceId && X.offerId > 0 && X.forAgents == "pr").Select(X => new ItemTransferModel
                                {
                                    itemsTransId = X.itemsTransId,

                                    //itemId = X.itemId,
                                    //itemName = X.name,
                                    quantity = X.quantity,

                                    itemUnitId = X.itemUnitId,
                                    price = X.price,
                                    //unitName = X.name,
                                    //unitId = X.unitId,
                                    //barcode = X.barcode,
                                    itemSerial = X.itemSerial,
                                    //itemType = X.type,

                                    //avgPurchasePrice = X.avgPurchasePrice,
                                    notes = X.notes,

                                    invoiceId = X.invoiceId,

                                    offerId = X.offerId,
                                    itemTax = X.itemTax,
                                    itemUnitPrice = X.itemUnitPrice,
                                    offerValue = X.offerValue,
                                    offerType = X.offerType,
                                    forAgents = X.forAgents,
                                    offerCode = X.offers.code,
                                    offerName = X.offers.name,
                                    finalDiscount = (decimal)X.offerType == 2 ? ((X.offerValue / 100) * (X.itemUnitPrice)) * X.quantity : X.offerValue * X.quantity,
                                }).ToList(),

                                invclassDiscount = 0,
                                couponDiscount = 0,
                                offerDiscount = 0,
                                totalDiscount = 0,
                                subscriptionType = I.memberships.subscriptionType,
                                agentMembershipcashobjList = entity.agentMembershipCash.Where(x =>
                                 x.agentId == I.agentId && I.membershipId == x.membershipId && I.memberships.subscriptionType == x.subscriptionType).Select(x => new AgentMembershipCashModel() {
                                     agentMembershipCashId = x.agentMembershipCashId,
                                     subscriptionFeesId = x.subscriptionFeesId,
                                     cashTransId = x.cashTransId,
                                     membershipId = x.membershipId,
                                     agentId = x.agentId,
                                     startDate = x.startDate,
                                     endDate = x.endDate,
                                     notes = x.notes,
                                     updateUserId = x.updateUserId,
                                     isActive = x.isActive,
                                     createDate = x.createDate,
                                     updateDate = x.updateDate,
                                     createUserId = x.createUserId,
                                     subscriptionType = x.subscriptionType,
                                     total = x.total,
                                     monthsCount = x.monthsCount,

                                 }).ToList(),
                               // endDate,
                            }).ToList();
                       
                        //      var tmplist=   entity.agentMembershipCash.Where(x => x.agentId == row.agentId && row.membershipId == x.membershipId && row.subscriptionType == x.subscriptionType).OrderBy(x => x.updateDate).
                       // Select(x => new { x.endDate }).ToList();

                        foreach (var row in invListm)
                        {
                            
                             //var tmplist=   entity.agentMembershipCash.Where(x => x.agentId == row.agentId && row.membershipId == x.membershipId && row.subscriptionType == x.subscriptionType).OrderBy(x => x.updateDate).
                             //   Select(x => new { x.endDate }).ToList();
                            row.endDate = row.agentMembershipcashobjList.ToList()!=null && row.agentMembershipcashobjList.ToList().Count()>0 ? row.agentMembershipcashobjList.ToList().LastOrDefault().endDate :null;
                            row.invclassDiscount = (decimal)row.invoiceClassDiscountList.Sum(X => X.discountType == 2 ? (X.discountValue / 100) * (row.total) : X.discountValue);
                            row.couponDiscount = (decimal)row.CouponInvoiceList.Sum(X => X.discountType == 2 ? (X.discountValue / 100) * (row.total) : X.discountValue);
                            row.offerDiscount = (decimal)row.itemsTransferList.Sum(X => X.offerType == 2 ? ((X.offerValue / 100) * (X.itemUnitPrice)) * X.quantity : X.offerValue * X.quantity);
                            row.totalDiscount = row.invclassDiscount + row.couponDiscount + row.offerDiscount;
                        }
                        invListm = invListm.Where(X => X.totalDiscount > 0).ToList();
                        return TokenManager.GenerateToken(invListm);
                    }

                }
                catch (Exception ex)
                {
                    //return TokenManager.GenerateToken("0");
                    return TokenManager.GenerateToken(ex.ToString());
                }

            }


        }

        [HttpPost]
        [Route("GetInvoiceClass")]
        public string GetInvoiceClass(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    List<SalesMembership> invListm = new List<SalesMembership>();
                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        invListm = (
                            from I in entity.invoices

                            join B in entity.branches on I.branchId equals B.branchId into JB
                            //join BC in entity.branches on I.branchId equals BC.branchId into JBC
                            join A in entity.agents on I.agentId equals A.agentId into JA
                            join U in entity.users on I.createUserId equals U.userId into JU
                            join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                            //join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                            join P in entity.pos on I.posId equals P.posId into JP
                            join IC in entity.invoiceClassDiscount on I.invoiceId equals IC.invoiceId  
                            from JBB in JB.DefaultIfEmpty()
                            from JPP in JP.DefaultIfEmpty()
                            from JUU in JU.DefaultIfEmpty()
                            from JUPUS in JUPUSR.DefaultIfEmpty()
                                //from JIMM in JIM.DefaultIfEmpty()
                            from JAA in JA.DefaultIfEmpty()
                                //from JBCC in JBC.DefaultIfEmpty()
                            where (brIds.Contains(JBB.branchId)) && (I.invType == "s" || I.invType == "sb"
                            || I.invType == "ts" || I.invType == "ss") && (I.membershipId != null)

                            select new SalesMembership
                            {

                                invoiceId = I.invoiceId,
                                // count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                invNumber = I.invNumber,
                                invBarcode = I.invBarcode,
                                posId = I.posId,
                                invType = I.invType,
                                total = I.total,
                                totalNet = I.totalNet,
                                paid = I.paid,
                                deserved = I.deserved,
                                deservedDate = I.deservedDate,
                                invDate = I.invDate,
                                invoiceMainId = I.invoiceMainId,
                                invCase = I.invCase,
                                invTime = I.invTime,
                                notes = I.notes,
                                vendorInvNum = I.vendorInvNum,
                                vendorInvDate = I.vendorInvDate,
                                createUserId = I.createUserId,
                                updateDate = I.updateDate,
                                updateUserId = I.updateUserId,
                                branchId = I.branchId,
                                discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),

                                discountType = I.discountType,
                                tax = I.tax,
                                //  name= I.name,
                                isApproved = I.isApproved,

                                //
                                //I.branchCreatorId,
                                //     branchCreatorName = JBCC.name,
                                //
                                branchName = JBB.name,
                                posName = JPP.name,
                                posCode = JPP.code,

                                agentCode = JAA.code,
                                //
                                agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                "unknown" : JAA.name,


                                //   agentType = JAA.type,
                                agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                ? "c" : JAA.type,
                                agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss"))
                                ? 0 : I.agentId,


                                //cuserName = JUU.name,
                                //cuserLast = JUU.lastname,
                                //cUserAccName = JUU.username,
                                uuserName = JUPUS.name,
                                uuserLast = JUPUS.lastname,
                                uUserAccName = JUPUS.username,
                                agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb" || I.invType == "ts" || I.invType == "ss")) ?
                                "unknown" : JAA.company,

                                membershipId = I.membershipId,
                                membershipsName = I.memberships.name,
                                membershipsCode = I.memberships.code,
                                //invoiceClassDiscountList = entity.invoiceClassDiscount.Where(X => X.invoiceId == I.invoiceId).Select(X => new invoicesClassModel
                                //{
                                //    name = X.invoicesClass.name,
                                //    invClassDiscountId = X.invClassDiscountId,
                                //    invoiceId = X.invoiceId,
                                //    invClassId = (int)X.invClassId,
                                //    discountType = X.discountType,
                                //    discountValue = X.discountValue,
                                //    finalDiscount = (decimal)X.discountType == 2 ? (X.discountValue / 100) * (I.total) : X.discountValue
                                //}).ToList(),

                               invClassDiscountId=IC.invClassDiscountId,
                               invClassdiscountType=IC.discountType,
                                invClassdiscountValue = IC.discountValue,
                                invoicesClassName=IC.invoicesClass.name,
                                invClassId=IC.invClassId,
                                finalDiscount= (decimal)IC.discountType == 2 ? (IC.discountValue / 100) * (I.total) : IC.discountValue,


                                invclassDiscount = 0,
                                couponDiscount = 0,
                                offerDiscount = 0,
                                totalDiscount = 0,
                                subscriptionType = I.memberships.subscriptionType,
                               
                                // endDate,
                            }).ToList();

                        //      var tmplist=   entity.agentMembershipCash.Where(x => x.agentId == row.agentId && row.membershipId == x.membershipId && row.subscriptionType == x.subscriptionType).OrderBy(x => x.updateDate).
                        // Select(x => new { x.endDate }).ToList();

                        //foreach (var row in invListm)
                        //{

                        //    //var tmplist=   entity.agentMembershipCash.Where(x => x.agentId == row.agentId && row.membershipId == x.membershipId && row.subscriptionType == x.subscriptionType).OrderBy(x => x.updateDate).
                        //    //   Select(x => new { x.endDate }).ToList();
                        //  //  row.endDate = row.agentMembershipcashobjList.ToList() != null && row.agentMembershipcashobjList.ToList().Count() > 0 ? row.agentMembershipcashobjList.ToList().LastOrDefault().endDate : null;
                        //    row.invclassDiscount = (decimal)row.invoiceClassDiscountList.Sum(X => X.discountType == 2 ? (X.discountValue / 100) * (row.total) : X.discountValue);
                        //    row.couponDiscount = (decimal)row.CouponInvoiceList.Sum(X => X.discountType == 2 ? (X.discountValue / 100) * (row.total) : X.discountValue);
                        //    row.offerDiscount = (decimal)row.itemsTransferList.Sum(X => X.offerType == 2 ? ((X.offerValue / 100) * (X.itemUnitPrice)) * X.quantity : X.offerValue * X.quantity);
                        //    row.totalDiscount = row.invclassDiscount + row.couponDiscount + row.offerDiscount;
                        //}
                        //invListm = invListm.Where(X => X.totalDiscount > 0).ToList();
                        return TokenManager.GenerateToken(invListm);
                    }

                }
                catch (Exception ex)
                {
                    //return TokenManager.GenerateToken("0");
                    return TokenManager.GenerateToken(ex.ToString());
                }

            }


        }


        #endregion

        // التخزين
        #region storage

        // عرض الاصناف واماكن تخزينها
        [HttpPost]
        [Route("GetStorage")]
        public string GetStorage(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invListm = (from L in entity.locations
                                            //  from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)


                                        join IUL in entity.itemsLocations on L.locationId equals IUL.locationId
                                        join IU in entity.itemsUnits on IUL.itemUnitId equals IU.itemUnitId

                                        //  join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                        //join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        join S in entity.sections on L.sectionId equals S.sectionId into JS
                                        join B in entity.branches on L.branchId equals B.branchId into JB


                                        join UPUSR in entity.users on IUL.updateUserId equals UPUSR.userId into JUPUS
                                        join U in entity.users on IUL.createUserId equals U.userId into JU



                                        from JBB in JB
                                        from JSS in JS.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUSS in JUPUS.DefaultIfEmpty()

                                        where (brIds.Contains(JBB.branchId) && ITEM.type != "sr")

                                        select new
                                        {
                                            /*branchId-agentType-agentName-invType-invNumber-ItemUnit-quantity-AgentTypeAgent
                                              InvTypeNumber-itemId-branchId-unitId-Secname-ItemUnits-SectionLoactionName-startDate-endDate-MinAll
                                              MaxAll-sectionId-locationId-LoactionName
                                              exportBranch-importBranch*/
                                            // item unit     
                                            itemName = ITEM.name,
                                            ITEM.min,
                                            ITEM.max,
                                            ITEM.minUnitId,
                                            ITEM.maxUnitId,
                                            itemType = ITEM.type,
                                            minUnitName = entity.units.Where(x => x.unitId == ITEM.minUnitId).FirstOrDefault().name,
                                            maxUnitName = entity.units.Where(x => x.unitId == ITEM.maxUnitId).FirstOrDefault().name,
                                            unitName = UNIT.name,
                                            IU.itemUnitId,

                                            IU.itemId,
                                            IU.unitId,

                                            //IU.barcode,
                                            //item location
                                            //CreateuserName = JUU.name,
                                            //CreateuserLName = JUU.lastname,
                                            //CreateuserAccName = JUU.username,
                                            //UuserName = JUPUSS.name,
                                            //UuserLName = JUPUSS.lastname,
                                            UuserAccName = JUPUSS.username,
                                            //
                                            branchId = L.branchId,
                                            branchName = JBB.name,
                                            branchType = JBB.type,

                                            //itemslocations

                                            IUL.itemsLocId,
                                            IUL.locationId,
                                            quantity = IUL.quantity,

                                            startDate = IUL.startDate,
                                            endDate = IUL.endDate,

                                            //IULnote = IUL.note,
                                            IU.storageCostId,

                                            //storageCostName = IU.storageCostId != null ? entity.storageCost.Where(X => X.storageCostId == IU.storageCostId).FirstOrDefault().name : "",
                                            storageCostValue = IU.storageCostId != null ?
                                          entity.storageCost.Where(X => X.storageCostId == IU.storageCostId).FirstOrDefault().cost
                                          : 0,

                                            IUL.updateDate,
                                            //cuserName = JUU.name,
                                            //cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            //uuserName = JUPUSS.name,
                                            //uuserLast = JUPUSS.lastname,
                                            uUserAccName = JUPUSS.username,
                                            // Location
                                            L.x,
                                            L.y,
                                            L.z,

                                            //LocisActive = L.isActive,
                                            L.sectionId,
                                            //Locnote = L.note,
                                            //L.branchId,
                                            LocisFreeZone = L.isFreeZone,


                                            // section

                                            Secname = JSS.name,
                                            SecisActive = JSS.isActive,
                                            //Secnote = JSS.note,
                                            SecisFreeZone = JSS.isFreeZone,


                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();


                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }



        // حركة الاصناف الخارجية -مع الموردين
        [HttpPost]
        [Route("GetExternalMov")]
        public string GetExternalMov(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                        join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                        join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        from JPP in entity.pos.Where(X => X.posId == I.posId)
                                        join BP in entity.branches on JPP.branchId equals BP.branchId

                                        from JBB in JB.DefaultIfEmpty()
                                            //   from JPP into  JP.DefaultIfEmpty
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()

                                        where (brIds.Contains(JBCC.branchId) || brIds.Contains(JBB.branchId))
                                        && (I.invType == "p"  || I.invType == "pb")// exw
                                        && ITEM.type != "sr"
                                        select new
                                        {
                                            /*itemId-itemName-branchId-unitId-unitName-agentId-agentName
                                              agentType-invType-invoiceId-invNumber*/

                                            itemName = ITEM.name,
                                            unitName = UNIT.name,
                                            // IT.itemsTransId,
                                            IT.itemUnitId,

                                            IU.itemId,
                                            IU.unitId,
                                            quantity = IT.quantity,

                                            // createDate = IT.createDate,
                                            //updateDate = IT.updateDate,
                                            //  ITcreateUserId = IT.createUserId,
                                            //ITupdateUserId = IT.updateUserId,
                                            //notes = IT.notes,
                                            //IT.price,
                                            //IU.barcode,
                                            //CreateuserName = ITCUSER.name,
                                            //CreateuserLName = ITCUSER.lastname,
                                            //CreateuserAccName = ITCUSER.username,

                                            // UpdateuserName = ITUPUSER.name,
                                            // UpdateuserLName = ITUPUSER.lastname,
                                            // UpdateuserAccName = ITUPUSER.username,
                                            I.invoiceId,
                                            I.invNumber,
                                        I.invBarcode,                                            
                                            //I.posId,
                                            I.invType,
                                            //I.total,
                                            //I.totalNet,
                                            //I.paid,
                                            //I.deserved,
                                            //I.deservedDate,
                                            I.invDate,
                                            //I.invoiceMainId,
                                            //I.invCase,
                                            //I.invTime,
                                            //I.notes,
                                            //I.vendorInvNum,
                                            // I.vendorInvDate,
                                            //I.createUserId,
                                            I.updateDate,
                                            //I.updateUserId,
                                            // I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),

                                            //I.discountType,
                                            //I.tax,
                                            //I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            branchName = ((I.invType == "s" || I.invType == "pb" || I.invType == "sb") ? JBCC.name : (I.invType == "p" ? JBB.name : JBCC.name)),
                                            branchId = ((I.invType == "s" || I.invType == "pb" || I.invType == "sb") ? I.branchCreatorId : (I.invType == "p" ? I.branchId : I.branchCreatorId)),
                                            //  branchName = JBB.name,

                                            branchType = JBB.type,
                                            //posName = JPP.name,
                                            //posCode = JPP.code,
                                            //agentCode = JAA.code,
                                            //   agentName =  JAA.name,
                                            agentName = ((I.agentId == null || I.agentId == 0) && (I.invType == "s" || I.invType == "sb" || I.invType == "p" || I.invType == "pb")) ? "unknown" : JAA.name,


                                            //   agentType = JAA.type,
                                            //agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
                                            //? "c" : JAA.type,
                                            agentType = ((I.agentId == null || I.agentId == 0) && (I.invType == "s" || I.invType == "sb")) ? "c" :
                                                                           ((I.agentId == null || I.agentId == 0) && (I.invType == "p" || I.invType == "pb")) ? "v" : JAA.type,


                                            agentId = ((I.agentId == null || I.agentId == 0) && (I.invType == "s" || I.invType == "sb" || I.invType == "p" || I.invType == "pb")) ? 0 : I.agentId,

                                            //cuserName = JUU.name,
                                            //cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            //uuserName = JUPUS.name,
                                            //uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,

                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();







                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        [HttpPost]
        [Route("GetDirectInMov")]
        public string GetDirectInMov(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                        join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                        join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        from JPP in entity.pos.Where(X => X.posId == I.posId)
                                        join BP in entity.branches on JPP.branchId equals BP.branchId

                                        from JBB in JB.DefaultIfEmpty()
                                            //   from JPP into  JP.DefaultIfEmpty
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()

                                        where (brIds.Contains(JBCC.branchId) || brIds.Contains(JBB.branchId)) && ITEM.type != "sr"


                                        && (I.invType == "is")// exw

                                        select new
                                        {
                                            /*itemId-itemName-branchId-unitId-unitName-agentId-agentName
                                              agentType-invType-invoiceId-invNumber*/

                                            itemName = ITEM.name,
                                            unitName = UNIT.name,

                                            IT.itemUnitId,

                                            IU.itemId,
                                            IU.unitId,
                                            quantity = IT.quantity,

                                            I.updateDate,
                                            I.invoiceId,
                                            I.invNumber,
                                           I.invBarcode,
                                            I.invDate,
                                            I.invType,

                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),

                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            branchName = JBB.name,
                                            branchId = I.branchId,


                                            branchType = JBB.type,

                                            agentName = (I.agentId == null || I.agentId == 0) ? "unknown" : JAA.name,



                                            agentType = JAA.type,

                                            agentId = I.agentId == null ? 0 : I.agentId,

                                            cUserAccName = JUU.username,

                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,


                                        }).ToList();







                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        // حركة الاصناف الداخلية -بين الفروع والمخازن
        [HttpPost]
        [Route("GetInternalMov")]
        public string GetInternalMov(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                        join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                        join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        //  from JPP in entity.pos.Where(X => X.posId == I.posId ).DefaultIfEmpty()
                                        //   join BP in entity.branches on JPP.branchId equals BP.branchId

                                        from JBB in JB.DefaultIfEmpty()

                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where ((I.invType == "ex") || (
                                        (I.invType == "im" && I.invoiceMainId == null) ?
                                        (entity.invoices.Where(x => x.invoiceMainId == I.invoiceId && x.invType == "ex").ToList().Count > 0)
                                        : (I.invType == "im" && I.invoiceMainId != null)
                                        ? entity.invoices.Where(x => x.invoiceId == I.invoiceMainId && x.invType == "ex").ToList().Count > 0 : false))
                                        && (brIds.Contains(JBCC.branchId) || brIds.Contains(JBB.branchId)) && ITEM.type != "sr"

                                        select new
                                        {
                                            itemName = ITEM.name,
                                            unitName = UNIT.name,
                                            IT.itemsTransId,
                                            IT.itemUnitId,

                                            IU.itemId,
                                            IU.unitId,
                                            IT.quantity,

                                            // createDate = IT.createDate,
                                            //updateDate = IT.updateDate,
                                            //  ITcreateUserId = IT.createUserId,
                                            //ITupdateUserId = IT.updateUserId,
                                            //notes = IT.notes,
                                            IT.price,
                                            IU.barcode,
                                            CreateuserName = ITCUSER.name,
                                            CreateuserLName = ITCUSER.lastname,
                                            CreateuserAccName = ITCUSER.username,

                                            UpdateuserName = ITUPUSER.name,
                                            UpdateuserLName = ITUPUSER.lastname,
                                            UpdateuserAccName = ITUPUSER.username,
                                            I.invoiceId,
                                            I.invNumber,
                                          I.invBarcode,
                                            I.agentId,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                           discountType="1",
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            movbranchname = JBB.name,
                                            movbranchid = I.branchId,
                                            branchName = JBB.name,

                                            branchType = JBB.type,
                                            //     posName = JPP.name,
                                            //  posCode = JPP.code,
                                            agentName = JAA.name,
                                            agentCode = JAA.code,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = JAA.company,
                                            agentType = JAA.type,
                                            exportBranch = I.invType == "ex" ? JBB.name :
                                            I.invType == "im"
                                            ? (I.invoiceMainId == null) ? (
                                            entity.branches.Where
                                            (B => B.branchId == entity.invoices.Where
                                           (x => x.invoiceMainId == I.invoiceId)
                                           .FirstOrDefault().branchId)
                                            .FirstOrDefault().name) :// I.invoiceMainId not null
                                           entity.branches.Where
                                           (b => b.branchId ==
                                           (entity.invoices.Where
                                           (i => i.invoiceId == I.invoiceMainId)
                                           .FirstOrDefault().branchId))
                                           .FirstOrDefault().name
                                           : "",
                                            importBranch = I.invType == "im" ? JBB.name :
                                            I.invType == "ex"
                                            ? (I.invoiceMainId == null) ? (
                                            entity.branches.Where
                                            (B => B.branchId == entity.invoices.Where
                                           (x => x.invoiceMainId == I.invoiceId)
                                           .FirstOrDefault().branchId)
                                            .FirstOrDefault().name) :// I.invoiceMainId not null
                                           entity.branches.Where
                                           (b => b.branchId ==
                                           (entity.invoices.Where
                                           (i => i.invoiceId == I.invoiceMainId)
                                           .FirstOrDefault().branchId))
                                           .FirstOrDefault().name
                                           : "",
                                            // ex im branchId
                                            exportBranchId = I.invType == "ex" ? I.branchId :
                                            I.invType == "im"
                                            ? (I.invoiceMainId == null) ?
                                           (entity.invoices.Where
                                           (x => x.invoiceMainId == I.invoiceId)
                                           .FirstOrDefault().branchId)
                                             :// I.invoiceMainId not null
                                           (entity.invoices.Where
                                           (i => i.invoiceId == I.invoiceMainId)
                                           .FirstOrDefault().branchId)
                                           : null,

                                            importBranchId = I.invType == "im" ? I.branchId :
                                            I.invType == "ex"
                                             ? (I.invoiceMainId == null) ?
                                           (entity.invoices.Where
                                           (x => x.invoiceMainId == I.invoiceId)
                                           .FirstOrDefault().branchId)
                                             :// I.invoiceMainId not null
                                           (entity.invoices.Where
                                           (i => i.invoiceId == I.invoiceMainId)
                                           .FirstOrDefault().branchId)
                                           : null,
                                            invopr = I.invoiceMainId == null ? I.invoiceId : I.invoiceMainId,
                                        }).ToList();


                        var list = invListm.GroupBy(g => new { g.invopr, g.itemUnitId }).SelectMany(grouping => grouping.Take(1)).ToList();

                        // cashopr = C.cashTransIdSource == null ? C.cashTransId : C.cashTransIdSource,
                        //var list = cachlist.GroupBy(g => g.cashopr).SelectMany(grouping => grouping.Take(1)).ToList();


                        return TokenManager.GenerateToken(list);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        //فواتير الصرف المنفذة من المخزن الى المطبخ 
        [HttpPost]
        [Route("GetSpendingStorage")]
        public string GetSpendingStorage(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchId equals BC.branchId into JBC

                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR

                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "sr" || I.invType == "srb"  )

                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                            I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            //I.total,
                                            //I.totalNet,
                                            //I.paid,
                                            //I.deserved,
                                            //I.deservedDate,
                                            I.invDate,
                                            //I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            //I.vendorInvNum,
                                            //I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            //discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            //I.discountType,
                                            //I.tax,
                                            //I.name,
                                            //I.isApproved,

                                            //
                                            //I.branchCreatorId,
                                            //branchCreatorName = JBCC.name,
                                            //
                                            branchName = JBCC.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex);
                    // return TokenManager.GenerateToken("0");
                }

            }


        }

        #endregion

        // الجرد
        #region
        // عناصر الجرد
        [HttpPost]
        [Route("GetInventory")]
        public string GetInventory(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var List = (from c in entity.inventoryItemLocation
                                    join i in entity.Inventory on c.inventoryId equals i.inventoryId
                                    join l in entity.itemsLocations on c.itemLocationId equals l.itemsLocId
                                    join u in entity.itemsUnits on l.itemUnitId equals u.itemUnitId
                                    join un in entity.units on u.unitId equals un.unitId
                                    join lo in entity.locations on l.locationId equals lo.locationId
                                    join usr in entity.users on i.updateUserId equals usr.userId
                                    //   join s in entity.sections on lo.sectionId equals s.sectionId
                                    where (brIds.Contains(lo.branches.branchId)) && u.items.type != "sr"
                                    select new
                                    {
                                        /*branchId
                                          inventoryType
                                          inventoryDate
                                          branchName
                                          ItemUnits
                                          inventoryNum
                                       
                                          diffPercentage
                                          itemCount
                                          DestroyedCount
                                          causeFalls
                                          inventoryId
                                       */
                                        usr.username,
                                        inventoryILId = c.id,
                                        c.isDestroyed,
                                        c.amount,
                                        c.amountDestroyed,
                                        c.realAmount,
                                        c.itemLocationId,
                                        c.inventoryId,
                                        c.isActive,
                                        c.notes,
                                        i.createDate,
                                        i.updateDate,
                                        i.createUserId,
                                        i.updateUserId,

                                        i.branchId,
                                        branchName = i.branches.name,
                                        u.items.itemId,
                                        itemName = u.items.name,
                                        un.unitId,
                                        u.itemUnitId,
                                        unitName = un.name,

                                        Secname = lo.sections.name,
                                        lo.sectionId,
                                        lo.x,
                                        lo.y,
                                        lo.z,
                                        itemType = u.items.type,
                                        inventoryDate = c.Inventory.createDate,
                                        inventoryNum = c.Inventory.num,
                                        c.Inventory.inventoryType,
                                        // diffPercentage =(c.realAmount == 0) ? 0 : ((( (decimal)(int)c.realAmount-(decimal)(int)c.amount)*100)/(decimal)(int)c.realAmountc.realAmount),
                                        //diffPercentage = (c.realAmount == 0) ? 0 : (((int)c.amount / (decimal)(int)c.realAmount) * 100),
                                    }).ToList();


                        var list2 = List.GroupBy(S => S.inventoryId).Select(X => new
                        {

                            X.FirstOrDefault().inventoryId,
                            X.FirstOrDefault().isDestroyed,
                            DestroyedCount = X.Where(a => a.isDestroyed == true ? true : false).Count(),
                            userFalls = X.FirstOrDefault().username,

                            X.FirstOrDefault().branchName,
                            X.FirstOrDefault().branchId,
                            X.FirstOrDefault().inventoryNum,
                            X.FirstOrDefault().inventoryType,
                            X.FirstOrDefault().inventoryDate,
                            //diffsum= (X.Sum(a=>a.diffPercentage )),
                            // diffPercentage = (X.Sum(a => a.diffPercentage)) / X.Count(),
                            diffPercentage = ((X.Sum(a => (decimal)(int)a.amount) / X.Sum(a => (decimal)(int)a.realAmount)) * 100),
                            itemCount = X.Count(),


                        }).ToList();



                        return TokenManager.GenerateToken(list2);


                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }
        //
        [HttpPost]
        [Route("GetInventoryItems")]
        public string GetInventoryItems(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var List = (from c in entity.inventoryItemLocation
                                    join i in entity.Inventory on c.inventoryId equals i.inventoryId
                                    join l in entity.itemsLocations on c.itemLocationId equals l.itemsLocId
                                    join u in entity.itemsUnits on l.itemUnitId equals u.itemUnitId
                                    join un in entity.units on u.unitId equals un.unitId
                                    join lo in entity.locations on l.locationId equals lo.locationId
                                    //   join s in entity.sections on lo.sectionId equals s.sectionId
                                    where (brIds.Contains(lo.branches.branchId)) && u.items.type != "sr"
                                    select new
                                    {
                                        inventoryILId = c.id,
                                        c.isDestroyed,
                                        c.amount,
                                        c.amountDestroyed,
                                        c.realAmount,
                                        c.itemLocationId,
                                        c.inventoryId,
                                        c.isActive,
                                        c.notes,
                                        i.createDate,
                                        i.updateDate,
                                        i.createUserId,
                                        i.updateUserId,
                                        i.branchId,

                                        branchName = i.branches.name,
                                        u.items.itemId,
                                        itemName = u.items.name,
                                        un.unitId,
                                        u.itemUnitId,
                                        unitName = un.name,

                                        Secname = lo.sections.name,
                                        lo.sectionId,
                                        lo.x,
                                        lo.y,
                                        lo.z,
                                        l.startDate,
                                        l.endDate,
                                        l.itemsLocId,
                                        l.locationId,
                                        itemType = u.items.type,
                                        inventoryDate = c.Inventory.createDate,
                                        inventoryNum = c.Inventory.num,
                                        c.Inventory.inventoryType,
                                        //   shortfalls= (int)c.realAmount - (int)c.amount,
                                        // shortfallspercent= (c.realAmount == 0) ? 0 : ((((decimal)(int)c.realAmount - (decimal)(int)c.amount) * 100) / (decimal)(int)c.realAmount)
                                        // diffPercentage =(c.realAmount == 0) ? 0 : ((( (decimal)(int)c.realAmount-(decimal)(int)c.amount)*100)/(decimal)(int)c.realAmountc.realAmount),
                                        //diffPercentage = (c.realAmount == 0) ? 0 : (((int)c.amount / (decimal)(int)c.realAmount) * 100),
                                    }).ToList();
                        var list2 = List.GroupBy(S => new { S.branchId, S.itemUnitId, S.inventoryId })
                            .Select(X => new
                            {

                                X.FirstOrDefault().inventoryId,
                                X.FirstOrDefault().isDestroyed,
                                DestroyedCount = X.Where(a => a.isDestroyed == true ? true : false).Count(),

                                X.FirstOrDefault().branchName,
                                X.FirstOrDefault().branchId,
                                X.FirstOrDefault().inventoryNum,
                                X.FirstOrDefault().inventoryType,
                                X.FirstOrDefault().inventoryDate,
                                //

                                amount = (X.Sum(a => (int)a.amount)),
                                amountDestroyed = (X.Sum(a => (int)a.amountDestroyed)),
                                realAmount = (X.Sum(a => (int)a.realAmount)),

                                X.FirstOrDefault().itemLocationId,


                                X.FirstOrDefault().notes,
                                X.FirstOrDefault().createDate,
                                X.FirstOrDefault().updateDate,
                                X.FirstOrDefault().createUserId,
                                X.FirstOrDefault().updateUserId,



                                X.FirstOrDefault().itemId,
                                X.FirstOrDefault().itemName,
                                X.FirstOrDefault().unitId,
                                X.FirstOrDefault().itemUnitId,
                                X.FirstOrDefault().unitName,

                                // X.FirstOrDefault().Secname ,
                                // X.FirstOrDefault().sectionId,
                                //  X.FirstOrDefault().x,
                                //  X.FirstOrDefault().y,
                                //  X.FirstOrDefault().z,

                                X.FirstOrDefault().itemsLocId,
                                X.FirstOrDefault().locationId,
                                X.FirstOrDefault().itemType,

                                shortfalls = (int)(X.Sum(a => (int)a.realAmount)) - (int)(X.Sum(a => (int)a.amount)),
                                shortfallspercent = ((X.Sum(a => (int)a.realAmount)) == 0) ? 0 : ((((decimal)(int)(X.Sum(a => (int)a.realAmount)) - (decimal)(int)(X.Sum(a => (int)a.amount))) * 100) / (decimal)(int)(X.Sum(a => (int)a.realAmount))),


                                //diffsum= (X.Sum(a=>a.diffPercentage )),
                                // diffPercentage = (X.Sum(a => a.diffPercentage)) / X.Count(),
                                itemCount = X.Count(),


                            }).ToList();



                        return TokenManager.GenerateToken(list2);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }


        // العناصر التالفة
        [HttpPost]
        [Route("GetDesItems")]
        public string GetDesItems(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)

                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        from JPP in entity.pos.Where(X => X.posId == I.posId)
                                        join BP in entity.branches on JPP.branchId equals BP.branchId
                                        join du in entity.users on I.userId equals du.userId into Dusr
                                        //   from JPP into  JP.DefaultIfEmpty
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from duu in Dusr.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where I.invType == "d" &&
                                        (brIds.Contains(JBCC.branchId) || brIds.Contains(BP.branchId)) && ITEM.type != "sr"

                                        select new
                                        {

                                            causeDestroy = IT.inventoryItemLocation.cause,
                                            //causeDestroy = IT.inventoryItemLocation.fallCause,
                                            userdestroy = duu.username,
                                            //I.userId,
                                            itemName = ITEM.name,
                                            unitName = UNIT.name,
                                            //IT.itemsTransId,
                                            IT.itemUnitId,

                                            IU.itemId,
                                            IU.unitId,
                                            IT.quantity,
                                            //IT.price,
                                            //IU.barcode,

                                            I.invoiceId,
                                            I.invNumber,
                                              I.invBarcode,
                                            //I.posId,
                                            //I.invType,
                                            //I.total,
                                            //I.totalNet,
                                            //I.paid,
                                            //I.deserved,


                                            //I.deservedDate,
                                            //I.invDate,
                                            //I.invoiceMainId,
                                            //I.invCase,
                                            //I.invTime,
                                            //I.notes,
                                            //I.vendorInvNum,
                                            //I.vendorInvDate,
                                            //I.createUserId,
                                            IupdateDate = I.updateDate,
                                            //I.updateUserId,
                                            // I.branchId,
                                            //I.discountValue,
                                            //I.discountType,
                                            //I.tax,
                                            //I.name,
                                            //I.isApproved,
                                            //IT.itemSerial,
                                            //
                                            //I.branchCreatorId,

                                            //
                                            branchName = JBCC.name,
                                            branchId = I.branchCreatorId,

                                            //branchType = JBCC.type,
                                            //posName = JPP.name,
                                            //posCode = JPP.code,

                                            //cuserName = JUU.name,
                                            //cuserLast = JUU.lastname,
                                            //cUserAccName = JUU.username,
                                            //uuserName = JUPUS.name,
                                            //uuserLast = JUPUS.lastname,
                                            //uUserAccName = JUPUS.username,


                                        }).ToList();





                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        // العناصر الناقصة
        [HttpPost]
        [Route("GetFallsItems")]
        public string GetFallsItems(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)

                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        from JPP in entity.pos.Where(X => X.posId == I.posId)
                                        join BP in entity.branches on JPP.branchId equals BP.branchId
                                        join du in entity.users on I.userId equals du.userId into Dusr
                                        //   from JPP into  JP.DefaultIfEmpty
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from duu in Dusr.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where I.invType == "sh" &&
                                         (brIds.Contains(JBCC.branchId) || brIds.Contains(BP.branchId)) && ITEM.type != "sr"

                                        select new
                                        {
                                            /*
                                             inventoryType
                                          */
                                            inventoryNum = IT.inventoryItemLocation.Inventory.num,
                                            inventoryType = IT.inventoryItemLocation.Inventory.inventoryType,
                                            inventoryDate = IT.inventoryItemLocation.Inventory.createDate,
                                            //   itemCount
                                            causeFalls = IT.inventoryItemLocation.fallCause,
                                            //userFalls = duu.username,
                                            //I.userId,
                                            itemName = ITEM.name,
                                            unitName = UNIT.name,
                                            IT.itemsTransId,
                                            IT.itemUnitId,

                                            IU.itemId,
                                            IU.unitId,
                                            itemCount = IT.quantity,
                                            //IT.price,
                                            //IU.barcode,

                                            //I.invoiceId,
                                            //I.invNumber,
                                            /*causeFalls inventoryNum  inventoryType inventoryDate*/
                                            //I.posId,
                                            //I.invType,
                                            //I.total,
                                            //I.totalNet,
                                            //I.paid,
                                            //I.deserved,
                                            //I.deservedDate,
                                            //I.invDate,
                                            //I.invoiceMainId,
                                            //I.invCase,
                                            //I.invTime,
                                            //I.notes,
                                            //I.vendorInvNum,
                                            //I.vendorInvDate,

                                            //I.createUserId,
                                            IupdateDate = I.updateDate,
                                            //I.updateUserId,
                                            // I.branchId,
                                            //I.discountValue,
                                            //I.discountType,
                                            //I.tax,
                                            // I.name,
                                            // I.isApproved,
                                            // IT.itemSerial,
                                            //
                                            //I.branchCreatorId,

                                            //

                                            branchName = JBCC.name,
                                            branchId = I.branchCreatorId,

                                            //branchType = JBCC.type,
                                            //posName = JPP.name,
                                            //posCode = JPP.code,

                                            //cuserName = JUU.name,
                                            //cuserLast = JUU.lastname,
                                            //cUserAccName = JUU.username,
                                            //uuserName = JUPUS.name,
                                            //uuserLast = JUPUS.lastname,
                                            //uUserAccName = JUPUS.username,

                                        }).ToList();





                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        #endregion

        // المحاسبة
        #region
        //المدفوعات
        [HttpPost]
        [Route("GetPayments")]
        public string GetPayments(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                //  IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "mainBranchId")
                //    {
                //        mainBranchId = int.Parse(c.Value);
                //    }
                //    else if (c.Type == "userId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }

                //}
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    //   List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            join b in entity.banks on C.bankId equals b.bankId into jb
                                                            join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.updateUserId equals uc.userId into juc
                                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            from jbb in jb.DefaultIfEmpty()
                                                            from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jcrd in jcr.DefaultIfEmpty()
                                                            from jbbo in jbo.DefaultIfEmpty()
                                                            where (C.transType == "p" && C.processType != "balance")
                                                            //&&  (brIds.Contains(jpp.branches.branchId) || brIds.Contains(jpcc.branches.branchId))

                                                            //( C.transType == "p" && C.side==Side)
                                                            select new CashTransferModel()
                                                            {
                                                                //*cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                //*posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                //*invId = C.invId,
                                                                transNum = C.transNum,
                                                                //*createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                //*updateUserId = C.updateUserId,
                                                                //*createUserId = C.createUserId,
                                                                //*notes = C.notes,
                                                                //*posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                //*cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                //*docName = C.docName,
                                                                //*docNum = C.docNum,
                                                                //*docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                bankName = jbb.name,
                                                                agentName = jaa.name,

                                                                //*usersName = juu.name,// side =u
                                                                userAcc = juu.username,// side =u
                                                                //*posName = jpp.name,
                                                                //*posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                //*cardId = C.cardId,
                                                                //*bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                //*updateUserName = jucc.name,
                                                                //*updateUserLName = jucc.lastname,
                                                                updateUserAcc = jucc.username,
                                                                //*createUserJob = jucc.job,
                                                                cardName = jcrd.name,
                                                                //*bondDeserveDate = jbbo.deserveDate,
                                                                //*bondIsRecieved = jbbo.isRecieved,
                                                                agentCompany = jaa.company,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                shippingCompanyName = C.shippingCompanies.name,

                                                            }).ToList();


                        /*
                        if (cachlist.Count > 0 )
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {if (cashtItem.side == "p") { }
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;
                                // cashtItem.posCreatorName = tempitem.posName;


                            }

                        }
                        */





                        return TokenManager.GenerateToken(cachlist);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

        }

        //bank all
        [HttpPost]
        [Route("GetBankTrans")]
        public string GetBankTrans(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //int mainBranchId = 0;
                //int userId = 0;

                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "mainBranchId")
                //    {
                //        mainBranchId = int.Parse(c.Value);
                //    }
                //    else if (c.Type == "userId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }

                //}
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    // List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            join b in entity.banks on C.bankId equals b.bankId into jb
                                                            join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.updateUserId equals uc.userId into juc
                                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            from jbb in jb.DefaultIfEmpty()
                                                            from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jcrd in jcr.DefaultIfEmpty()
                                                            from jbbo in jbo.DefaultIfEmpty()
                                                            where (C.side == "bn" && C.isConfirm == 1 && C.docNum != null)
                                                            // &&(brIds.Contains(jpp.branches.branchId) || brIds.Contains(jpcc.branches.branchId))

                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                bankName = jbb.name,
                                                                agentName = jaa.name,

                                                                usersName = juu.name,// side =u
                                                                userAcc = juu.username,// side =u
                                                                posName = jpp.name,
                                                                posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                updateUserName = jucc.name,
                                                                updateUserLName = jucc.lastname,
                                                                updateUserAcc = jucc.username,
                                                                createUserJob = jucc.job,
                                                                cardName = jcrd.name,
                                                                bondDeserveDate = jbbo.deserveDate,
                                                                //  bondIsRecieved = jbbo.isRecieved,
                                                                agentCompany = jaa.company,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                shippingCompanyName = C.shippingCompanies.name,

                                                            }).ToList();

                        /*
                        if (cachlist.Count > 0 )
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {if (cashtItem.side == "p") { }
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;
                                // cashtItem.posCreatorName = tempitem.posName;


                            }

                        }
                        */







                        return TokenManager.GenerateToken(cachlist);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }
        // المقبوضات
        [HttpPost]
        [Route("GetReceipt")]
        public string GetReceipt(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //    int mainBranchId = 0;
                //    int userId = 0;

                //    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //    foreach (Claim c in claims)
                //    {
                //        if (c.Type == "mainBranchId")
                //        {
                //            mainBranchId = int.Parse(c.Value);
                //        }
                //        else if (c.Type == "userId")
                //        {
                //            userId = int.Parse(c.Value);
                //        }

                //}
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    //  List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                            join b in entity.banks on C.bankId equals b.bankId into jb
                                                            join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.updateUserId equals uc.userId into juc
                                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            from jbb in jb.DefaultIfEmpty()
                                                            from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jcrd in jcr.DefaultIfEmpty()
                                                            from jbbo in jbo.DefaultIfEmpty()
                                                            where (C.transType == "d" && C.processType != "balance")
                                                            //&& (brIds.Contains(jpp.branches.branchId) || brIds.Contains(jpcc.branches.branchId))

                                                            //( C.transType == "p" && C.side==Side)
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                //*posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                //*invId = C.invId,
                                                                transNum = C.transNum,
                                                                //*createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                //*createUserId = C.createUserId,
                                                                //*notes = C.notes,
                                                                //*posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                //*cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                //*docName = C.docName,
                                                                //*docNum = C.docNum,
                                                                //*docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                bankName = jbb.name,
                                                                agentName = jaa.name,

                                                                usersName = juu.name,// side =u
                                                                userAcc = juu.username,// side =u
                                                                //*posName = jpp.name,
                                                                //*posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                //*cardId = C.cardId,
                                                                //*bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                updateUserName = jucc.name,
                                                                updateUserLName = jucc.lastname,
                                                                updateUserAcc = jucc.username,
                                                                //*createUserJob = jucc.job,
                                                                cardName = jcrd.name,
                                                                //*bondDeserveDate = jbbo.deserveDate,
                                                                //*bondIsRecieved = jbbo.isRecieved,
                                                                agentCompany = jaa.company,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                shippingCompanyName = C.shippingCompanies.name,

                                                            }).ToList();







                        return TokenManager.GenerateToken(cachlist);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

            //var re = Request;
            //
            //string token = "";


            //if (headers.Contains("APIKey"))
            //{
            //    token = headers.GetValues("APIKey").First();
            //}



            //Validation validation = new Validation();
            //bool valid = validation.CheckApiKey(token);

            //if (valid)
            //{
            //    //  List<int> brIds = AllowedBranchsId(mainBranchId, userId);
            //    using (incposdbEntities entity = new incposdbEntities())
            //    {


            //        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
            //                                            join b in entity.banks on C.bankId equals b.bankId into jb
            //                                            join a in entity.agents on C.agentId equals a.agentId into ja
            //                                            join p in entity.pos on C.posId equals p.posId into jp
            //                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
            //                                            join u in entity.users on C.userId equals u.userId into ju
            //                                            join uc in entity.users on C.updateUserId equals uc.userId into juc
            //                                            join cr in entity.cards on C.cardId equals cr.cardId into jcr
            //                                            join bo in entity.bondes on C.bondId equals bo.bondId into jbo
            //                                            from jbb in jb.DefaultIfEmpty()
            //                                            from jaa in ja.DefaultIfEmpty()
            //                                            from jpp in jp.DefaultIfEmpty()
            //                                            from juu in ju.DefaultIfEmpty()
            //                                            from jpcc in jpcr.DefaultIfEmpty()
            //                                            from jucc in juc.DefaultIfEmpty()
            //                                            from jcrd in jcr.DefaultIfEmpty()
            //                                            from jbbo in jbo.DefaultIfEmpty()
            //                                            where (C.transType == "d")
            //                                            //&& (brIds.Contains(jpp.branches.branchId) || brIds.Contains(jpcc.branches.branchId))

            //                                            //( C.transType == "p" && C.side==Side)
            //                                            select new CashTransferModel()
            //                                            {
            //                                                cashTransId = C.cashTransId,
            //                                                transType = C.transType,
            //                                                posId = C.posId,
            //                                                userId = C.userId,
            //                                                agentId = C.agentId,
            //                                                invId = C.invId,
            //                                                transNum = C.transNum,
            //                                                createDate = C.createDate,
            //                                                updateDate = C.updateDate,
            //                                                cash = C.cash,
            //                                                updateUserId = C.updateUserId,
            //                                                createUserId = C.createUserId,
            //                                                notes = C.notes,
            //                                                posIdCreator = C.posIdCreator,
            //                                                isConfirm = C.isConfirm,
            //                                                cashTransIdSource = C.cashTransIdSource,
            //                                                side = C.side,

            //                                                docName = C.docName,
            //                                                docNum = C.docNum,
            //                                                docImage = C.docImage,
            //                                                bankId = C.bankId,
            //                                                bankName = jbb.name,
            //                                                agentName = jaa.name,

            //                                                usersName = juu.name,// side =u
            //                                                userAcc = juu.username,// side =u
            //                                                posName = jpp.name,
            //                                                posCreatorName = jpcc.name,
            //                                                processType = C.processType,
            //                                                cardId = C.cardId,
            //                                                bondId = C.bondId,
            //                                                usersLName = juu.lastname,// side =u
            //                                                updateUserName = jucc.name,
            //                                                updateUserLName = jucc.lastname,
            //                                                updateUserAcc = jucc.username,
            //                                                createUserJob = jucc.job,
            //                                                cardName = jcrd.name,
            //                                                bondDeserveDate = jbbo.deserveDate,
            //                                                bondIsRecieved = jbbo.isRecieved,
            //                                                agentCompany = jaa.company,
            //                                                shippingCompanyId = C.shippingCompanyId,
            //                                                shippingCompanyName = C.shippingCompanies.name,

            //                                            }).ToList();




            //        if (cachlist == null)
            //            return NotFound();
            //        else
            //            return Ok(cachlist);

            //    }
            //}
            //else
            //    return NotFound();
        }


        // pos
        [HttpPost]
        [Route("GetPosTrans")]
        public string GetPosTrans(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //int mainBranchId = 0;
                //int userId = 0;

                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "mainBranchId")
                //    {
                //        mainBranchId = int.Parse(c.Value);
                //    }
                //    else if (c.Type == "userId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }

                //}
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    //  List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {



                        var cachlist = (from C in entity.cashTransfer
                                        join b in entity.banks on C.bankId equals b.bankId into jb
                                        join a in entity.agents on C.agentId equals a.agentId into ja
                                        join p in entity.pos on C.posId equals p.posId into jp
                                        join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                        join u in entity.users on C.userId equals u.userId into ju
                                        join uc in entity.users on C.updateUserId equals uc.userId into juc
                                        join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                        join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                        from jbb in jb.DefaultIfEmpty()
                                        from jaa in ja.DefaultIfEmpty()
                                        from jpp in jp.DefaultIfEmpty()
                                        from juu in ju.DefaultIfEmpty()
                                        from jpcc in jpcr.DefaultIfEmpty()
                                        from jucc in juc.DefaultIfEmpty()
                                        from jcrd in jcr.DefaultIfEmpty()
                                        from jbbo in jbo.DefaultIfEmpty()
                                        where (C.isConfirm == 1 && C.side == "p"
                                        && (C.transType == "d" ?
                                        entity.cashTransfer.Where(x2 => x2.cashTransId == (int)C.cashTransIdSource).FirstOrDefault().isConfirm == 1 :
                                        entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().isConfirm == 1
                                        ))
                                        //  && (brIds.Contains(jpp.branches.branchId) || brIds.Contains(jpcc.branches.branchId))


                                        select new
                                        {
                                            cashTransId = C.cashTransId,
                                            transType = C.transType,
                                            posId = C.posId,
                                            userId = C.userId,
                                            agentId = C.agentId,
                                            //*invId = C.invId,
                                            transNum = C.transNum,
                                            //*createDate = C.createDate,
                                            updateDate = C.updateDate,
                                            cash = C.cash,
                                            updateUserId = C.updateUserId,
                                            createUserId = C.createUserId,
                                            //*notes = C.notes,
                                            posIdCreator = C.posIdCreator,
                                            isConfirm = C.isConfirm,
                                            cashTransIdSource = C.cashTransIdSource,
                                            cashopr = C.cashTransIdSource == null ? C.cashTransId : C.cashTransIdSource,
                                            //*side = C.side,

                                            //*docName = C.docName,
                                            //*docNum = C.docNum,
                                            //*docImage = C.docImage,
                                            //*bankId = C.bankId,
                                            //*bankName = jbb.name,
                                            //*agentName = jaa.name,

                                            usersName = juu.name,// side =u
                                            userAcc = juu.username,// side =u
                                            posName = jpp.name,
                                            posCreatorName = jpcc.name,
                                            //*processType = C.processType,
                                            //*cardId = C.cardId,
                                            //*bondId = C.bondId,
                                            usersLName = juu.lastname,// side =u
                                            updateUserName = jucc.name,
                                            updateUserLName = jucc.lastname,
                                            updateUserAcc = jucc.username,
                                            createUserJob = jucc.job,
                                            //*cardName = jcrd.name,
                                            //*bondDeserveDate = jbbo.deserveDate,
                                            //*bondIsRecieved = jbbo.isRecieved,
                                            //*agentCompany = jaa.company,
                                            //*shippingCompanyId = C.shippingCompanyId,
                                            //*shippingCompanyName = C.shippingCompanies.name,
                                            /*
                                            pos2Id = C.transType == "d" ? C.cashTransfer2.posId :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().posId
                                            ,
                                            pos2Name = C.transType == "d" ? C.cashTransfer2.pos.name :
                                            entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.name
                                            ,
                                            branchId = C.pos.branchId,
                                            branchName = C.pos.branches.name,
                                            branch2Id = C.transType == "d" ? C.cashTransfer2.pos.branchId :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.branchId
                                           ,
                                            branch2Name = C.transType == "d" ? C.cashTransfer2.pos.branches.name :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.branches.name
                                          ,
                                          */

                                            branchCreatorId = C.pos1.branchId,
                                            branchCreator = C.pos1.branches.name,



                                            fromposId = C.transType == "p" ? C.posId : (C.transType == "d" ? C.cashTransfer2.posId :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().posId
                                            ),
                                            fromposName = C.transType == "p" ? jpp.name : (C.transType == "d" ? C.cashTransfer2.pos.name :
                                            entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.name
                                           ),
                                            frombranchId = C.transType == "p" ? C.pos.branchId : C.transType == "d" ? C.cashTransfer2.pos.branchId :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.branchId
                                           ,
                                            frombranchName = C.transType == "p" ? C.pos.branches.name : C.transType == "d" ? C.cashTransfer2.pos.branches.name :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.branches.name
                                          ,





                                            toposId = C.transType == "d" ? C.posId : (C.transType == "d" ? C.cashTransfer2.posId :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().posId
                                            ),
                                            toposName = C.transType == "d" ? jpp.name : (C.transType == "d" ? C.cashTransfer2.pos.name :
                                            entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.name
                                           ),
                                            tobranchId = C.transType == "d" ? C.pos.branchId : C.transType == "d" ? C.cashTransfer2.pos.branchId :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.branchId
                                           ,
                                            tobranchName = C.transType == "d" ? C.pos.branches.name : C.transType == "d" ? C.cashTransfer2.pos.branches.name :
                                             entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().pos.branches.name
                                          ,

                                        }).ToList();

                        var list = cachlist.GroupBy(g => g.cashopr).SelectMany(grouping => grouping.Take(1)).ToList();


                        return TokenManager.GenerateToken(list);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        // حركات ال POS
        [HttpPost]
        [Route("GetBytypeAndSideForPos")]
        public string GetBytypeAndSideForPos(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string type = "";
                string side = "";

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "type")
                    {
                        type = c.Value;
                    }
                    else if (c.Type == "side")
                    {
                        side = c.Value;
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<CashTransferModel> cachlist = (from C in entity.cashTransfer
                                                                //join b in entity.banks on C.bankId equals b.bankId into jb
                                                                //join a in entity.agents on C.agentId equals a.agentId into ja
                                                            join p in entity.pos on C.posId equals p.posId into jp
                                                            join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                            join u in entity.users on C.userId equals u.userId into ju
                                                            join uc in entity.users on C.createUserId equals uc.userId into juc
                                                            join uu in entity.users on C.createUserId equals uu.userId into jup

                                                            //join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                            //join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                            //join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                            //from jbb in jb.DefaultIfEmpty()
                                                            //from jaa in ja.DefaultIfEmpty()
                                                            from jpp in jp.DefaultIfEmpty()
                                                            from juu in ju.DefaultIfEmpty()
                                                            from jpcc in jpcr.DefaultIfEmpty()
                                                            from jucc in juc.DefaultIfEmpty()
                                                            from jupdateusr in jup.DefaultIfEmpty()

                                                                //from jcrd in jcr.DefaultIfEmpty()
                                                                //from jbbo in jbo.DefaultIfEmpty()
                                                                //from jssh in jsh.DefaultIfEmpty()
                                                            select new CashTransferModel()
                                                            {
                                                                cashTransId = C.cashTransId,
                                                                transType = C.transType,
                                                                posId = C.posId,
                                                                userId = C.userId,
                                                                agentId = C.agentId,
                                                                invId = C.invId,
                                                                transNum = C.transNum,
                                                                createDate = C.createDate,
                                                                updateDate = C.updateDate,
                                                                cash = C.cash,
                                                                updateUserId = C.updateUserId,
                                                                createUserId = C.createUserId,
                                                                notes = C.notes,
                                                                posIdCreator = C.posIdCreator,
                                                                isConfirm = C.isConfirm,
                                                                cashTransIdSource = C.cashTransIdSource,
                                                                side = C.side,

                                                                docName = C.docName,
                                                                docNum = C.docNum,
                                                                docImage = C.docImage,
                                                                bankId = C.bankId,
                                                                //bankName = jbb.name,
                                                                //agentName = jaa.name,
                                                                usersName = juu.name,// side =u

                                                                posName = jpp.name,
                                                                posCreatorName = jpcc.name,
                                                                processType = C.processType,
                                                                cardId = C.cardId,
                                                                bondId = C.bondId,
                                                                usersLName = juu.lastname,// side =u
                                                                createUserName = jucc.name,
                                                                createUserLName = jucc.lastname,
                                                                createUserJob = jucc.job,
                                                                //cardName = jcrd.name,
                                                                //bondDeserveDate = jbbo.deserveDate,
                                                                //  bondIsRecieved = jbbo.isRecieved,
                                                                shippingCompanyId = C.shippingCompanyId,
                                                                //shippingCompanyName = jssh.name,
                                                                branchCreatorId = jpcc.branchId,
                                                                branchCreatorname = jpcc.branches.name,
                                                                branchId = jpp.branchId,
                                                                branchName = jpp.branches.name,
                                                                branch2Id = 0,
                                                                branch2Name = "",
                                                                updateUserAcc = jupdateusr.username,

                                                            }).Where(C => ((type == "all") ? true : C.transType == type) && (C.processType != "balance")
                && ((side == "all") ? true : C.side == side)).ToList();

                        BranchesController branchCntrlr = new BranchesController();


                        if (cachlist.Count > 0 && side == "p")
                        {
                            branches branchmodel = new branches();

                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;

                                branchmodel = branchCntrlr.GetBranchByPosId(cashtItem.pos2Id);
                                cashtItem.branch2Id = branchmodel.branchId;
                                cashtItem.branch2Name = branchmodel.name;

                            }

                        }


                        return TokenManager.GenerateToken(cachlist);


                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        //كشف حساب
        [HttpPost]
        [Route("GetStatement")]
        public string GetStatement(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //int mainBranchId = 0;
                //int userId = 0;

                //IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                //foreach (Claim c in claims)
                //{
                //    if (c.Type == "mainBranchId")
                //    {
                //        mainBranchId = int.Parse(c.Value);
                //    }
                //    else if (c.Type == "userId")
                //    {
                //        userId = int.Parse(c.Value);
                //    }

                //}
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    //List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var cachlist = (from C in entity.cashTransfer
                                        join b in entity.banks on C.bankId equals b.bankId into jb
                                        join a in entity.agents on C.agentId equals a.agentId into ja
                                        join p in entity.pos on C.posId equals p.posId into jp
                                        join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                        join u in entity.users on C.userId equals u.userId into ju
                                        join uc in entity.users on C.updateUserId equals uc.userId into juc
                                        join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                        join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                        join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                        join inv in entity.invoices on C.invId equals inv.invoiceId into jinv//yasmine
                                        from jbb in jb.DefaultIfEmpty()
                                        from jaa in ja.DefaultIfEmpty()
                                        from jpp in jp.DefaultIfEmpty()
                                        from juu in ju.DefaultIfEmpty()
                                        from jpcc in jpcr.DefaultIfEmpty()
                                        from jucc in juc.DefaultIfEmpty()
                                        from jcrd in jcr.DefaultIfEmpty()
                                        from jbbo in jbo.DefaultIfEmpty()
                                        from jshh in jsh.DefaultIfEmpty()
                                        from jinvv in jinv.DefaultIfEmpty()//yasmine
                                        where (C.processType != "balance" && (C.side == "c" || C.side == "v" || C.side == "b" || C.side == "u" || C.side == "sh" || C.side == "bnd" || C.side == "mb"))//( C.transType == "p" && C.side==Side)

                                        //&& (brIds.Contains(jpp.branches.branchId) || brIds.Contains(jpcc.branches.branchId))

                                        select new
                                        {
                                            cashTransId = C.cashTransId,
                                            transType = C.transType,
                                            posId = C.posId,
                                            userId = C.userId,
                                            agentId = C.agentId,
                                            invId = C.invId,
                                            transNum = C.transNum,
                                            //*createDate = C.createDate,
                                            updateDate = C.updateDate,
                                            cash = C.cash,
                                            updateUserId = C.updateUserId,
                                            //*createUserId = C.createUserId,
                                            //*notes = C.notes,
                                            //*posIdCreator = C.posIdCreator,
                                            isConfirm = (byte?)C.isConfirm,/////////////////////////
                                            cashTransIdSource = C.cashTransIdSource,
                                            side = C.side,

                                            docName = C.docName,
                                            docNum = C.docNum,
                                            //*docImage = C.docImage,
                                            //*bankId = C.bankId,
                                            //*bankName = jbb.name,
                                            agentName = jaa.name,

                                            usersName = juu.name,// side =u
                                            userAcc = juu.username,// side =u
                                            posName = jpp.name,
                                            //*posCreatorName = jpcc.name,
                                            processType = C.processType,
                                            cardId = C.cardId,
                                            bondId = C.bondId,
                                            usersLName = juu.lastname,// side =u
                                            updateUserName = jucc.name,
                                            updateUserLName = jucc.lastname,
                                            updateUserAcc = jucc.username,
                                            //*createUserJob = jucc.job,
                                            cardName = jcrd.name,
                                            //*bondDeserveDate = jbbo.deserveDate,
                                            bondIsRecieved = (byte?)jbbo.isRecieved,////////////////////////

                                            //*agentCompany = jaa.company,
                                            shippingCompanyId = C.shippingCompanyId,
                                            shippingCompanyName = C.shippingCompanies.name,

                                            //*agentBalance = jaa.balance,
                                            agentBType = (byte?)jaa.balanceType,///////////////////////
                                            //*userBalance = juu.balance,
                                            userBType = (byte?)juu.balanceType,/////////////////
                                            shippingBalance = (decimal?)jshh.balance,

                                            shippingCompaniesBType = (byte?)jshh.balanceType,/////////////////

                                            invNumber = jinvv.invNumber,//yasmine
                                            invBarcode =jinvv.invBarcode,
                                            bondNumber = jbbo.number,//yasmine

                                            invShippingCompanyId = jinvv.shippingCompanyId,
                                            // invShippingCompanyName = jinvv.name,
                                            jinvv.shipUserId,

                                            //invAgentId = jinvv.agentId,
                                            //invAgentName = jinvv.agents.name,

                                            invAgentId = C.invId == null ? C.agentId : jinvv.agentId,
                                            invAgentName = C.invId == null ? jaa.name : jinvv.agents.name,

                                            invShippingCompanyName = jinvv.shippingCompanies.name,


                                        }).ToList();
                        /*
                        if (cachlist.Count > 0 )
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {if (cashtItem.side == "p") { }
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;
                                // cashtItem.posCreatorName = tempitem.posName;


                            }

                        }
                        */


                        return TokenManager.GenerateToken(cachlist);

                    }

                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                    //   return TokenManager.GenerateToken("0");
                }

            }


        }



        public List<CashTransferModel> Getpostransmodel(int cashTransId)
        {

            string side = "p";

            using (incposdbEntities entity = new incposdbEntities())
            {

                var cachlist = (from C in entity.cashTransfer
                                join b in entity.banks on C.bankId equals b.bankId into jb
                                join a in entity.agents on C.agentId equals a.agentId into ja
                                join p in entity.pos on C.posId equals p.posId into jp
                                join u in entity.users on C.userId equals u.userId into ju
                                from jbb in jb.DefaultIfEmpty()
                                from jaa in ja.DefaultIfEmpty()
                                from jpp in jp.DefaultIfEmpty()
                                from juu in ju.DefaultIfEmpty()

                                select new CashTransferModel()
                                {
                                    cashTransId = C.cashTransId,
                                    transType = C.transType,
                                    posId = C.posId,
                                    userId = C.userId,
                                    agentId = C.agentId,
                                    invId = C.invId,
                                    transNum = C.transNum,
                                    createDate = C.createDate,
                                    updateDate = C.updateDate,
                                    cash = C.cash,
                                    updateUserId = C.updateUserId,
                                    createUserId = C.createUserId,
                                    notes = C.notes,
                                    posIdCreator = C.posIdCreator,
                                    isConfirm = C.isConfirm,
                                    cashTransIdSource = C.cashTransIdSource,
                                    side = C.side,

                                    docName = C.docName,
                                    docNum = C.docNum,
                                    docImage = C.docImage,
                                    bankId = C.bankId,
                                    bankName = jbb.name,
                                    agentName = jaa.name,
                                    usersName = juu.username,
                                    posName = jpp.name,
                                    processType = C.processType,
                                    cardId = C.cardId,
                                    bondId = C.bondId,
                                }).Where(C => (C.side == "p") && (C.cashTransId == cashTransId || C.cashTransIdSource == cashTransId)).ToList();


                // one row mean type=d
                if (cachlist.Count == 1)
                {
                    int? pullposcashtransid = cachlist.First().cashTransIdSource;

                    //
                    var cachadd = (from C in entity.cashTransfer
                                   join b in entity.banks on C.bankId equals b.bankId into jb
                                   join a in entity.agents on C.agentId equals a.agentId into ja
                                   join p in entity.pos on C.posId equals p.posId into jp
                                   join u in entity.users on C.userId equals u.userId into ju
                                   from jbb in jb.DefaultIfEmpty()
                                   from jaa in ja.DefaultIfEmpty()
                                   from jpp in jp.DefaultIfEmpty()
                                   from juu in ju.DefaultIfEmpty()

                                   select new CashTransferModel()
                                   {
                                       cashTransId = C.cashTransId,
                                       transType = C.transType,
                                       posId = C.posId,
                                       userId = C.userId,
                                       agentId = C.agentId,
                                       invId = C.invId,
                                       transNum = C.transNum,
                                       createDate = C.createDate,
                                       updateDate = C.updateDate,
                                       cash = C.cash,
                                       updateUserId = C.updateUserId,
                                       createUserId = C.createUserId,
                                       notes = C.notes,
                                       posIdCreator = C.posIdCreator,
                                       isConfirm = C.isConfirm,
                                       cashTransIdSource = C.cashTransIdSource,
                                       side = C.side,

                                       docName = C.docName,
                                       docNum = C.docNum,
                                       docImage = C.docImage,
                                       bankId = C.bankId,
                                       bankName = jbb.name,
                                       agentName = jaa.name,
                                       usersName = juu.username,
                                       posName = jpp.name,
                                       processType = C.processType,
                                       cardId = C.cardId,
                                       bondId = C.bondId,
                                   }).Where(C => ((side == "all") ? true : C.side == side) && (C.cashTransId == pullposcashtransid)).ToList();

                    //

                    if (cachadd.Count > 0)
                    {
                        cachlist.AddRange(cachadd);

                    }

                }

                return cachlist;
            }
        }


        // يومية الصندوق
        [HttpPost]
        [Route("GetBalance")]
        public string GetBalance(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var cachlist = (from p in entity.pos
                                        join b in entity.branches on p.branchId equals b.branchId


                                        //  from jbbo in jbo.DefaultIfEmpty()

                                        where (brIds.Contains(b.branchId))
                                        select new
                                        {

                                            p.posId,
                                            posName = p.name,
                                            posIsActive = p.isActive,
                                            posCode = p.code,
                                            p.balance,
                                            branchName = b.name,
                                            b.branchId,
                                            branchType = b.type,
                                            branchCode = b.code,
                                            banchIsActive = b.isActive
                                        }).ToList();






                        return TokenManager.GenerateToken(cachlist);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }


        //  الضريبة
        //  الضريبة حساب ضريبة العناصر والفواتير
        [HttpPost]
        [Route("GetInvItemTax")]
        public string GetInvItemTax(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;
                InvoicesController invoice = new InvoicesController();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<ItemTransferInvoiceTax> invListm = (from IT in entity.itemsTransfer
                                                                 from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                                                 from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                                                     //  join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                                                 join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                                                 join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                                                 join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                                                 //    join B in entity.branches on I.branchId equals B.branchId into JB
                                                                 join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                                                 join A in entity.agents on I.agentId equals A.agentId into JA
                                                                 // join U in entity.users on I.createUserId equals U.userId into JU
                                                                 join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                                                 // join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                                                 join P in entity.pos on I.posId equals P.posId into JP
                                                                 join JOF in entity.offers on IT.offerId equals JOF.offerId into JO
                                                                 // from JBB in JB
                                                                 from JPP in JP.DefaultIfEmpty()
                                                                     // from JUU in JU.DefaultIfEmpty()
                                                                 from JUPUS in JUPUSR.DefaultIfEmpty()
                                                                     // from JIMM in JIM.DefaultIfEmpty()
                                                                 from JAA in JA.DefaultIfEmpty()
                                                                 from JBCC in JBC.DefaultIfEmpty()
                                                                 from O in JO.DefaultIfEmpty()
                                                                 where (brIds.Contains(JBCC.branchId)) && (I.invType == "s")

                                                                 select new ItemTransferInvoiceTax
                                                                 {

                                                                     ITitemName = ITEM.name,
                                                                     ITunitName = UNIT.name,
                                                                     ITitemsTransId = IT.itemsTransId,
                                                                     ITitemUnitId = IT.itemUnitId,

                                                                     ITitemId = IU.itemId,
                                                                     ITunitId = IU.unitId,
                                                                     ITquantity = IT.quantity,
                                                                     //avgPurchasePrice = ITEM.avgPurchasePrice,
                                                                     // ITcreateDate = IT.createDate,
                                                                     //ITupdateDate = IT.updateDate,
                                                                     //  ITcreateUserId = IT.createUserId,
                                                                     //ITupdateUserId = IT.updateUserId,
                                                                     // ITnotes = IT.notes,
                                                                     ITprice = IT.price,//no tax
                                                                     ITbarcode = IU.barcode,
                                                                     //  ITCreateuserName = ITCUSER.name,
                                                                     // ITCreateuserLName = ITCUSER.lastname,
                                                                     //  ITCreateuserAccName = ITCUSER.username,


                                                                     invoiceId = I.invoiceId,
                                                                     invNumber = I.invNumber,
                                                                     invBarcode = I.invBarcode,
                                                                     agentId = I.agentId,
                                                                     posId = I.posId,
                                                                     invType = I.invType,
                                                                     total = I.total - I.shippingCost,
                                                                     branchName = I.pos.branches.name,

                                                                     //  I.updateUserId,
                                                                     //  I.paid,
                                                                     // I.deserved,
                                                                     //I.deservedDate,
                                                                     invDate= I.invDate,
                                                                     //  I.invoiceMainId,
                                                                     // I.invCase,
                                                                     //  I.invTime,
                                                                     // I.notes,
                                                                     //  I.vendorInvNum,
                                                                     // I.vendorInvDate,
                                                                     // I.createUserId,
                                                                     updateDate = I.updateDate,
                                                                     updateUserId = I.updateUserId,
                                                                     branchId = I.branchId,
                                                                     //calc coupon + manual discount
                                                                     discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),

                                                                     discountType = "1",
                                                                     tax = I.tax,
                                                                     //  I.name,
                                                                     // I.isApproved,

                                                                     //
                                                                     //branchCreatorId = I.branchCreatorId,
                                                                     //branchCreatorName = JBCC.name,
                                                                     //
                                                                     //  branchName = JBB.name,

                                                                     //  branchType = JBB.type,
                                                                     posName = JPP.name,
                                                                     posCode = JPP.code,
                                                                     agentName = JAA.name,
                                                                     agentCode = JAA.code,
                                                                     agentType = JAA.type,
                                                                     //  cuserName = JUU.name,
                                                                     //  cuserLast = JUU.lastname,
                                                                     // cUserAccName = JUU.username,
                                                                     uuserName = JUPUS.name,
                                                                     uuserLast = JUPUS.lastname,
                                                                     uUserAccName = JUPUS.username,
                                                                     agentCompany = JAA.company,
                                                                     //   subTotal = ((IT.price - (ITEM.taxes * IU.price / 100)) * IT.quantity),
                                                                     //   subTotalNet = ((IT.price - (ITEM.taxes * IU.price / 100)) * IT.quantity),

                                                                     //  itemUnitTax = (ITEM.taxes * IU.price / 100),//1
                                                                     //  itemTaxValue= (ITEM.taxes * IU.price / 100) * IT.quantity,//n

                                                                     //subTotalTax = IT.price * IT.quantity,
                                                                     // subTotalNotax = (IT.price * IT.quantity)- (ITEM.taxes * IU.price / 100) * IT.quantity,

                                                                     totalNet = I.totalNet,//calc in clint

                                                                     OneItemOfferVal = IT.offerId == null ? 0 : ((IT.offerType == 1 || IT.offerType == null) ? (IT.offerValue) : (IT.offerType == 2 ? ((IT.offerValue / 100) * (IT.itemUnitPrice)) : 0)),

                                                                     //  offerTotalValue = (O.discountType == "1" || O.discountType == null) ? (O.discountValue * (IT.quantity)) : (O.discountType == "2" ? ((O.discountValue / 100) * (IT.price * IT.quantity)) : 0),

                                                                     itemUnitPrice = IT.itemUnitPrice,
                                                                     ItemTaxes = IT.itemTax,
                                                                     //shippingCost = I.shippingCost,
                                                                     //realShippingCost = I.realShippingCost,
                                                                     //shippingProfit = I.shippingCost - I.realShippingCost,
                                                                     //totalNetNoShip = (decimal)I.totalNet - I.shippingCost,
                                                                     //totalNoShip = (decimal)I.total - I.shippingCost,
                                                                     //(ITEM.taxes *IU.price/100) = tax value
                                                                     //username

                                                                     //  I.invoiceId,
                                                                     //    JBB.name
                                                                 }).ToList();

                        Calculate calc = new Calculate();

                        foreach (ItemTransferInvoiceTax row in invListm)
                        {
                            // invoice tax
                            row.totalNoTax = row.total - row.discountValue;

                            row.invTaxVal = calc.percentValue(row.totalNoTax, row.tax);
                            row.totalwithTax = row.totalNoTax + row.invTaxVal;//=totalNet

                            //item tax
                            row.OneItemPriceNoTax = row.itemUnitPrice - row.OneItemOfferVal;
                            row.OneitemUnitTax = calc.percentValue(row.OneItemPriceNoTax, row.ItemTaxes);//قيمة الضريبة للعنصر الواحد
                            row.OneItemPricewithTax = row.OneItemPriceNoTax + row.OneitemUnitTax;

                            row.itemUnitTaxwithQTY = row.OneitemUnitTax * row.ITquantity;
                            row.subTotalNotax = row.OneItemPriceNoTax * row.ITquantity;//قبل الضريبة
                                                                                       // row.subTotalTax = row.OneItemPricewithTax * row.ITquantity;//
                            row.subTotalTax = row.ITprice * row.ITquantity;//
                            row.itemsRowsCount = invListm.Where(x => x.invoiceId == row.invoiceId).ToList().Count();


                            //item tax

                        }




                        //List<ItemTransferInvoiceTax> listtaxgroup = new List<ItemTransferInvoiceTax>();
                        //listtaxgroup = invListm.GroupBy(g => g.invoiceId).Select(g => new ItemTransferInvoiceTax
                        //{
                        //    invoiceId=g.FirstOrDefault().invoiceId,
                        //    invNumber= g.FirstOrDefault().invNumber,
                        //    agentId = g.FirstOrDefault().agentId,
                        //    posId = g.FirstOrDefault().posId,
                        //    invType = g.FirstOrDefault().invType,
                        //    branchName = g.FirstOrDefault().branchName,
                        //    updateDate = g.FirstOrDefault().updateDate,
                        //    branchId = g.FirstOrDefault().branchId,
                        //    posName = g.FirstOrDefault().posName,
                        //    posCode = g.FirstOrDefault().posCode,
                        //    totalNet= g.FirstOrDefault().totalNet,


                        //}).ToList();
                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch (Exception ex)
                {

                    return TokenManager.GenerateToken(ex.ToString());
                    // return TokenManager.GenerateToken("0");
                }

            }

        }


        //عمليات الفتح والاغلاق للصندوق
        [HttpPost]
        [Route("GetPosCashOpenClose")]
        public string GetPosCashOpenClose(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {

                int mainBranchId = 0;
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }


                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    List<POSOpenCloseModel> cachlist = new List<POSOpenCloseModel>();
                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        cachlist = (from C in entity.cashTransfer

                                    join p in entity.pos on C.posId equals p.posId into jp

                                    from jpp in jp.DefaultIfEmpty()
                                    select new POSOpenCloseModel()
                                    {
                                        cashTransId = C.cashTransId,
                                        transType = C.transType,
                                        posId = C.posId,
                                        updateDate = C.updateDate,
                                        transNum = C.transNum,

                                        cash = C.cash,

                                        notes = C.notes,

                                        isConfirm = C.isConfirm,
                                        cashTransIdSource = C.cashTransIdSource,
                                        side = C.side,

                                        posName = jpp.name,

                                        processType = C.processType,

                                        branchId = jpp.branchId,
                                        branchName = jpp.branches.name,
                                        openDate = null,
                                        openCash = null,
                                        //  }).Where(C => (C.transType == "c" || C.transType == "o")  ).ToList();
                                    }).Where(C => (C.transType == "c" || C.transType == "o") && (brIds.Contains((int)C.branchId))).ToList();

                        //  List<POSOpenCloseModel> closelist = cachlist.Where(C => C.transType == "c").ToList();



                        //   branchmodel = branchCntrlr.GetBranchByPosId(cashtItem.pos2Id);

                    }
                    POSOpenCloseModel openrow = new POSOpenCloseModel();
                    List<POSOpenCloseModel> tmplist = new List<POSOpenCloseModel>();
                    foreach (POSOpenCloseModel row in cachlist)
                    {
                        if (row.transType == "c")
                        {


                            openrow = new POSOpenCloseModel();
                            //   openrow = cachlist.Where(C => C.posId == row.posId && C.transNum == row.transNum && C.transType == "o").FirstOrDefault();

                            tmplist = cachlist.Where(X => X.posId == row.posId && X.transNum == row.transNum && X.transType == "o").ToList();
                            //tmplist = tmplist.Where(X => X.transType.ToString() == "o").ToList();
                            if (tmplist != null && tmplist.Count() > 0)
                            {
                                openrow = tmplist.FirstOrDefault();
                                if (openrow.cashTransId > 0)
                                {
                                    row.openDate = openrow.updateDate;
                                    row.openCash = openrow.cash;
                                    row.openCashTransId = openrow.cashTransId;
                                }
                            }

                        }

                        //  row.openDate=
                    }

                    List<POSOpenCloseModel> closelist = new List<POSOpenCloseModel>();
                    closelist = cachlist.Where(X => X.transType == "c" && X.openCashTransId != null && X.openCashTransId > 0).ToList();
                    return TokenManager.GenerateToken(closelist);
                }
                catch (Exception ex)
                {
                    List<POSOpenCloseModel> tmplis = new List<POSOpenCloseModel>();
                    POSOpenCloseModel enrow = new POSOpenCloseModel();
                    enrow.posName = ex.ToString();
                    tmplis.Add(enrow);

                    return TokenManager.GenerateToken(tmplis);
                    //  return TokenManager.GenerateToken("0");
                }

            }


        }


        //العمليات المنفذة بين تاريخ الفتح والاغلاق
        [HttpPost]
        [Route("GetTransBetweenOpenClose")]
        public string GetTransBetweenOpenClose(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int openCashTransId = 0;
                int closeCashTransId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "openCashTransId")//openid
                    {
                        openCashTransId = int.Parse(c.Value);
                    }
                    else if (c.Type == "closeCashTransId")//closeid
                    {
                        closeCashTransId = int.Parse(c.Value);
                    }

                }

                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<cashTransfer> allcashlist = entity.cashTransfer.ToList();

                        cashTransfer closrow = allcashlist.Where(X => X.cashTransId == closeCashTransId).FirstOrDefault();
                        cashTransfer openrow = allcashlist.Where(X => X.cashTransId == openCashTransId).FirstOrDefault();

                        List<OpenClosOperatinModel> cachlist = (from C in entity.cashTransfer
                                                                join b in entity.banks on C.bankId equals b.bankId into jb
                                                                join a in entity.agents on C.agentId equals a.agentId into ja
                                                                join p in entity.pos on C.posId equals p.posId into jp
                                                                join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                                                join u in entity.users on C.userId equals u.userId into ju
                                                                //   join uc in entity.users on C.createUserId equals uc.userId into juc
                                                                join uu in entity.users on C.createUserId equals uu.userId into jup

                                                                join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                                                join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                                                join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                                                from jbb in jb.DefaultIfEmpty()
                                                                from jaa in ja.DefaultIfEmpty()
                                                                from jpp in jp.DefaultIfEmpty()
                                                                from juu in ju.DefaultIfEmpty()
                                                                from jpcc in jpcr.DefaultIfEmpty()
                                                                    // from jucc in juc.DefaultIfEmpty()
                                                                from jupdateusr in jup.DefaultIfEmpty()

                                                                from jcrd in jcr.DefaultIfEmpty()
                                                                from jbbo in jbo.DefaultIfEmpty()
                                                                from jssh in jsh.DefaultIfEmpty()
                                                                select new OpenClosOperatinModel()
                                                                {
                                                                    cashTransId = C.cashTransId,
                                                                    transType = C.transType,
                                                                    posId = C.posId,
                                                                    userId = C.userId,
                                                                    agentId = C.agentId,
                                                                    invId = C.invId,
                                                                    transNum = C.transNum,
                                                                    createDate = C.createDate,
                                                                    updateDate = C.updateDate,
                                                                    cash = C.cash,
                                                                    updateUserId = C.updateUserId,
                                                                    createUserId = C.createUserId,
                                                                    notes = C.notes,
                                                                    posIdCreator = C.posIdCreator,
                                                                    isConfirm = C.isConfirm,
                                                                    cashTransIdSource = C.cashTransIdSource,
                                                                    side = C.side,

                                                                    docName = C.docName,
                                                                    docNum = C.docNum,
                                                                    docImage = C.docImage,
                                                                    bankId = C.bankId,
                                                                    bankName = jbb.name,
                                                                    agentName = jaa.name,
                                                                    usersName = juu.name,// side =u

                                                                    posName = jpp.name,
                                                                    posCreatorName = jpcc.name,
                                                                    processType = C.processType,
                                                                    cardId = C.cardId,
                                                                    bondId = C.bondId,
                                                                    usersLName = juu.lastname,// side =u
                                                                                              //createUserName = jucc.name,
                                                                                              //createUserLName = jucc.lastname,
                                                                                              //createUserJob = jucc.job,
                                                                    cardName = jcrd.name,
                                                                    bondDeserveDate = jbbo.deserveDate,
                                                                    bondIsRecieved = jbbo.isRecieved,
                                                                    shippingCompanyId = C.shippingCompanyId,
                                                                    shippingCompanyName = jssh.name,
                                                                    branchCreatorId = jpcc.branchId,
                                                                    branchCreatorname = jpcc.branches.name,
                                                                    branchId = jpp.branchId,
                                                                    branchName = jpp.branches.name,
                                                                    branch2Id = 0,
                                                                    branch2Name = "",
                                                                    updateUserAcc = jupdateusr.username,

                                                                }).Where(C => (C.cashTransId == openCashTransId || C.cashTransId == closeCashTransId) ||

                                                              (C.transType != "o" && C.transType != "c"
                                                                && C.processType != "balance" && C.processType != "box" &&
                                                                C.processType != "inv" && C.processType != "card" && C.processType != "cheque" && C.processType != "doc"
                                                                && C.posId == closrow.posId
                                                                && (C.side == "bn" ? C.isConfirm == 1 : true)
                                                                && C.updateDate >= openrow.updateDate && C.updateDate <= closrow.updateDate)
                                                            ).OrderBy(X => X.updateDate).ToList();


                        BranchesController branchCntrlr = new BranchesController();

                        if (cachlist.Count > 0)
                        {
                            branches branchmodel = new branches();

                            CashTransferModel tempitem = null;
                            foreach (OpenClosOperatinModel cashtItem in cachlist)
                            {
                                if (cashtItem.side == "p")
                                {
                                    tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                        .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                    cashtItem.cashTrans2Id = tempitem.cashTransId;
                                    cashtItem.pos2Id = tempitem.posId;
                                    cashtItem.pos2Name = tempitem.posName;
                                    cashtItem.isConfirm2 = tempitem.isConfirm;

                                    branchmodel = branchCntrlr.GetBranchByPosId(cashtItem.pos2Id);
                                    cashtItem.branch2Id = branchmodel.branchId;
                                    cashtItem.branch2Name = branchmodel.name;
                                }

                            }

                        }
                        cachlist = cachlist.Where(X => X.side == "p" ? (X.isConfirm == 1 && X.isConfirm2 == 1) : true).ToList();

                        return TokenManager.GenerateToken(cachlist);


                    }
                }
                catch (Exception ex)
                {
                    List<OpenClosOperatinModel> tmplis = new List<OpenClosOperatinModel>();
                    OpenClosOperatinModel enrow = new OpenClosOperatinModel();
                    enrow.posName = ex.ToString();
                    tmplis.Add(enrow);

                    return TokenManager.GenerateToken(tmplis);
                    //  return TokenManager.GenerateToken("0");
                }

            }


        }



        #endregion

        //اليومية
        #region
        //  يومية الفواتير العامة



        [HttpPost]
        [Route("Getdailyinvoice")]
        public string Getdailyinvoice(string token)//,DateTime? date
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId(int mainBranchId, int userId, DateTime? date



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;
                DateTime? date = new DateTime?();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "date")
                    {
                        date = DateTime.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    DateTime dt = Convert.ToDateTime(DateTime.Today.AddDays(-2).ToShortDateString());
                    DateTime dt1 = Convert.ToDateTime(DateTime.Today.AddDays(-1).ToShortDateString());
                    Calculate calc = new Calculate();
                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //  join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "sd" || I.invType == "s"
                                        || I.invType == "sbd" || I.invType == "sd" || I.invType == "ord" || I.invType == "or"
                                        || I.invType == "q" || I.invType == "qd" || I.invType == "ts" || I.invType == "ss")
                                        // && calc.changeDateformat(I.updateDate, "yyyy-MM-dd")== calc.changeDateformat(date, "yyyy-MM-dd")
                                        //&& DateTime.Compare((DateTime)IO.startDate, DateTime.Now) <= 0
                                        //    && DateTime.Compare((DateTime)calc.changeDateformat(I.updateDate, "yyyy-MM-dd"), (DateTime)calc.changeDateformat(date, "yyyy-MM-dd")) >= 0

                                        select new
                                        {


                                            //  Convert.ToDateTime()
                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                           I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                           discountType="1",
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            // branchName = JBB.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            agentCode = JAA.code,
                                            //
                                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb")) ?
                                            "unknown" : JAA.name,


                                            //   agentType = JAA.type,
                                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
                                            ? "c" : JAA.type,
                                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
                                            ? 0 : I.agentId,


                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb")) ?
                                            "unknown" : JAA.company,

                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                            archived = ((DateTime)I.updateDate >=
                                            ((I.invType == "sd" || I.invType == "sbd" || I.invType == "sd" || I.invType == "ord" || I.invType == "qd") ? dt : dt1)) ? 0 : 1,
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                            processType = entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").ToList().Count() > 0 ?
                                            entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").ToList().Count() > 1
                                            ? "multiple" :
                                            entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").FirstOrDefault().processType == "card" ?
                                             entity.cards.Where(C => C.cardId == entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").FirstOrDefault().cardId).FirstOrDefault().name
                                             : entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").FirstOrDefault().processType
                                             : "balance",
                                             I.shippingCompanyId,
                                             I.shipUserId,
                                            shippingCompanyName=   I.shippingCompanies.name,
                                            shipUserName=  I.users4.name,
                                            shipUserLastName= I.users4.lastname,



                                        }).ToList();

                        invListm = invListm.Where(X => DateTime.Compare((DateTime)calc.changeDateformat(X.updateDate, "yyyy-MM-dd"), (DateTime)calc.changeDateformat(date, "yyyy-MM-dd")) == 0).ToList();






                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        // يومية فواتير المشتريات العامة في قسم التقارير
        [HttpPost]
        [Route("GetPurdailyinvoice")]
        public string GetPurdailyinvoice(string token)//,DateTime? date
        {
            // public ResponseVM GetPurinv(string token)(int mainBranchId, int userId, DateTime? date)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;
                DateTime? date = new DateTime?();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                    else if (c.Type == "date")
                    {
                        date = DateTime.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {
                    Calculate calc = new Calculate();
                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var invListm = (from I in entity.invoices

                                            //  join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId) || brIds.Contains(JPP.branches.branchId)) &&
                                        (I.invType == "pw" || I.invType == "pd"
                                        || I.invType == "p" || I.invType == "pbd" || I.invType == "pbw" || I.invType == "pb"
                                        || I.invType == "pod" || I.invType == "po")
                                        // && calc.changeDateformat(I.updateDate, "yyyy-MM-dd")== calc.changeDateformat(date, "yyyy-MM-dd")
                                        //&& DateTime.Compare((DateTime)IO.startDate, DateTime.Now) <= 0
                                        //    && DateTime.Compare((DateTime)calc.changeDateformat(I.updateDate, "yyyy-MM-dd"), (DateTime)calc.changeDateformat(date, "yyyy-MM-dd")) >= 0

                                        select new
                                        {


                                            //  Convert.ToDateTime()
                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                             I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            I.discountType,
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            // branchName = JBB.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            agentCode = JAA.code,
                                            //
                                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb")) ?
                                            "unknown" : JAA.name,


                                            //   agentType = JAA.type,
                                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
                                            ? "c" : JAA.type,
                                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
                                            ? 0 : I.agentId,


                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb")) ?
                                            "unknown" : JAA.company,

                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();

                        invListm = invListm.Where(X => DateTime.Compare((DateTime)calc.changeDateformat(X.updateDate, "yyyy-MM-dd"), (DateTime)calc.changeDateformat(date, "yyyy-MM-dd")) == 0).ToList();





                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

        }

        //  يومية الفواتير الخاصة بمستخدم
        [HttpPost]
        [Route("GetUserdailyinvoice")]
        public string GetUserdailyinvoice(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var invListm = (from I in entity.invoices

                                            //  join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                        join A in entity.agents on I.agentId equals A.agentId into JA
                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JIMM in JIM.DefaultIfEmpty()
                                        from JAA in JA.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                            // where (JUPUS.userId == userId)
                                        where (
                                      //  (brIds.Contains(JBCC.branchId) || brIds.Contains(JPP.branches.branchId)) &&
                                        JUU.userId== userId &&
                                        I.invType=="s" || I.invType == "ss" || I.invType == "ts"
                                        || I.invType == "ssd" || I.invType == "sd" || I.invType == "tsd" || I.invType == "sb" || I.invType == "sbd")
                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                            I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            I.total,
                                            I.totalNet,
                                            I.paid,
                                            I.deserved,
                                            I.deservedDate,
                                            I.invDate,
                                            I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),
                                            discountType="1",
                                            I.tax,
                                            I.name,
                                            I.isApproved,

                                            //
                                            I.branchCreatorId,
                                            branchCreatorName = JBCC.name,
                                            //
                                            // branchName = JBB.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            agentCode = JAA.code,
                                            //
                                            agentName = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb")) ?
                                            "unknown" : JAA.name,


                                            //   agentType = JAA.type,
                                            agentType = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
                                            ? "c" : JAA.type,
                                            agentId = ((JAA.name == null || JAA.name == "") && (I.invType == "s" || I.invType == "sb"))
                                            ? 0 : I.agentId,


                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            agentCompany = ((JAA.company == null || JAA.company == "") && (I.invType == "s" || I.invType == "sb")) ?
                                            "unknown" : JAA.company,
                                        
                                            processType = entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").ToList().Count() > 0 ?
                                            entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").ToList().Count() > 1
                                            ? "multiple" :
                                            entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").FirstOrDefault().processType == "card" ?
                                             entity.cards.Where(C => C.cardId == entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").FirstOrDefault().cardId).FirstOrDefault().name
                                             : entity.cashTransfer.Where(x => x.invId == I.invoiceId && x.processType != "inv").FirstOrDefault().processType
                                             : "balance",
                                            I.shippingCompanyId,
                                            I.shipUserId,
                                            shippingCompanyName = I.shippingCompanies.name,
                                            shipUserName = I.users4.name,
                                            shipUserLastName = I.users4.lastname,
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        // يومية الصندوق
        [HttpPost]
        [Route("GetDailyStatement")]
        public string GetDailyStatement(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var cachlist = (from C in entity.cashTransfer
                                        join b in entity.banks on C.bankId equals b.bankId into jb
                                        join a in entity.agents on C.agentId equals a.agentId into ja
                                        join p in entity.pos on C.posId equals p.posId into jp
                                        join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                        join u in entity.users on C.userId equals u.userId into ju
                                        join uc in entity.users on C.updateUserId equals uc.userId into juc
                                        join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                        join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                        join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                        from jbb in jb.DefaultIfEmpty()
                                        from jaa in ja.DefaultIfEmpty()
                                        from jpp in jp.DefaultIfEmpty()
                                        from juu in ju.DefaultIfEmpty()
                                        from jpcc in jpcr.DefaultIfEmpty()
                                        from jucc in juc.DefaultIfEmpty()
                                        from jcrd in jcr.DefaultIfEmpty()
                                        from jbbo in jbo.DefaultIfEmpty()
                                        from jshh in jsh.DefaultIfEmpty()
                                        where (C.processType == "cash" ||
                                     (C.isConfirm == 1 && C.side == "p"
                                       && (C.transType == "d" ?
                                       entity.cashTransfer.Where(x2 => x2.cashTransId == (int)C.cashTransIdSource).FirstOrDefault().isConfirm == 1 :
                                       entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().isConfirm == 1
                                       ))) && (brIds.Contains(jpp.branches.branchId) || brIds.Contains(jpcc.branches.branchId))
                                        select new
                                        {
                                            cashTransId = C.cashTransId,
                                            transType = C.transType,
                                            posId = C.posId,
                                            userId = C.userId,
                                            agentId = C.agentId,
                                            invId = C.invId,
                                            transNum = C.transNum,
                                            createDate = C.createDate,
                                            updateDate = C.updateDate,
                                            cash = C.cash,
                                            updateUserId = C.updateUserId,
                                            createUserId = C.createUserId,
                                            notes = C.notes,
                                            posIdCreator = C.posIdCreator,
                                            isConfirm = C.isConfirm,
                                            cashTransIdSource = C.cashTransIdSource,
                                            side = C.side,

                                            docName = C.docName,
                                            docNum = C.docNum,
                                            docImage = C.docImage,
                                            bankId = C.bankId,
                                            bankName = jbb.name,
                                            agentName = jaa.name,

                                            usersName = juu.name,// side =u
                                            userAcc = juu.username,// side =u
                                            posName = jpp.name,
                                            posCreatorName = jpcc.name,
                                            processType = C.processType,
                                            cardId = C.cardId,
                                            bondId = C.bondId,
                                            usersLName = juu.lastname,// side =u
                                            updateUserName = jucc.name,
                                            updateUserLName = jucc.lastname,
                                            updateUserAcc = jucc.username,
                                            createUserJob = jucc.job,
                                            cardName = jcrd.name,
                                            bondDeserveDate = jbbo.deserveDate,
                                            bondIsRecieved = jbbo.isRecieved,
                                            agentCompany = jaa.company,
                                            shippingCompanyId = C.shippingCompanyId,
                                            shippingCompanyName = C.shippingCompanies.name,

                                            agentBalance = jaa.balance,
                                            agentBType = jaa.balanceType,
                                            userBalance = juu.balance,
                                            userBType = juu.balanceType,
                                            shippingBalance = (decimal?)jshh.balance,

                                            shippingCompaniesBType = jshh.balanceType,
                                            branchName = jpp.branches.name,
                                            jpp.branchId,
                                            posBalance = jpp.balance,
                                        }).ToList();
                        /*
                        if (cachlist.Count > 0 )
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {if (cashtItem.side == "p") { }
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;
                                // cashtItem.posCreatorName = tempitem.posName;


                            }

                        }
                        */




                        return TokenManager.GenerateToken(cachlist);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }

        }


        // يومية الصندوق الخاصة بالمستخدم
        [HttpPost]
        [Route("GetUserDailyStatement")]
        public string GetUserDailyStatement(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var cachlist = (from C in entity.cashTransfer
                                        join b in entity.banks on C.bankId equals b.bankId into jb
                                        join a in entity.agents on C.agentId equals a.agentId into ja
                                        join p in entity.pos on C.posId equals p.posId into jp
                                        join pc in entity.pos on C.posIdCreator equals pc.posId into jpcr
                                        join u in entity.users on C.userId equals u.userId into ju
                                        join uc in entity.users on C.updateUserId equals uc.userId into juc
                                        join cr in entity.cards on C.cardId equals cr.cardId into jcr
                                        join bo in entity.bondes on C.bondId equals bo.bondId into jbo
                                        join sh in entity.shippingCompanies on C.shippingCompanyId equals sh.shippingCompanyId into jsh
                                        from jbb in jb.DefaultIfEmpty()
                                        from jaa in ja.DefaultIfEmpty()
                                        from jpp in jp.DefaultIfEmpty()
                                        from juu in ju.DefaultIfEmpty()
                                        from jpcc in jpcr.DefaultIfEmpty()
                                        from jucc in juc.DefaultIfEmpty()
                                        from jcrd in jcr.DefaultIfEmpty()
                                        from jbbo in jbo.DefaultIfEmpty()
                                        from jshh in jsh.DefaultIfEmpty()
                                        where ((C.processType == "cash" ||
                                     (C.isConfirm == 1 && C.side == "p"
                                       && (C.transType == "d" ?
                                       entity.cashTransfer.Where(x2 => x2.cashTransId == (int)C.cashTransIdSource).FirstOrDefault().isConfirm == 1 :
                                       entity.cashTransfer.Where(x2 => C.cashTransId == (int)x2.cashTransIdSource).FirstOrDefault().isConfirm == 1
                                       ))) && (brIds.Contains(jpp.branches.branchId) || brIds.Contains(jpcc.branches.branchId)))


                                        select new
                                        {
                                            cashTransId = C.cashTransId,
                                            transType = C.transType,
                                            posId = C.posId,
                                            userId = C.userId,
                                            agentId = C.agentId,
                                            invId = C.invId,
                                            transNum = C.transNum,
                                            createDate = C.createDate,
                                            updateDate = C.updateDate,
                                            cash = C.cash,
                                            updateUserId = C.updateUserId,
                                            createUserId = C.createUserId,
                                            notes = C.notes,
                                            posIdCreator = C.posIdCreator,
                                            isConfirm = C.isConfirm,
                                            cashTransIdSource = C.cashTransIdSource,
                                            side = C.side,

                                            docName = C.docName,
                                            docNum = C.docNum,
                                            docImage = C.docImage,
                                            bankId = C.bankId,
                                            bankName = jbb.name,
                                            agentName = jaa.name,

                                            usersName = juu.name,// side =u
                                            userAcc = juu.username,// side =u
                                            posName = jpp.name,
                                            posCreatorName = jpcc.name,
                                            processType = C.processType,
                                            cardId = C.cardId,
                                            bondId = C.bondId,
                                            usersLName = juu.lastname,// side =u
                                            updateUserName = jucc.name,
                                            updateUserLName = jucc.lastname,
                                            updateUserAcc = jucc.username,
                                            createUserJob = jucc.job,
                                            cardName = jcrd.name,
                                            bondDeserveDate = jbbo.deserveDate,
                                            bondIsRecieved = jbbo.isRecieved,
                                            agentCompany = jaa.company,
                                            shippingCompanyId = C.shippingCompanyId,
                                            shippingCompanyName = C.shippingCompanies.name,

                                            agentBalance = jaa.balance,
                                            agentBType = jaa.balanceType,
                                            userBalance = juu.balance,
                                            userBType = juu.balanceType,
                                            shippingBalance = (decimal?)jshh.balance,

                                            shippingCompaniesBType = jshh.balanceType,
                                            branchName = jpp.branches.name,
                                            jpp.branchId,
                                            posBalance = jpp.balance,

                                        }).ToList();
                        /*
                        if (cachlist.Count > 0 )
                        {
                            CashTransferModel tempitem = null;
                            foreach (CashTransferModel cashtItem in cachlist)
                            {if (cashtItem.side == "p") { }
                                tempitem = this.Getpostransmodel(cashtItem.cashTransId)
                                    .Where(C => C.cashTransId != cashtItem.cashTransId).FirstOrDefault();
                                cashtItem.cashTrans2Id = tempitem.cashTransId;
                                cashtItem.pos2Id = tempitem.posId;
                                cashtItem.pos2Name = tempitem.posName;
                                cashtItem.isConfirm2 = tempitem.isConfirm;
                                // cashtItem.posCreatorName = tempitem.posName;


                            }

                        }
                        */





                        return TokenManager.GenerateToken(cachlist);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        // الارباح


        [HttpPost]
        [Route("GetProfit")]
        public string GetProfit(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;
                InvoicesController invoice = new InvoicesController();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                int ITitemUnitId = 0;
                int ITitemId = 0;
                decimal? avgPurchasePrice = 0;
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<ItemUnitInvoiceProfitModel> invListm = (from IT in entity.itemsTransfer
                                                                     from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                                                     from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                                                         //  join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                                                     join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                                                     join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                                                     join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                                                     //    join B in entity.branches on I.branchId equals B.branchId into JB
                                                                     join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                                                     join A in entity.agents on I.agentId equals A.agentId into JA
                                                                     // join U in entity.users on I.createUserId equals U.userId into JU
                                                                     join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                                                     // join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                                                     join P in entity.pos on I.posId equals P.posId into JP

                                                                     // from JBB in JB
                                                                     from JPP in JP.DefaultIfEmpty()
                                                                         // from JUU in JU.DefaultIfEmpty()
                                                                     from JUPUS in JUPUSR.DefaultIfEmpty()
                                                                         // from JIMM in JIM.DefaultIfEmpty()
                                                                     from JAA in JA.DefaultIfEmpty()
                                                                     from JBCC in JBC.DefaultIfEmpty()
                                                                     where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "ts" || I.invType == "ss")

                                                                     select new ItemUnitInvoiceProfitModel
                                                                     {

                                                                         ITitemName = ITEM.name,
                                                                         ITunitName = UNIT.name,
                                                                         ITitemsTransId = IT.itemsTransId,
                                                                         ITitemUnitId = IT.itemUnitId,

                                                                         ITitemId = IU.itemId,
                                                                         ITunitId = IU.unitId,
                                                                         ITquantity = IT.quantity,
                                                                         avgPurchasePrice = ITEM.avgPurchasePrice,
                                                                         // ITcreateDate = IT.createDate,
                                                                         ITupdateDate = IT.updateDate,
                                                                         //  ITcreateUserId = IT.createUserId,
                                                                         ITupdateUserId = IT.updateUserId,
                                                                         // ITnotes = IT.notes,
                                                                         ITprice = IT.price,
                                                                         ITbarcode = IU.barcode,
                                                                         //  ITCreateuserName = ITCUSER.name,
                                                                         // ITCreateuserLName = ITCUSER.lastname,
                                                                         //  ITCreateuserAccName = ITCUSER.username,


                                                                         invoiceId = I.invoiceId,
                                                                         invNumber = I.invNumber,
                                                                         invBarcode = I.invBarcode,                                                         
                                                                         agentId = I.agentId,
                                                                         posId = I.posId,
                                                                         invType = I.invType,
                                                                         total = I.total - I.shippingCost,
                                                                         totalNet = I.totalNet,
                                                                         //  I.paid,
                                                                         // I.deserved,
                                                                         //I.deservedDate,
                                                                         invDate= I.invDate,
                                                                         //  I.invoiceMainId,
                                                                         // I.invCase,
                                                                         //  I.invTime,
                                                                         // I.notes,
                                                                         //  I.vendorInvNum,
                                                                         // I.vendorInvDate,
                                                                         // I.createUserId,
                                                                         updateDate = I.updateDate,
                                                                         updateUserId = I.updateUserId,
                                                                         branchId = I.branchId,
                                                                         //calc coupon + manual discount
                                                                         discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),

                                                                         discountType ="1",
                                                                         tax = I.tax,
                                                                         //  I.name,
                                                                         // I.isApproved,

                                                                         //
                                                                         branchCreatorId = I.branchCreatorId,
                                                                         branchCreatorName = JBCC.name,
                                                                         //
                                                                         //  branchName = JBB.name,

                                                                         //  branchType = JBB.type,
                                                                         posName = JPP.name,
                                                                         posCode = JPP.code,
                                                                         agentName = JAA.name,
                                                                         agentCode = JAA.code,
                                                                         agentType = JAA.type,
                                                                         //  cuserName = JUU.name,
                                                                         //  cuserLast = JUU.lastname,
                                                                         // cUserAccName = JUU.username,
                                                                         uuserName = JUPUS.name,
                                                                         uuserLast = JUPUS.lastname,
                                                                         uUserAccName = JUPUS.username,
                                                                         agentCompany = JAA.company,
                                                                         subTotal = ((IT.price - (IT.itemTax * IU.price / 100)) * IT.quantity),
                                                                         shippingCost = I.shippingCost,
                                                                         realShippingCost = I.realShippingCost,
                                                                         shippingProfit = I.shippingCost - I.realShippingCost,
                                                                         totalNetNoShip = (decimal)I.totalNet - I.shippingCost,
                                                                         totalNoShip = (decimal)I.total - I.shippingCost,
                                                                         itemType = ITEM.type,
                                                                         //(ITEM.taxes *IU.price/100) = tax value
                                                                         //username

                                                                         //  I.invoiceId,
                                                                         //    JBB.name

                                                                     }).ToList();


                        foreach (ItemUnitInvoiceProfitModel row in invListm)
                        {
                            decimal unitpurchasePrice = 0;
                            //   unitpurchasePrice = invoice.AvgItemPurPrice((int)row.ITitemUnitId, (int)row.ITitemId);
                            //4 test

                            avgPurchasePrice = row.avgPurchasePrice;


                            //
                            if (row.itemType == "p")
                            {
                                unitpurchasePrice = AvgPackagePurPrice(ITitemUnitId);
                            }
                            else
                            {
                                unitpurchasePrice = AvgPurPrice((int)row.ITitemUnitId, (int)row.ITitemId, row.avgPurchasePrice == null ? 0 : row.avgPurchasePrice);

                            }


                            row.purchasePrice = (decimal)row.ITquantity * unitpurchasePrice;

                            if (row.discountValue > 0)
                            {
                                decimal ITdiscountpercent = 0;
                                ITdiscountpercent = ((decimal)row.ITprice * 100) / ((decimal)row.totalNoShip);
                                decimal subTotalDiscount = (ITdiscountpercent * (decimal)row.discountValue) / 100;
                                row.subTotalNet = (decimal)row.subTotal - subTotalDiscount;
                                //   row.ITdiscountpercent = ITdiscountpercent;
                            }
                            else
                            {
                                row.subTotalNet = (decimal)row.subTotal;
                                // row.ITdiscountpercent = 0;
                            }

                            row.itemunitProfit = row.subTotalNet - row.purchasePrice;



                        }

                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch (Exception ex)
                {
                    //  return TokenManager.GenerateToken(ITitemUnitId.ToString() + "-" + ITitemId.ToString() + "-" + avgPurchasePrice.ToString()); 


                    return TokenManager.GenerateToken("0");
                }

            }


        }
        [HttpPost]
        [Route("GetInvoiceProfit")]
        public string GetInvoiceProfit(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;
                InvoicesController invoice = new InvoicesController();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);

                decimal? avgPurchasePrice = 0;
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<ItemUnitInvoiceProfitModel> invListm = (from IT in entity.itemsTransfer
                                                                     from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                                                     from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                                                         //  join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                                                     join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                                                     join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                                                     join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                                                     //    join B in entity.branches on I.branchId equals B.branchId into JB
                                                                     join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                                                     join A in entity.agents on I.agentId equals A.agentId into JA
                                                                     // join U in entity.users on I.createUserId equals U.userId into JU
                                                                     join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                                                     // join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                                                     join P in entity.pos on I.posId equals P.posId into JP

                                                                     // from JBB in JB
                                                                     from JPP in JP.DefaultIfEmpty()
                                                                         // from JUU in JU.DefaultIfEmpty()
                                                                     from JUPUS in JUPUSR.DefaultIfEmpty()
                                                                         // from JIMM in JIM.DefaultIfEmpty()
                                                                     from JAA in JA.DefaultIfEmpty()
                                                                     from JBCC in JBC.DefaultIfEmpty()
                                                                     where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "ts" || I.invType == "ss")

                                                                     select new ItemUnitInvoiceProfitModel
                                                                     {

                                                                         ITitemName = ITEM.name,
                                                                         ITunitName = UNIT.name,
                                                                         ITitemsTransId = IT.itemsTransId,
                                                                         ITitemUnitId = IT.itemUnitId,

                                                                         ITitemId = IU.itemId,
                                                                         ITunitId = IU.unitId,
                                                                         ITquantity = IT.quantity,
                                                                         avgPurchasePrice = ITEM.avgPurchasePrice,
                                                                         // ITcreateDate = IT.createDate,
                                                                         ITupdateDate = IT.updateDate,
                                                                         //  ITcreateUserId = IT.createUserId,
                                                                         ITupdateUserId = IT.updateUserId,
                                                                         // ITnotes = IT.notes,
                                                                         ITprice = IT.price,
                                                                         ITbarcode = IU.barcode,
                                                                         //  ITCreateuserName = ITCUSER.name,
                                                                         // ITCreateuserLName = ITCUSER.lastname,
                                                                         //  ITCreateuserAccName = ITCUSER.username,


                                                                         invoiceId = I.invoiceId,
                                                                         invNumber = I.invNumber,
                                                                         invBarcode = I.invBarcode,
                                                                         agentId = I.agentId,
                                                                         posId = I.posId,
                                                                         invType = I.invType,
                                                                         total = I.total,
                                                                         totalNet = I.totalNet,
                                                                         //  I.paid,
                                                                         // I.deserved,
                                                                         //I.deservedDate,
                                                                         invDate= I.invDate,
                                                                         //  I.invoiceMainId,
                                                                         // I.invCase,
                                                                         //  I.invTime,
                                                                         // I.notes,
                                                                         //  I.vendorInvNum,
                                                                         // I.vendorInvDate,
                                                                         // I.createUserId,
                                                                         updateDate = I.updateDate,
                                                                         updateUserId = I.updateUserId,
                                                                         branchId = I.branchId,
                                                                         //calc coupon + manual discount
                                                                         discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),

                                                                         discountType ="1",
                                                                         tax = I.tax,
                                                                         //  I.name,
                                                                         // I.isApproved,
                                                                         //    //offer discount
                                                                         //
                                                                         branchCreatorId = I.branchCreatorId,
                                                                         branchCreatorName = JBCC.name,
                                                                         //
                                                                         //  branchName = JBB.name,

                                                                         //  branchType = JBB.type,
                                                                         posName = JPP.name,
                                                                         posCode = JPP.code,
                                                                         agentName = JAA.name,
                                                                         agentCode = JAA.code,
                                                                         agentType = JAA.type,
                                                                         //  cuserName = JUU.name,
                                                                         //  cuserLast = JUU.lastname,
                                                                         // cUserAccName = JUU.username,
                                                                         uuserName = JUPUS.name,
                                                                         uuserLast = JUPUS.lastname,
                                                                         uUserAccName = JUPUS.username,
                                                                         agentCompany = JAA.company,
                                                                         subTotal = (IT.itemUnitPrice - (IT.offerType == 2 ? (IT.offerValue * IT.itemUnitPrice / 100) : IT.offerValue)) * IT.quantity,
                                                                         shippingCost = I.shippingCost,
                                                                         realShippingCost = I.realShippingCost,
                                                                         shippingProfit = I.shippingCost - I.realShippingCost,
                                                                         totalNetNoShip = (decimal)I.totalNet - I.shippingCost,
                                                                         totalNoShip = (decimal)I.total,
                                                                         itemType = ITEM.type,
                                                                         //(ITEM.taxes *IU.price/100) = tax value
                                                                         //username
                                                                         //ITUpdateuserAccNam.invdate
                                                                         //  I.invoiceId,
                                                                         //    JBB.name

                                                                     }).ToList();


                        foreach (ItemUnitInvoiceProfitModel row in invListm)
                        {
                            decimal unitpurchasePrice = 0;

                            avgPurchasePrice = row.avgPurchasePrice;


                            //
                            if (row.itemType == "p")
                            {
                                unitpurchasePrice = AvgPackagePurPrice((int)row.ITitemUnitId);
                            }
                            else
                            {
                                unitpurchasePrice = AvgPurPrice((int)row.ITitemUnitId, (int)row.ITitemId, row.avgPurchasePrice == null ? 0 : row.avgPurchasePrice);

                            }


                            row.purchasePrice = (decimal)row.ITquantity * unitpurchasePrice;


                            row.subTotalNet = (decimal)row.subTotal;



                            row.itemunitProfit = row.subTotalNet - row.purchasePrice;

                        }
                        invListm = invListm.GroupBy(G => G.invoiceId).Select(G => new ItemUnitInvoiceProfitModel
                        {
                            ITitemName = G.FirstOrDefault().ITitemName,
                            //ITunitName = UNIT.name,
                            //ITitemsTransId = IT.itemsTransId,
                            //ITitemUnitId = IT.itemUnitId,

                            //ITitemId = IU.itemId,
                            //ITunitId = IU.unitId,
                            ITquantity = G.Sum(q => q.ITquantity),
                            //avgPurchasePrice = ITEM.avgPurchasePrice,
                            purchasePrice = G.Sum(q => q.purchasePrice),//مجموع اسعار الشراء للعناصر

                            //ITupdateDate = IT.updateDate,

                            //ITupdateUserId = IT.updateUserId,

                            //ITprice = IT.price,
                            //ITbarcode = IU.barcode,



                            invoiceId = G.FirstOrDefault().invoiceId,

                            invNumber = G.FirstOrDefault().invNumber,
                            invBarcode = G.FirstOrDefault().invBarcode,
                            agentId = G.FirstOrDefault().agentId,
                            posId = G.FirstOrDefault().posId,
                            invType = G.FirstOrDefault().invType,
                            total = G.FirstOrDefault().total,
                            totalNet = G.FirstOrDefault().totalNet,
                            invDate = G.FirstOrDefault().invDate,
                            updateDate = G.FirstOrDefault().updateDate,
                            updateUserId = G.FirstOrDefault().updateUserId,
                            branchId = G.FirstOrDefault().branchId,

                            discountValue = G.FirstOrDefault().discountValue,
                            discountType = G.FirstOrDefault().discountType,
                            tax = G.FirstOrDefault().tax,

                            branchCreatorId = G.FirstOrDefault().branchCreatorId,
                            branchCreatorName = G.FirstOrDefault().branchCreatorName,

                            posName = G.FirstOrDefault().posName,
                            posCode = G.FirstOrDefault().posCode,
                            agentName = G.FirstOrDefault().agentName,
                            agentCode = G.FirstOrDefault().agentCode,
                            agentType = G.FirstOrDefault().agentType,

                            uuserName = G.FirstOrDefault().uuserName,
                            uuserLast = G.FirstOrDefault().uuserLast,
                            uUserAccName = G.FirstOrDefault().uUserAccName,
                            agentCompany = G.FirstOrDefault().agentCompany,
                            subTotalNet = G.Sum(q => q.subTotalNet),
                            shippingCost = G.FirstOrDefault().shippingCost,
                            realShippingCost = G.FirstOrDefault().realShippingCost,
                            shippingProfit = G.FirstOrDefault().shippingProfit,
                            totalNetNoShip = G.FirstOrDefault().totalNetNoShip,
                            totalNoShip = G.FirstOrDefault().totalNoShip,
                            invoiceProfit = 0,
                        }).ToList();
                        foreach (ItemUnitInvoiceProfitModel row in invListm)
                        {
                            row.invoiceProfit = row.subTotalNet - (decimal)row.discountValue - row.purchasePrice + row.shippingProfit;
                        }


                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch (Exception ex)
                {
                    //  return TokenManager.GenerateToken(ITitemUnitId.ToString() + "-" + ITitemId.ToString() + "-" + avgPurchasePrice.ToString()); 


                    return TokenManager.GenerateToken("0");
                }

            }


        }

        [HttpPost]
        [Route("GetItemProfit")]
        public string GetItemProfit(string token)
        {
            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                InvoicesController invoice = new InvoicesController();
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }


                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);

                decimal? avgPurchasePrice = 0;
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        List<ItemUnitInvoiceProfitModel> invListm = (from IT in entity.itemsTransfer
                                                                     from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                                                     from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)
                                                                         //  join ITCUSER in entity.users on IT.createUserId equals ITCUSER.userId
                                                                     join ITUPUSER in entity.users on IT.updateUserId equals ITUPUSER.userId
                                                                     join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                                                     join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                                                     //    join B in entity.branches on I.branchId equals B.branchId into JB
                                                                     join BC in entity.branches on I.branchCreatorId equals BC.branchId into JBC
                                                                     join A in entity.agents on I.agentId equals A.agentId into JA
                                                                     // join U in entity.users on I.createUserId equals U.userId into JU
                                                                     join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                                                     // join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                                                     join P in entity.pos on I.posId equals P.posId into JP

                                                                     // from JBB in JB
                                                                     from JPP in JP.DefaultIfEmpty()
                                                                         // from JUU in JU.DefaultIfEmpty()
                                                                     from JUPUS in JUPUSR.DefaultIfEmpty()
                                                                         // from JIMM in JIM.DefaultIfEmpty()
                                                                     from JAA in JA.DefaultIfEmpty()
                                                                     from JBCC in JBC.DefaultIfEmpty()
                                                                     where (brIds.Contains(JBCC.branchId)) && (I.invType == "s" || I.invType == "ts" || I.invType == "ss")

                                                                     select new ItemUnitInvoiceProfitModel
                                                                     {

                                                                         ITitemName = ITEM.name,
                                                                         ITunitName = UNIT.name,
                                                                         ITitemsTransId = IT.itemsTransId,
                                                                         ITitemUnitId = IT.itemUnitId,

                                                                         ITitemId = IU.itemId,
                                                                         ITunitId = IU.unitId,
                                                                         ITquantity = IT.quantity,
                                                                         avgPurchasePrice = ITEM.avgPurchasePrice,
                                                                         // ITcreateDate = IT.createDate,
                                                                         ITupdateDate = IT.updateDate,
                                                                         //  ITcreateUserId = IT.createUserId,
                                                                         ITupdateUserId = IT.updateUserId,
                                                                         // ITnotes = IT.notes,
                                                                         ITprice = IT.price,
                                                                         ITbarcode = IU.barcode,
                                                                         //  ITCreateuserName = ITCUSER.name,
                                                                         // ITCreateuserLName = ITCUSER.lastname,
                                                                         //  ITCreateuserAccName = ITCUSER.username,

                                                                         invoiceId = I.invoiceId,
                                                                         invNumber = I.invNumber,
                                                                         invBarcode = I.invBarcode,
                                                                         agentId = I.agentId,
                                                                         posId = I.posId,
                                                                         invType = I.invType,
                                                                         total = I.total,
                                                                         totalNet = I.totalNet,
                                                                         //  I.paid,
                                                                         // I.deserved,
                                                                         //I.deservedDate,
                                                                         //I.invDate,
                                                                         //  I.invoiceMainId,
                                                                         // I.invCase,
                                                                         //  I.invTime,
                                                                         // I.notes,
                                                                         //  I.vendorInvNum,
                                                                         // I.vendorInvDate,
                                                                         // I.createUserId,
                                                                         updateDate = I.updateDate,
                                                                         updateUserId = I.updateUserId,
                                                                         branchId = I.branchId,
                                                                         //calc coupon + manual discount
                                                                         discountValue = (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Count()) == 0 ? 0 :
                                           (I.couponsInvoices.Where(X => X.InvoiceId == I.invoiceId).ToList().Sum(C => C.discountType == 2 ? (C.discountValue * I.total / 100) : C.discountValue))
                                            + (I.discountType == "2" ? I.discountValue * I.total / 100 : I.discountValue),

                                                                         discountType = "1",
                                                                         tax = I.tax,
                                                                         //  I.name,
                                                                         // I.isApproved,

                                                                         //
                                                                         //branchCreatorId = I.branchCreatorId,
                                                                         //branchCreatorName = JBCC.name,
                                                                         //
                                                                         //  branchName = JBB.name,

                                                                         //  branchType = JBB.type,
                                                                         //posName = JPP.name,
                                                                         //posCode = JPP.code,
                                                                         //agentName = JAA.name,
                                                                         //agentCode = JAA.code,
                                                                         //agentType = JAA.type,
                                                                         //  cuserName = JUU.name,
                                                                         //  cuserLast = JUU.lastname,
                                                                         // cUserAccName = JUU.username,
                                                                         //uuserName = JUPUS.name,
                                                                         //uuserLast = JUPUS.lastname,
                                                                         //uUserAccName = JUPUS.username,
                                                                         //agentCompany = JAA.company,
                                                                         subTotal = (IT.itemUnitPrice - (IT.offerType == 2 ? (IT.offerValue * IT.itemUnitPrice / 100) : IT.offerValue)) * IT.quantity,
                                                                         //shippingCost = I.shippingCost,
                                                                         //realShippingCost = I.realShippingCost,
                                                                         //shippingProfit = I.shippingCost - I.realShippingCost,
                                                                         //totalNetNoShip = (decimal)I.totalNet - I.shippingCost,
                                                                         //totalNoShip = (decimal)I.total,
                                                                         itemType = ITEM.type,
                                                                         //(ITEM.taxes *IU.price/100) = tax value
                                                                         //username

                                                                         //  I.invoiceId,
                                                                         //    JBB.name

                                                                     }).ToList();

                        foreach (ItemUnitInvoiceProfitModel row in invListm)
                        {
                            decimal unitpurchasePrice = 0;
                            //   unitpurchasePrice = invoice.AvgItemPurPrice((int)row.ITitemUnitId, (int)row.ITitemId);
                            //4 test

                            avgPurchasePrice = row.avgPurchasePrice;

                            //
                            if (row.itemType == "p")
                            {
                                unitpurchasePrice = AvgPackagePurPrice((int)row.ITitemUnitId);
                            }
                            else
                            {
                                unitpurchasePrice = AvgPurPrice((int)row.ITitemUnitId, (int)row.ITitemId, row.avgPurchasePrice == null ? 0 : row.avgPurchasePrice);

                            }
                            row.purchasePrice = (decimal)row.ITquantity * unitpurchasePrice;
                            row.subTotalNet = (decimal)row.subTotal;
                            row.itemunitProfit = row.subTotalNet - row.purchasePrice;
                        }

                        //////////////////////
                        //invListm = invListm.GroupBy(G => G.ITitemUnitId).Select(G => new ItemUnitInvoiceProfitModel
                        //{
                        //    itemunitProfit= G.Sum(q => q.itemunitProfit),
                        //    ITitemName = G.FirstOrDefault().ITitemName,
                        //    ITunitName = G.FirstOrDefault().ITunitName,
                        //     ITitemsTransId = G.FirstOrDefault().ITitemsTransId,
                        //     ITitemUnitId = G.FirstOrDefault().ITitemUnitId,

                        //    ITitemId = G.FirstOrDefault().ITitemId,
                        //    ITunitId = G.FirstOrDefault().ITunitId,
                        //    ITquantity = G.Sum(q => q.ITquantity),
                        //    //avgPurchasePrice = ITEM.avgPurchasePrice,
                        //    purchasePrice = G.Sum(q => q.purchasePrice),//مجموع اسعار الشراء للعناصر

                        //    //ITupdateDate = IT.updateDate,

                        //    //ITupdateUserId = IT.updateUserId,

                        //    //ITprice = IT.price,
                        //     ITbarcode =G.FirstOrDefault().ITbarcode,

                        //    invoiceId = G.FirstOrDefault().invoiceId,

                        //  //  invNumber = G.FirstOrDefault().invNumber,
                        //  //  agentId = G.FirstOrDefault().agentId,
                        //    posId = G.FirstOrDefault().posId,
                        //    //  invType = G.FirstOrDefault().invType,
                        //    //  total = G.FirstOrDefault().total,
                        //    //  totalNet = G.FirstOrDefault().totalNet,

                        //    updateDate = G.FirstOrDefault().updateDate,
                        //    //updateUserId = G.FirstOrDefault().updateUserId,
                        //    //branchId = G.FirstOrDefault().branchId,

                        //    //discountValue = G.FirstOrDefault().discountValue,
                        //    //discountType = G.FirstOrDefault().discountType,
                        //    //tax = G.FirstOrDefault().tax,

                        //    //branchCreatorId = G.FirstOrDefault().branchCreatorId,
                        //    //branchCreatorName = G.FirstOrDefault().branchCreatorName,

                        //    posName = G.FirstOrDefault().posName,
                        //    posCode = G.FirstOrDefault().posCode,
                        //    //agentName = G.FirstOrDefault().agentName,
                        //    //agentCode = G.FirstOrDefault().agentCode,
                        //    //agentType = G.FirstOrDefault().agentType,

                        //    //uuserName = G.FirstOrDefault().uuserName,
                        //    //uuserLast = G.FirstOrDefault().uuserLast,
                        //    //uUserAccName = G.FirstOrDefault().uUserAccName,
                        //    //agentCompany = G.FirstOrDefault().agentCompany,
                        //    //subTotal = G.FirstOrDefault().subTotal,
                        //    //shippingCost = G.FirstOrDefault().shippingCost,
                        //    //realShippingCost = G.FirstOrDefault().realShippingCost,
                        //    //shippingProfit = G.FirstOrDefault().shippingProfit,
                        //    //totalNetNoShip = G.FirstOrDefault().totalNetNoShip,
                        //    //totalNoShip = G.FirstOrDefault().totalNoShip,
                        //    invoiceProfit = 0,

                        //}).ToList();

                        ///////////////////
                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch (Exception ex)
                {
                    //  return TokenManager.GenerateToken(ITitemUnitId.ToString() + "-" + ITitemId.ToString() + "-" + avgPurchasePrice.ToString()); 


                    return TokenManager.GenerateToken("0");
                }

            }


        }

        public decimal AvgPackagePurPrice(int parentIUId)
        {
            PackageController pctrlr = new PackageController();
            decimal avgtotal = 0;

            List<PackageModel> list = pctrlr.GetChildsByParentId(parentIUId);
            foreach (var row in list)
            {
                if (row.type == "p")
                {
                    avgtotal += AvgPackagePurPrice((int)row.childIUId);
                }
                else
                {
                    avgtotal += AvgPurPrice((int)row.childIUId, (int)row.citemId, row.avgPurchasePrice == null ? 0 : row.avgPurchasePrice) * row.quantity;

                }
            }
            return avgtotal;

        }
        //public decimal AvgPackagePurPrice(int parentIUId)
        //{
        //    PackageController pctrlr = new PackageController();
        //    decimal avgtotal = 0;

        //    List<PackageModel> list = pctrlr.GetChildsByParentId(parentIUId);
        //    foreach (var row in list)
        //    {
        //        if (row.type == "p")
        //        {
        //            avgtotal += AvgPackagePurPrice((int)row.childIUId);
        //        }
        //        else
        //        {
        //            avgtotal += AvgPurPrice((int)row.childIUId, (int)row.citemId, row.avgPurchasePrice == null ? 0 : row.avgPurchasePrice) * row.quantity;

        //        }
        //    }
        //    return avgtotal;

        //}
        public decimal AvgPurPrice(int itemUnitId, int itemId, decimal? smallavgprice)
        {


            decimal avgPrice = 0;

            using (incposdbEntities entity = new incposdbEntities())
            {
                var itemUnits = (from i in entity.itemsUnits where (i.itemId == itemId) select (i.itemUnitId)).ToList();



                var smallestUnitId = (from iu in entity.itemsUnits
                                      where (itemUnits.Contains((int)iu.itemUnitId) && iu.unitId == iu.subUnitId)
                                      select iu.itemUnitId).FirstOrDefault();


                if (itemUnitId == smallestUnitId || smallestUnitId == null || smallestUnitId == 0)
                    return (decimal)smallavgprice;
                else
                {
                    avgPrice = ((decimal)smallavgprice) * getUpperUnitValue(smallestUnitId, itemUnitId);
                    return avgPrice;
                }
            }


        }
        private int getUpperUnitValue(int itemUnitId, int basicItemUnitId)
        {
            int unitValue = 0;
            using (incposdbEntities entity = new incposdbEntities())
            {

                var unit = entity.itemsUnits.Where(x => x.itemUnitId == itemUnitId).Select(x => new { x.unitId, x.itemId }).FirstOrDefault();
                var upperUnit = entity.itemsUnits.Where(x => x.subUnitId == unit.unitId && x.itemId == unit.itemId && x.subUnitId != x.unitId).Select(x => new { x.unitValue, x.itemUnitId }).FirstOrDefault();

                if (upperUnit == null)
                    return 1;
                if (upperUnit.itemUnitId == basicItemUnitId)
                    return (int)upperUnit.unitValue;
                else
                    unitValue *= getUpperUnitValue(upperUnit.itemUnitId, basicItemUnitId);
                return unitValue;
            }
        }
        #endregion
        // 
        #region Kitchen

        // preparing orders
        [HttpPost]
        [Route("GetPreparingOrders")]
        public string GetPreparingOrders(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                //  int invoiceId = 0;
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {

                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                try
                {
                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        List<OrderPreparingSTSModel> prepOrders1 = (from o in entity.orderPreparing
                                                                    join i in entity.itemOrderPreparing on o.orderPreparingId equals i.orderPreparingId
                                                                    join s in entity.orderPreparingStatus on o.orderPreparingId equals s.orderPreparingId
                                                                    where (brIds.Contains((int)o.invoices.branchId)) && (s.orderStatusId == entity.orderPreparingStatus.Where(x => x.orderPreparingId == o.orderPreparingId).Max(x => x.orderStatusId))
                                                                    select new OrderPreparingSTSModel()
                                                                    {

                                                                        orderPreparingId = o.orderPreparingId,
                                                                        invoiceId = o.invoiceId,
                                                                        notes = o.notes,
                                                                        orderNum = o.orderNum,
                                                                        preparingTime = o.preparingTime,
                                                                        updateDate = s.updateDate,
                                                                        updateUserId = s.updateUserId,
                                                                        createDate = s.createDate,
                                                                        createUserId = s.createUserId,
                                                                        itemName = i.itemsUnits.items.name,
                                                                        quantity = (int)i.quantity,
                                                                        status = s.status,
                                                                        itemUnitId = i.itemUnitId,
                                                                        branchId = o.invoices.branchId,
                                                                        branchName = o.invoices.branches.name,
                                                                        categoryId = i.itemsUnits.items.categoryId,
                                                                        categoryName = i.itemsUnits.items.categories.name,
                                                                        invNumber = o.invoices.invNumber,
                                                                        invBarcode = o.invoices.invBarcode,
                                                                        tagId = i.itemsUnits.items.tagId,
                                                                        tagName = i.itemsUnits.items.tags.tagName,
                                                                        listedDate = entity.orderPreparingStatus.Where(X => X.orderPreparingId == o.orderPreparingId).OrderBy(X => X.orderStatusId).Select(X => X.createDate).FirstOrDefault().Value,

                                                                    }).OrderBy(x => x.orderNum).ToList();


                        List<OrderPreparingSTSModel> prepOrders = prepOrders1.GroupBy(X => X.orderPreparingId).SelectMany(grouping => grouping.Take(1)).ToList();

                        //int index = 1;
                        foreach (OrderPreparingSTSModel o in prepOrders)
                        {
                            if (o.createDate == null || o.listedDate == null)
                            {
                                o.realDuration = 0;
                            }
                            else
                            {
                                TimeSpan remainingTime = (DateTime)o.createDate - (DateTime)o.listedDate;
                                o.realDuration = (int)remainingTime.TotalMinutes;
                            }

                            //  o.num = index;
                            //index++;
                            //#region calculate remaining time
                            //if (o.preparingTime != null)
                            //{
                            //    DateTime createDate = (DateTime)o.createDate;
                            //    createDate = createDate.AddMinutes((double)o.preparingTime);

                            //    if (createDate > DateTime.Now)
                            //    {
                            //        TimeSpan remainingTime = createDate - DateTime.Now;
                            //        o.remainingTime = (decimal)remainingTime.TotalMinutes;
                            //    }
                            //    else
                            //    {
                            //        o.remainingTime = 0;

                            //    }
                            //}

                        }
                        return TokenManager.GenerateToken(prepOrders);
                    }
                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                }
            }
        }


        //فواتير الصرف من المخزن الى المطبخ 
        [HttpPost]
        [Route("GetSpendingRequest")]
        public string GetSpendingRequest(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchId equals BC.branchId into JBC

                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR

                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "sr" || I.invType == "srb" || I.invType == "srw")

                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                         I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            //I.total,
                                            //I.totalNet,
                                            //I.paid,
                                            //I.deserved,
                                            //I.deservedDate,
                                            I.invDate,
                                            //I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            //I.vendorInvNum,
                                            //I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            //discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            //I.discountType,
                                            //I.tax,
                                            //I.name,
                                            //I.isApproved,

                                            //
                                            //I.branchCreatorId,
                                            //branchCreatorName = JBCC.name,
                                            //
                                            branchName = JBCC.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch (Exception ex)
                {
                    return TokenManager.GenerateToken(ex);
                    // return TokenManager.GenerateToken("0");
                }

            }


        }

        //عناصر فواتير الصرف
        [HttpPost]
        [Route("GetSpendingItems")]
        public string GetSpendingItems(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {


                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)

                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        //    join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchId equals BC.branchId into JBC

                                        //join U in entity.users on I.createUserId equals U.userId into JU
                                        //join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        //join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        // from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()



                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "sr" || I.invType == "srb")

                                        select new
                                        {

                                            ITitemName = ITEM.name,
                                            ITunitName = UNIT.name,
                                            ITitemsTransId = IT.itemsTransId,
                                            ITitemUnitId = IT.itemUnitId,

                                            ITitemId = IU.itemId,
                                            ITunitId = IU.unitId,
                                            ITquantity = IT.quantity,

                                            //ITcreateDate = IT.createDate,
                                            //ITupdateDate = IT.updateDate,
                                            //ITcreateUserId = IT.createUserId,
                                            //ITupdateUserId = IT.updateUserId,
                                            //ITnotes = IT.notes,
                                            //ITprice = IT.price,
                                            ITbarcode = IU.barcode,

                                            I.invoiceId,
                                            I.invNumber,
                                        I.invBarcode,
                                            //I.agentId,
                                            I.posId,
                                            I.invType,
                                            //I.total,
                                            //I.totalNet,
                                            //I.paid,
                                            //I.deserved,
                                            //I.deservedDate,
                                            I.invDate,
                                            //I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            //discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            //I.discountType,
                                            I.tax,

                                            I.isApproved,

                                            //
                                            //    I.branchCreatorId,
                                            //branchCreatorName = JBCC.name,
                                            //
                                            branchName = JBCC.name,

                                            //  branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            //subTotal = (IT.price * IT.quantity),
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();





                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }


        }


        // consumption الاستهلاك
        //فواتير الاستهلاك 
        [HttpPost]
        [Route("GetConsumption")]
        public string GetConsumption(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId



            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from I in entity.invoices

                                            //join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchId equals BC.branchId into JBC

                                        join U in entity.users on I.createUserId equals U.userId into JU
                                        join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR

                                        join P in entity.pos on I.posId equals P.posId into JP

                                        //   from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()
                                        from JUU in JU.DefaultIfEmpty()
                                        from JUPUS in JUPUSR.DefaultIfEmpty()
                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "fbc")

                                        select new
                                        {

                                            I.invoiceId,
                                            count = entity.itemsTransfer.Where(x => x.invoiceId == I.invoiceId).Count(),
                                            I.invNumber,
                                            I.invBarcode,
                                            I.posId,
                                            I.invType,
                                            //I.total,
                                            //I.totalNet,
                                            //I.paid,
                                            //I.deserved,
                                            //I.deservedDate,
                                            I.invDate,
                                            //I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            //I.vendorInvNum,
                                            //I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            //discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            //I.discountType,
                                            //I.tax,
                                            //I.name,
                                            //I.isApproved,

                                            //
                                            //I.branchCreatorId,
                                            //branchCreatorName = JBCC.name,
                                            //
                                            branchName = JBCC.name,

                                            //     branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,
                                            cuserName = JUU.name,
                                            cuserLast = JUU.lastname,
                                            cUserAccName = JUU.username,
                                            uuserName = JUPUS.name,
                                            uuserLast = JUPUS.lastname,
                                            uUserAccName = JUPUS.username,
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();




                        return TokenManager.GenerateToken(invListm);

                    }

                }
                catch (Exception ex)
                {
                    //  return TokenManager.GenerateToken(ex);
                    return TokenManager.GenerateToken("0");
                }

            }


        }

        //عناصر فواتير الاستهلاك
        [HttpPost]
        [Route("GetConsumptionItems")]
        public string GetConsumptionItems(string token)
        {

            // public ResponseVM GetPurinv(string token)

            //int mainBranchId, int userId

            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int mainBranchId = 0;
                int userId = 0;

                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }

                }
                // DateTime cmpdate = DateTime.Now.AddDays(newdays);
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    using (incposdbEntities entity = new incposdbEntities())
                    {

                        var invListm = (from IT in entity.itemsTransfer
                                        from I in entity.invoices.Where(I => I.invoiceId == IT.invoiceId)

                                        from IU in entity.itemsUnits.Where(IU => IU.itemUnitId == IT.itemUnitId)

                                        join ITEM in entity.items on IU.itemId equals ITEM.itemId
                                        join UNIT in entity.units on IU.unitId equals UNIT.unitId
                                        //    join B in entity.branches on I.branchId equals B.branchId into JB
                                        join BC in entity.branches on I.branchId equals BC.branchId into JBC

                                        //join U in entity.users on I.createUserId equals U.userId into JU
                                        //join UPUSR in entity.users on I.updateUserId equals UPUSR.userId into JUPUSR
                                        //join IM in entity.invoices on I.invoiceMainId equals IM.invoiceId into JIM
                                        join P in entity.pos on I.posId equals P.posId into JP

                                        // from JBB in JB
                                        from JPP in JP.DefaultIfEmpty()

                                        from JBCC in JBC.DefaultIfEmpty()
                                        where (brIds.Contains(JBCC.branchId)) && (I.invType == "fbc")

                                        select new
                                        {

                                            ITitemName = ITEM.name,
                                            ITunitName = UNIT.name,
                                            ITitemsTransId = IT.itemsTransId,
                                            ITitemUnitId = IT.itemUnitId,

                                            ITitemId = IU.itemId,
                                            ITunitId = IU.unitId,
                                            ITquantity = IT.quantity,

                                            //ITcreateDate = IT.createDate,
                                            //ITupdateDate = IT.updateDate,
                                            //ITcreateUserId = IT.createUserId,
                                            //ITupdateUserId = IT.updateUserId,
                                            //ITnotes = IT.notes,
                                            //ITprice = IT.price,
                                            ITbarcode = IU.barcode,

                                            I.invoiceId,
                                            I.invNumber,
                                             I.invBarcode,
                                            //I.agentId,
                                            I.posId,
                                            I.invType,
                                            //I.total,
                                            //I.totalNet,
                                            //I.paid,
                                            //I.deserved,
                                            //I.deservedDate,
                                            I.invDate,
                                            //I.invoiceMainId,
                                            I.invCase,
                                            I.invTime,
                                            I.notes,
                                            I.vendorInvNum,
                                            I.vendorInvDate,
                                            I.createUserId,
                                            I.updateDate,
                                            I.updateUserId,
                                            I.branchId,
                                            //discountValue = (I.discountType == "1" || I.discountType == null) ? I.discountValue : (I.discountType == "2" ? (I.discountValue / 100) : 0),
                                            //I.discountType,
                                            I.tax,

                                            I.isApproved,
                                            //
                                            //    I.branchCreatorId,
                                            //branchCreatorName = JBCC.name,
                                            //
                                            branchName = JBCC.name,

                                            //  branchType = JBB.type,
                                            posName = JPP.name,
                                            posCode = JPP.code,

                                            //subTotal = (IT.price * IT.quantity),
                                            //username

                                            //  I.invoiceId,
                                            //    JBB.name
                                        }).ToList();

                        return TokenManager.GenerateToken(invListm);
                    }
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }

            }
        }

        #endregion
        #region delivery
        [HttpPost]
        [Route("GetDelivery")]
        public string GetDelivery(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                #region params


                int mainBranchId = 0;
                int userId = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "mainBranchId")
                    {
                        mainBranchId = int.Parse(c.Value);
                    }
                    else if (c.Type == "userId")
                    {
                        userId = int.Parse(c.Value);
                    }
                }
                #endregion
                try
                {

                    List<int> brIds = AllowedBranchsId(mainBranchId, userId);
                    List<OrderPreparingSTSModel> invoices =new List<OrderPreparingSTSModel>();
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        //var searchPredicate = PredicateBuilder.New<invoices>();
                        //searchPredicate = searchPredicate.And(x => x.branchId == branchId);

                        //searchPredicate = searchPredicate.And(x => x.invType == "ts" || x.invType == "ss");
                        //searchPredicate = searchPredicate.And(x => x.shippingCompanyId != null);


                         invoices = (from x in entity.invoices
                                        join o in entity.orderPreparing on x.invoiceId equals o.invoiceId
                                        join ag in entity.agents on x.agentId equals ag.agentId into gj
                                        join u in entity.users on x.shipUserId equals u.userId into lj
                                        from y in lj.DefaultIfEmpty()
                                        from g in gj.DefaultIfEmpty()
                                        where (brIds.Contains((int)x.branchId) && x.shippingCompanyId != null
                                        && (x.invType == "ts" || x.invType == "ss"))
                                        select new OrderPreparingSTSModel()// InvoiceModel()//
                                                                           //   OrderPreparingSTSModel
                                        {
                                            invNumber = x.invNumber,
                                            invBarcode = x.invBarcode,
                                            invoiceId = x.invoiceId,
                                            shipUserId = x.shipUserId,
                                            shipUserName = y.name,
                                            shipUserLastName = y.lastname,
                                            shippingCompanyId = x.shippingCompanyId,
                                            shippingCompanyName = x.shippingCompanies.name,
                                            invType=x.invType,

                                            orderTime = x.orderTime,//
                                            orderPreparingId = o.orderPreparingId,
                                            orderNum = o.orderNum,
                                            createDate = o.createDate,
                                            createUserId = o.createUserId,
                                            
                                            agentId = x.agentId,
                                            agentName = g.name,
                                            agentCompany = g.company,
                                            agentType = g.type,
                                            agentCode = g.code,

                                            branchId=x.branchId,
                                            branchName=x.branches.name,

                                            orderStatusList = entity.orderPreparingStatus.Where(X => X.orderPreparingId == o.orderPreparingId && (X.status == "InTheWay" || X.status == "Done")).Select(X => new orderPreparingStatusModel
                                            {
                                                status = X.status,
                                                createDate = X.createDate,
                                                updateDate = X.updateDate,
                                                orderPreparingId = X.orderPreparingId,
                                                orderStatusId = X.orderStatusId,

                                            }).OrderBy(X => X.createDate).ToList(),

                                            orderDuration =0,
                                        }).ToList();

                    
                       
                        //o.status == "Done"
                        //foreach (InvoiceModel inv in invoices)
                        //{
                        //    var prepOrders = (from o in entity.orderPreparing.Where(x => x.invoiceId == inv.invoiceId)
                        //                      join s in entity.orderPreparingStatus on o.orderPreparingId equals s.orderPreparingId
                        //                      where (s.orderStatusId == entity.orderPreparingStatus.Where(x => x.orderPreparingId == o.orderPreparingId).Max(x => x.orderStatusId))
                        //                      select new OrderPreparingModel()
                        //                      {
                        //                          orderPreparingId = o.orderPreparingId,
                        //                          invoiceId = o.invoiceId,
                        //                          notes = o.notes,
                        //                          orderNum = o.orderNum,
                        //                          createDate = o.createDate,
                        //                          createUserId = o.createUserId,
                        //                          invNum = o.invoices.invNumber,
                        //                          status = s.status,
                        //                      }).ToList();


                        //    foreach (OrderPreparingModel o in prepOrders)
                        //    {
                        //        #region set inv status
                        //        if (o.status == "Collected")
                        //        {
                        //            inv.status = "Collected";
                        //            break;
                        //        }
                        //        else if (o.status == "InTheWay")
                        //        {
                        //            inv.status = "InTheWay";
                        //            break;
                        //        }
                        //        else if (o.status == "Done")
                        //        {
                        //            inv.status = "Done";
                        //            break;
                        //        }
                        //        else if (o.status == "Listed" || o.status == "Preparing")
                        //        {
                        //            inv.status = "Listed";
                        //            break;
                        //        }
                        //        else
                        //            inv.status = "Ready";
                        //        #endregion

                        //    }

                        //}

                        //#region get orders according to status
                        //if (statusStr != "")
                        //    invoices = invoices.Where(x => statusL.Contains(x.status)).OrderBy(x => x.invNumber).ToList();
                        //#endregion

                       
                    }
                invoices = invoices.Where(X => X.orderStatusList.LastOrDefault()!=null? X.orderStatusList.LastOrDefault().status == "Done":false).ToList();
                    if (invoices != null )
                    {
                        if(invoices.Count() > 0)
                        {

                        
                        foreach (OrderPreparingSTSModel row  in invoices.ToList())
                        {
                            if (row.orderStatusList != null)
                            {

                                TimeSpan tmp = (TimeSpan)(row.orderStatusList.LastOrDefault().createDate - row.orderStatusList.FirstOrDefault().createDate);
                                row.orderDuration = (decimal)tmp.TotalMinutes;


                            }

                        }
                        }
                    }
                    return TokenManager.GenerateToken(invoices);
                }
                catch(Exception ex)
                {
                    return TokenManager.GenerateToken(ex.ToString());
                   // return TokenManager.GenerateToken("0");
                }
            }
        }
        #endregion

    }
}