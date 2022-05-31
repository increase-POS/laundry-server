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
    [RoutePrefix("api/Activate")]
    public class ActivateController : ApiController
    {


        public string ServerID()
        {
            string deviceCode = "";
            // deviceCode = GetMotherBoardID() + "-" + GetHDDSerialNo();
            deviceCode =  GetHDDSerialNo();
            return deviceCode;
        }
        public static string GetMotherBoardID()
        {
            string mbInfo = String.Empty;
            ManagementScope scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            scope.Connect();
            ManagementObject wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

            foreach (PropertyData propData in wmiClass.Properties)
            {
                if (propData.Name == "SerialNumber")
                    mbInfo = Convert.ToString(propData.Value);
            }

            return mbInfo;
        }
        public static String GetHDDSerialNo()
        {
            ManagementClass mangnmt = new ManagementClass("Win32_LogicalDisk");
            ManagementObjectCollection mcol = mangnmt.GetInstances();
            string result = "";
            foreach (ManagementObject strt in mcol)
            {
                result += Convert.ToString(strt["VolumeSerialNumber"]);
            }
            return result;
        }
        public async Task<SendDetail> GetSerialsAndDetails(string packageSaleCode, string customerServerCode)
        {
            SendDetail item = new SendDetail();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("packageSaleCode", packageSaleCode);
            parameters.Add("customerServerCode", customerServerCode);

            //#################
            IEnumerable<Claim> claims = await APIResult.getList("packageUser/ActivateServer", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = JsonConvert.DeserializeObject<SendDetail>(c.Value, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });

                }
            }
            return item;

        }


        private int SaveProgDetails(packagesSend newObject)
        {
            int message = 0;
            if (newObject != null)
            {



                ProgramDetails tmpObject;


                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        var locationEntity = entity.Set<ProgramDetails>();

                      // List<ProgramDetails> Objectlist = entity.ProgramDetails.ToList();
                        tmpObject = entity.ProgramDetails.FirstOrDefault();

                        if (tmpObject != null)
                        {

                            tmpObject.updateDate =DateTime.Now;
        tmpObject.programName = newObject.programName;
                            tmpObject.branchCount = newObject.branchCount;
                            tmpObject.posCount = newObject.posCount;
                            tmpObject.userCount = newObject.userCount;
                            tmpObject.vendorCount = newObject.vendorCount;
                            tmpObject.customerCount = newObject.customerCount;
                            tmpObject.itemCount = newObject.itemCount;
                            tmpObject.saleinvCount = newObject.salesInvCount;
                            tmpObject.versionName = newObject.verName;
                            tmpObject.storeCount = newObject.storeCount;

                            tmpObject.packageSaleCode = newObject.packageSaleCode;
                            tmpObject.customerServerCode = newObject.customerServerCode;// from function
                            tmpObject.expireDate = newObject.endDate;
                            tmpObject.isOnlineServer = newObject.isOnlineServer;
                            // tmpObject.packageNumber = newObject.packageCode;


                        }
                        else
                        {
                            message = -1;
                        }



                        //  tmpObject.posSerial = newObject.posSerial;      public Nullable<int> docPapersizeId { get; set; }




                        message = entity.SaveChanges();



                        //  entity.SaveChanges();
                        //   return (message);
                    }
                    return (message);

                }
                catch
                {
                    message = -1;
                     return (message);
                  //  return (ex.ToString());
                }

            }
            else
            {
                return (-1);
            }

        }



        private int SaveposSerials(List<PosSerialSend> newObjectlist)
        {
            int message = 0;
            if (newObjectlist != null)
            {
                List<posSerials> poslist = new List<posSerials>();
                List<string> newserial = new List<string>();
                List<string> oldserial = new List<string>();
                foreach (PosSerialSend sendrow in newObjectlist)
                {

                    newserial.Add(sendrow.serial);

                    // poslist.Except
                }




                try
                {
                    using (incposdbEntities entity = new incposdbEntities())
                    {
                        //  var locationEntity = entity.Set<posSerials>();
                        List<posSerials> alllist = entity.posSerials.ToList();

                        oldserial = alllist.Select(s => s.posSerial).ToList();
                        List<string> finallist = new List<string>();
                        // for no duplicate 
                        finallist = newserial.Except(oldserial).ToList();
                        foreach (string sendrow in finallist)
                        {
                            posSerials positem = new posSerials();
                            positem.posSerial = sendrow;

                            poslist.Add(positem);

                        }


                        entity.posSerials.AddRange(poslist);


                        message = entity.SaveChanges();



                    }
                    return (message);

                }
                catch
                {
                    message = -1;
                    return (message);
                }

            }
            else
            {
                return (-1);
            }

        }



        // GET api/<controller>



        [HttpPost]
        [Route("checkconn")]
        public string checkconn(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request); var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                {
                    int id = 0;
                    IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                    foreach (Claim c in claims)
                    {
                        if (c.Type == "id")
                        {
                            id = int.Parse(c.Value);
                        }
                    }
                    if (id == 1)
                    {
                        return TokenManager.GenerateToken(2.ToString());
                    }
                    else
                    {
                        return TokenManager.GenerateToken(0.ToString());
                    }

                }
            }
        }

       

        [HttpPost]
        [Route("Sendserverkey")]
        public async Task<string> Sendserverkey(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                string res1 = "";
                string skey = "";
                string serverId = "";
                SendDetail sendDetailItem = new SendDetail();
                int res = 0;
                int tempres = 0;
                IEnumerable<Claim> claims = TokenManager.getTokenClaims(token);
                foreach (Claim c in claims)
                {
                    if (c.Type == "skey")
                    {
                        skey = c.Value;
                    }
                }
 try
                {
                  //  return TokenManager.GenerateToken("1212".ToString());
                   serverId = ServerID();
                   // serverId = "server13213ascas";


                    int conres=  await  checkIncServerConn();

                    // check con to increase server
                    if (conres > 0)
                    {


                       // return TokenManager.GenerateToken(conres.ToString());

                        sendDetailItem = await GetSerialsAndDetails(skey, serverId);
                        //update server detail
                        // return TokenManager.GenerateToken(sendDetailItem);
                        if (sendDetailItem.packageSend.posCount == -2)
                        {
                            // serial is booked
                            res = -2;

                        } else if(sendDetailItem.packageSend.posCount == -3)
                        {
                            //serial not found
                            res = -3;
                        }
                        else
                        {
                            sendDetailItem.packageSend.customerServerCode = serverId;
                            sendDetailItem.packageSend.packageSaleCode = skey;

                            tempres = SaveProgDetails(sendDetailItem.packageSend);
                            //    return TokenManager.GenerateToken(res1);
                            //update serials 
                            if (tempres >= 0)
                            {
                                res += 1;
                                tempres = 0;
                                tempres = SaveposSerials(sendDetailItem.PosSerialSendList);
                            }
                            if (tempres >= 0)
                            {
                                res += 1;
                            }
                            else
                            {
                                // activation error
                                res = 0;
                            }
                        }
                   
                    }
                    else
                    {
                        // connection error
                        res = -1;
                    }
                    return TokenManager.GenerateToken(res);
                }
                catch (Exception ex)
                {
                    // connection error
                 // return TokenManager.GenerateToken(-1);
                  return TokenManager.GenerateToken(ex.ToString());
                }

            }
        
            }

        public async Task<int> checkIncServerConn()
        {
            int id = 1;
            int item = 0;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("id", id.ToString());
            //#################
            IEnumerable<Claim> claims = await APIResult.getList("packageUser/checkconn", parameters);

            foreach (Claim c in claims)
            {
                if (c.Type == "scopes")
                {
                    item = int.Parse(c.Value);
                    break;
                }
            }
            return item;



        }

        [HttpPost]
        [Route("CheckPeriod")]
        public string CheckPeriod(string token)
        {
            token = TokenManager.readToken(HttpContext.Current.Request);
            var strP = TokenManager.GetPrincipal(token);
            if (strP != "0") //invalid authorization
            {
                return TokenManager.GenerateToken(strP);
            }
            else
            {
                int res = CheckPeriod();
                return TokenManager.GenerateToken(res.ToString());

            }
        }

        public int CheckPeriod()
        {
            ProgramDetails tmpObject;
            // 1 :  time not end-
            //  0 : time is end 

            try
            {
                using (incposdbEntities entity = new incposdbEntities())
                {
                    var locationEntity = entity.Set<ProgramDetails>();
                    tmpObject = entity.ProgramDetails.FirstOrDefault();
                    if (tmpObject != null)
                    {
                        if (tmpObject.isLimitDate == false)
                        {
                            return 1;
                        }
                        else
                        {// limited
                            if (tmpObject.expireDate <= DateTime.Now || tmpObject.expireDate == null)
                            {
                                return 0;
                            }
                            else
                            {
                                return 1;
                            }
                        }
                    }
                    else
                    {
                        return -1;
                    }
                }

            }
            catch
            {

                return -1;

            }

        }

    }
}