﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web.Mvc;
using System.Xml;
using System.Net.NetworkInformation;
using System.Net;
using System.Web;
using Web.Services.UserService;
using System.Web.Script.Serialization;
using Web.Attribute;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using OrderManager.Model.Models;
using OrderManager.Model.DTO;
using OrderManager.Web.Models;




namespace OrderManager.Web
{
    public class OrderController : BaseController
    {

        private IUserService UserService;
        static string CardCode = string.Empty;
        public OrderController()
        {
            UserService = new UserServiceClient();
        }

        public ViewResult Index(OderConditionModel condition, int? pageIndex = 0, int? pageSize = 10)
        {
            List<OM_Order> list = UserService.GetCurrentSalesOrderList(Cipher, CurrentUser.User.Guid);
            if (!string.IsNullOrWhiteSpace(condition.Id))
            {

                list = list.Where(s => s.CardName.Contains(condition.UserName ?? "") && s.CardCode.Contains(condition.UserCode ?? "")
                                        && s.DocStatus == condition.OrderStatus && s.Remarks.Contains(condition.Remarks ?? "")
                                        && s.DocEntry.ToString().Contains(condition.OrderEntry ?? "")).ToList();

                if (!string.IsNullOrWhiteSpace(condition.OrderDate))
                {
                    var dateRange = SplitDate(condition.OrderDate);
                    list = list.Where(s => s.DocDate >= dateRange.From && s.DocDate <= dateRange.To).ToList();
                }

                if (!string.IsNullOrWhiteSpace(condition.DeliverDate))
                {
                    var dateRange = SplitDate(condition.DeliverDate);
                    list = list.Where(s => s.DocDate >= dateRange.From && s.DocDate <= dateRange.To).ToList();
                }

                ViewBag.Condition = condition;
            }
            else
            {
                ViewBag.Condition = new OderConditionModel();
            }

            ViewBag.PageSize = pageSize;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = Math.Ceiling(Convert.ToDouble(list.Count) / Convert.ToDouble(pageSize));
            var result = list.Skip(Convert.ToInt32(pageIndex * pageSize)).Take((int)pageSize).ToList();
            return View("~/views/order/index.cshtml", result);
        }

        private DateRange SplitDate(string dateStr)
        {
            var array = dateStr.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new DateRange { From = Convert.ToDateTime(array[0]), To = Convert.ToDateTime(array[1]) };
            return result;
        }


        public ViewResult OrderItem(string orderItemGuid)
        {
            var detail = UserService.GetSalesOrderAndDetail(Cipher, orderItemGuid);
            var status = Enum.Parse(typeof(OrderManager.Model.Models.OM_DocStatusEnum), detail.DocStatus);
            ViewBag.Status = status;
            ViewBag.DocDueDate = detail.DocDueDate == null ? " " : Convert.ToDateTime(detail.DocDueDate).ToString("yyyy.MM.dd");
            ViewBag.DocDate = Convert.ToDateTime(detail.DocDate).ToString("yyyy.MM.dd");
            return View("~/views/order/order.cshtml", detail);
        }


        public ViewResult Order()
        {
            ViewBag.DocDate = DateTime.Now.ToString("yyyy.MM.dd");
            return View();
        }

        public ViewResult UserCodeList()
        {

            var list = UserService.GetCurrentUserByCardCode(Cipher, CurrentUser.User.Guid);

            ViewBag.PageSize = 10;
            ViewBag.PageIndex = 0;
            ViewBag.TotalPages = Math.Ceiling(Convert.ToDouble(list.Count) / Convert.ToDouble(10));
            var result = list.Skip(Convert.ToInt32(0)).Take(10).ToList();

            return View("~/views/order/UserCodeList.cshtml", result);

        }

        [HttpPost]
        public JsonResult UserCodeList(string key, int? pageindex)
        {
            var list = UserService.GetCurrentUserByCardCode(Cipher, CurrentUser.User.Guid);


            if (!string.IsNullOrWhiteSpace(key))
            {
                list = list.Where(s => s.Name.Contains(key) || s.Account.Contains(key.ToUpper())).ToList();
                ViewBag.Key = key;
            }

            var result = list.Skip(Convert.ToInt32(pageindex)).Take(10).ToList();
            return Json(new OrderManager.Web.Models.JsonModel { Data = result });
        }

        public ViewResult Search()
        {
            return this.View();
        }

        [HttpPost]
        public JsonResult GetCrystalData(string cardCode, string cardName, string startDate, string endDate)
        {
            this.HttpContext.Session["ReportName"] = "01 经销商对账报表.rpt";
            this.HttpContext.Session["CardCode"] = cardCode;
            this.HttpContext.Session["CardName"] = cardName;
            this.HttpContext.Session["StartDate"] = startDate;
            this.HttpContext.Session["EndDate"] = endDate;
            return Json(new OrderManager.Web.Models.JsonModel { Data = "success" });
        }

