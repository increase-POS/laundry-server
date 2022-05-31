using Newtonsoft.Json;
using POS_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using POS_Server.Models.VM;
using System.Security.Claims;
using System.Web;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using System.Management;

namespace POS_Server.Controllers
{
    [RoutePrefix("api/ProgramDetails")]
    public class ProgramDetailsController : ApiController
    {


        [HttpPost]
        [Route("getCurrentInfo")]
        public async Task<string> getCurrentInfo(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                try
                {
                    ProgramDetailsModel packrow = new ProgramDetailsModel();
                    packrow = getCurrentInfo();
                
                    return TokenManager.GenerateToken(packrow);
                }
                catch
                {
                    return TokenManager.GenerateToken("0");
                }


            }
        }

        [HttpPost]
        [Route("updateIsonline")]
        public string updateIsonline(string token)
        {
            //string Object
            string message = "";
 
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
               
                bool isOnlineServer = false;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "isOnlineServer")
                    {
                        isOnlineServer = bool.Parse( c.Value) ;
                      
                    }
                }
                    
                    try
                    {
                
                    using (incposdbEntities entity = new incposdbEntities())
                        {

                            var locationEntity = entity.Set<ProgramDetails>();

                      var  packsl = entity.ProgramDetails.ToList();

                      var  packs = packsl.FirstOrDefault();
                        packs.isOnlineServer = isOnlineServer;
                                entity.SaveChanges();
                                message = packs.id.ToString();
 
                           
                            //  entity.SaveChanges();
                        }
                        return TokenManager.GenerateToken(message);

                    }
                    catch
                    {
                        message = "0";
                        return TokenManager.GenerateToken(message);
                    }


                }

               

          
     
        }
        public ProgramDetailsModel getCurrentInfo()
        {       
            ProgramDetailsModel packs = new ProgramDetailsModel();      
            using (incposdbEntities entity = new incposdbEntities())
            {         
                packs = (from p in entity.ProgramDetails
                             //  join p in entity.posSetting on S.id equals p.posSerialId
                         select new ProgramDetailsModel
                         {
                             programName = p.programName,

                             branchCount = p.branchCount,
                             posCount = p.posCount,
                             userCount = p.userCount,
                             vendorCount = p.vendorCount,
                             customerCount = p.customerCount,
                             itemCount = p.itemCount,
                             saleinvCount = p.saleinvCount,
                             storeCount = p.storeCount,
                             packageSaleCode = p.packageSaleCode,
                             customerServerCode = p.customerServerCode,
                             expireDate = p.expireDate,
                             isOnlineServer = p.isOnlineServer,
                             
                             updateDate = p.updateDate,
                             isLimitDate = (p.isLimitDate == true) ? true : false,
                        
                           
                             isActive = p.isActive,
                             packageName=p.packageName,
                             versionName=p.versionName,
                             packageNumber=p.packageNumber,
                             customerName = p.customerName,
                             customerLastName = p.customerLastName,
                             agentName = p.agentName,
                             agentLastName = p.agentLastName,
                             agentAccountName = p.agentAccountName,
                         }).FirstOrDefault();

                packs.posCountNow=  entity.pos.Count();

                packs.branchCountNow = entity.branches.Where(x => x.type == "b").Count();

                packs.storeCountNow= entity.branches.Where(x => x.type == "s").Count();
                packs.userCountNow = entity.users.Count();
                packs.vendorCountNow  = entity.agents.Where(x => x.type =="v").Count();
                packs.customerCountNow= entity.agents.Where(x => x.type == "c").Count();
                packs.itemCountNow = entity.items.Count();

                packs.saleinvCountNow = getSalesInvCountInMonth();
                packs.serverDateNow = DateTime.Now;

            }



            return packs;
        }


        public ProgramDetails getCurrentProgDetail()
        {

            ProgramDetails packs = new ProgramDetails();


            using (incposdbEntities entity = new incposdbEntities())
            {


                packs = entity.ProgramDetails.ToList().FirstOrDefault();
                             //  join p in entity.posSetting on S.id equals p.posSerialId
                       

           

            }



            return packs;
        }



        public ProgramDetailsModel getCustomerServerCode()
        {
            ProgramDetailsModel packs = new ProgramDetailsModel();
            using (incposdbEntities entity = new incposdbEntities())
            {
                packs = (from p in entity.ProgramDetails
                             //  join p in entity.posSetting on S.id equals p.posSerialId
                         select new ProgramDetailsModel
                         {
                             customerServerCode = p.customerServerCode,
                         }).FirstOrDefault();
            }
            return packs;
        }
        public int getSalesInvCountInMonth()
        {
            int invCount = 0;
            
            using (incposdbEntities entity = new incposdbEntities())
            {
                var expireDate = entity.ProgramDetails.Select(x => x.expireDate).Single();
                int expireDay = Convert.ToDateTime(expireDate).Day;               
                int currentMonth = DateTime.Now.Month;
                int currentYear = DateTime.Now.Year;
                int currentMonthDays =  DateTime.DaysInMonth(currentYear, currentMonth);

                if (expireDay > currentMonthDays)
                    expireDay = currentMonthDays;
                DateTime compaireDate2 = new DateTime(currentYear, currentMonth, expireDay);
                DateTime compairDate1 = compaireDate2.AddMonths(-1);

                // get sales imvoice count between compaireDate1 and compairDate2
                invCount = entity.invoices.Where(x => x.invType == "s" && x.updateDate >= compairDate1 && x.updateDate < compaireDate2).Count();

            }
            return invCount;
        }



    }
}