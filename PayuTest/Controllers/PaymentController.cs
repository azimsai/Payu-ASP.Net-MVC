using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PayuTest.Models;
using System.Net;
using System.IO;
using System.Web.Mvc.Html;
namespace PayuTest.Controllers
{
    public class PaymentController : Controller
    {
        public string action1 = string.Empty;
        public string hash1 = string.Empty;
        public string txnid1 = string.Empty;

        [HttpGet]
        public ActionResult Pay()
        {
            PaymentDetails Details = new PaymentDetails();
            return View();
        }

        [HttpPost]
        public ActionResult Pay1(PaymentDetails Details)
        {
           
            try
            {
                TryUpdateModel(Details);
                if (ModelState.IsValid)
                {
                    Details.key = ConfigurationManager.AppSettings["MERCHANT_KEY"];
                    string[] hashVarsSeq;
                    string hash_string = string.Empty;
                    if (string.IsNullOrEmpty(Details.TxId)) // generating txnid
                    {
                        Random rnd = new Random();
                        string strHash = Generatehash512(rnd.ToString() + DateTime.Now);
                        txnid1 = strHash.ToString().Substring(0, 20);
                    }
                    else
                    {
                        txnid1 = Details.TxId;
                    }
                    if (string.IsNullOrEmpty(Details.Hash))
                    {
                        if (
                            string.IsNullOrEmpty(ConfigurationManager.AppSettings["MERCHANT_KEY"]) ||
                            string.IsNullOrEmpty(txnid1) ||
                            Details.Amount <=0 ||
                            string.IsNullOrEmpty(Details.FirstName) ||
                            string.IsNullOrEmpty(Details.Email) ||
                            string.IsNullOrEmpty(Details.PhoneNo) ||
                            string.IsNullOrEmpty(Details.ProductInfo)                            
                            )
                        {
                            return View(Details);
                        }
                        else
                        {
                            hashVarsSeq = ConfigurationManager.AppSettings["hashSequence"].Split('|'); // spliting hash sequence from config
                            hash_string = "";
                            foreach (string hash_var in hashVarsSeq)
                            {
                                if (hash_var == "key")
                                {
                                    hash_string = hash_string + ConfigurationManager.AppSettings["MERCHANT_KEY"];
                                    hash_string = hash_string + '|';
                                }
                                else if (hash_var == "txnid")
                                {
                                    hash_string = hash_string + txnid1;
                                    hash_string = hash_string + '|';
                                }
                                else if (hash_var == "amount")
                                {
                                    hash_string = hash_string + Convert.ToDecimal(Request.Form[hash_var]).ToString("g29");
                                    hash_string = hash_string + '|';
                                }
                                else
                                {

                                    hash_string = hash_string + (Request.Form[hash_var] != null ? Request.Form[hash_var] : "");// isset if else
                                    hash_string = hash_string + '|';
                                }
                            }

                            hash_string += ConfigurationManager.AppSettings["SALT"];// appending SALT

                            hash1 = Generatehash512(hash_string).ToLower();         //generating hash
                            action1 = ConfigurationManager.AppSettings["PAYU_BASE_URL"] + "/_payment";// setting URL
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(Details.Hash))
                {
                    hash1 = Details.Hash;
                    action1 = ConfigurationManager.AppSettings["PAYU_BASE_URL"] + "/_payment";
                }

                if (!string.IsNullOrEmpty(hash1))
                {
                    Details.Hash = hash1;
                    Details.TxId = txnid1;

                    System.Collections.Hashtable data = new System.Collections.Hashtable(); // adding values in gash table for data post
                    data.Add("hash", Details.Hash);
                    data.Add("txnid", Details.TxId);
                    data.Add("key", Details.key);
                    string AmountForm = Convert.ToDecimal(Details.Amount).ToString("g29");// eliminating trailing zeros
                    //amount.Text = AmountForm;
                    data.Add("amount", AmountForm);
                    data.Add("firstname", Details.FirstName.Trim());
                    data.Add("email", Details.Email.Trim());
                    data.Add("phone", Details.PhoneNo.Trim());
                    data.Add("productinfo", Details.ProductInfo.Trim());
                    data.Add("surl", "http://localhost:52125/Payment/PaymentResponse/".Trim());
                    data.Add("furl", "http://localhost:52125/Payment/PaymentResponse/".Trim());
                    data.Add("lastname", "".Trim());
                    data.Add("curl", "".Trim());
                    data.Add("address1", "".Trim());
                    data.Add("address2", "".Trim());
                    data.Add("city", "".Trim());
                    data.Add("state", "".Trim());
                    data.Add("country", "".Trim());
                    data.Add("zipcode", "".Trim());
                    data.Add("udf1", "".Trim());
                    data.Add("udf2", "".Trim());
                    data.Add("udf3", "".Trim());
                    data.Add("udf4", "".Trim());
                    data.Add("udf5", "".Trim());
                    data.Add("pg", "".Trim());
                    List<MvcHtmlString> lstMvcHtmlString = PreparePOSTForm(action1, data);

                    return View(lstMvcHtmlString);

                    //string strForm = PreparePOSTForm(action1, data);
                    //Page.Controls.Add(new LiteralControl(strForm));

                }

                else
                {
                    //no hash

                }

                return RedirectToAction("PaymentResponse");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult PaymentResponse()
        {

            try
            {

                string[] merc_hash_vars_seq;
                string merc_hash_string = string.Empty;
                string merc_hash = string.Empty;
                string order_id = string.Empty;
                string hash_seq = "key|txnid|amount|productinfo|firstname|email|udf1|udf2|udf3|udf4|udf5|udf6|udf7|udf8|udf9|udf10";


                if (Request.Form["status"] == "success")
                {

                    merc_hash_vars_seq = hash_seq.Split('|');
                    Array.Reverse(merc_hash_vars_seq);
                    merc_hash_string = ConfigurationManager.AppSettings["SALT"] + "|" + Request.Form["status"];


                    foreach (string merc_hash_var in merc_hash_vars_seq)
                    {
                        merc_hash_string += "|";
                        merc_hash_string = merc_hash_string + (Request.Form[merc_hash_var] != null ? Request.Form[merc_hash_var] : "");

                    }
                    merc_hash = Generatehash512(merc_hash_string).ToLower();



                    if (merc_hash != Request.Form["hash"])
                    {
                        //Value didn't match that means some paramter value change between transaction 
                        Response.Write("Hash value did not matched");

                    }
                    else
                    {
                        //if hash value match for before transaction data and after transaction data
                        //that means success full transaction  , see more in response
                        order_id = Request.Form["txnid"];

                        Response.Write("value matched");

                        //Hash value did not matched
                    }

                }

                else
                {

                    Response.Write("Hash value did not matched");
                    // osc_redirect(osc_href_link(FILENAME_CHECKOUT, 'payment' , 'SSL', null, null,true));

                }
            }

            catch (Exception ex)
            {
                Response.Write("<span style='color:red'>" + ex.Message + "</span>");

            }

            return View();

        }

        public string Generatehash512(string text)
        {

            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;

        }



        private List<MvcHtmlString> PreparePOSTForm(string url, System.Collections.Hashtable data)      // post form
        {
            HtmlHelper helper = this.GetHtmlHelper();
            List<MvcHtmlString> lstMvcHtmlString = new List<MvcHtmlString>();
            //Set a name for the form
           // string formID = "PostForm";
            //Build the form using the specified data to be posted.
            //StringBuilder strForm = new StringBuilder();
            //strForm.Append("<form id=\"" + formID + "\" name=\"" +
            //               formID + "\" action=\"" + url +
            //               "\" method=\"POST\">");

            foreach (System.Collections.DictionaryEntry key in data)
            {
                lstMvcHtmlString.Add(helper.Hidden(Convert.ToString(key.Key), key.Value));
                //strForm.Append("<input type=\"hidden\" name=\"" + key.Key +
                //               "\" value=\"" + key.Value + "\">");
            }


            //strForm.Append("</form>");
            //Build the JavaScript which will do the Posting operation.
            //StringBuilder strScript = new StringBuilder();


            //strScript.Append("<script language='javascript'>");
            //strScript.Append("var v" + formID + " = document." +
            //                 formID + ";");
            //strScript.Append("v" + formID + ".submit();");
            //strScript.Append("</script>");
            //Return the form and the script concatenated.
            //(The order is important, Form then JavaScript)
            return lstMvcHtmlString; //strForm.ToString() + strScript.ToString();
        }

        


    }


    public static class Extensions
    {
        //Exntesion method to get html helper for the controller
        public static HtmlHelper GetHtmlHelper(this Controller controller)
        {
            var viewContext = new ViewContext(controller.ControllerContext, new FakeView(), controller.ViewData, controller.TempData, TextWriter.Null);
            return new HtmlHelper(viewContext, new ViewPage());
        }

        public class FakeView : IView
        {
            public void Render(ViewContext viewContext, TextWriter writer)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