        public void ShowRptData()
        {
            try
            {
                string strReportName = System.Web.HttpContext.Current.Session["ReportName"].ToString();
                string cardCode = System.Web.HttpContext.Current.Session["CardCode"].ToString();
                string cardName = System.Web.HttpContext.Current.Session["CardName"].ToString();
                string startDate = System.Web.HttpContext.Current.Session["StartDate"].ToString();
                string endDate = System.Web.HttpContext.Current.Session["EndDate"].ToString();
                ReportDocument rd = new ReportDocument();
                string strRptPath = System.Web.HttpContext.Current.Server.MapPath("~/") + "Report//" + strReportName;
                rd.Load(strRptPath);
                TableLogOnInfo logInfo = new TableLogOnInfo();
                logInfo.ConnectionInfo.ServerName = ".";
                logInfo.ConnectionInfo.DatabaseName = "SBO_GS_TEST";
                logInfo.ConnectionInfo.UserID = "sa";
                logInfo.ConnectionInfo.Password = "avatech";
                rd.Database.Tables[0].ApplyLogOnInfo(logInfo);
                if (!string.IsNullOrEmpty(cardName))
                    rd.SetParameterValue("cardname", cardName);
                if (!string.IsNullOrEmpty(startDate))
                    rd.SetParameterValue("Sdate", Convert.ToDateTime(startDate));
                if (!string.IsNullOrEmpty(startDate))
                    rd.SetParameterValue("Edate", Convert.ToDateTime(endDate));
                rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, "crReport");

                Session["ReportName"] = null;
                Session["CardCode"] = null;
                Session["CardName"] = null;
                Session["StartDate"] = null;
                Session["EndDate"] = null;
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
        public ViewResult ProductList(string cardCode)
        {

            var list = UserService.GetProductList(Cipher, cardCode, "", 0);
            var count = UserService.GetProductListCount(Cipher, cardCode, "");
            CardCode = cardCode;
            ViewBag.PageSize = 5;
            ViewBag.PageIndex = 0;
            ViewBag.TotalPages = Math.Ceiling(Convert.ToDouble(count) / Convert.ToDouble(5));
            return View("~/views/order/ProductList.cshtml", list);

        }

        [HttpPost]
        public JsonResult ProductList(string key, int? pageindex)
        {
            var list = UserService.GetProductList(Cipher, CardCode, key, (int)pageindex);
            //var count = UserService.GetProductListCount(Cipher, CardCode, key);

            return Json(new OrderManager.Web.Models.JsonModel { Data = list });
        }


        public ViewResult ProductPrice(string productItemCode, string cardCode)
        {
            var list = UserService.GetCurrentProducePriceList(Cipher, productItemCode, cardCode);
            return View("~/views/order/productPrice.cshtml", list);

        }


        [HttpPost]
        public JsonResult SaveDraft(string obj)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            var orderDetail = jsonSerializer.Deserialize<OM_SalesOrderDataObject>(obj);
            try
            {
                orderDetail.User_Guid = CurrentUser.User.Guid;

                if (!string.IsNullOrWhiteSpace(orderDetail.Guid))
                {
                    UserService.UpdateSalesOrder(Cipher, orderDetail);
                }
                else
                {
                    orderDetail.Guid = Guid.NewGuid().ToString().ToUpper();
                    //orderDetail.Guid = UserService.SaveSalesOrder(Cipher, orderDetail);  // return order guid
                    UserService.SaveSalesOrder(Cipher, orderDetail);
                }
            }
            catch (Exception ex)
            {

                return Json(GetException(ex));
            }

            return Json(new OrderManager.Web.Models.JsonModel { Data = orderDetail.Guid });

        }


        [HttpPost]
        public JsonResult SubmitOrder(string orderGuid)
        {
            try
            {
                UserService.UpdateSalesOrderStatusByCommit(Cipher, orderGuid);
            }
            catch (Exception ex)
            {

                return Json(GetException(ex));
            }
            return Json(new OrderManager.Web.Models.JsonModel { Data = "已提交" });
        }

        [HttpPost]
        public JsonResult SubmitOrder2SAp(string orderGuid)
        {
            if (UserService.UpdateSalesOrderStatusByToSAP(Cipher, orderGuid))
            {
                return Json(new OrderManager.Web.Models.JsonModel { Data = "已对接", Code = 0 });
            }
            return Json(new OrderManager.Web.Models.JsonModel { Code = -2 });
        }
        [SkipLogin]
        public ActionResult DateTimePickerIframe()
        {
            return View();
        }

    }
}
