
using Microsoft.Practices.Unity;
using OrderManager.Common;
using OrderManager.Manager.Common;
using OrderManager.Model;
using OrderManager.Model.DTO;
using OrderManager.Model.Models;
using OrderManager.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager.Manager
{
    public class OrderManger : BaseManger, IOrderManger
    {
        [Dependency]
        public IUserManager userManager { get; set; }

        #region Save Method
        public bool SaveProduct(OM_Product product)
        {
            if (DbRepository.Add(product) > 0)
            {
                return true;
            }
            return false;
        }

        public bool SaveSalesOrder(OM_Order saleOrder)
        {
            if (DbRepository.Add(saleOrder) > 0)
            {
                return true;
            }
            return false;
        }
        public bool SaveSalesOrderItem(OM_OrderItem saleOrderItem)
        {
            if (DbRepository.Add(saleOrderItem) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Update Method
        public bool UpdateProduct(OM_Product product)
        {
            if (DbRepository.Update(product) > 0)
            {
                return true;
            }
            return false;
        }


        public bool UpdateSalesOrderItem(OM_OrderItem saleOrderItem)
        {
            if (DbRepository.Update(saleOrderItem) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Update Method
        public bool DeleteProduct(Expression<Func<OM_Product, bool>> product)
        {
            if (DbRepository.Delete(product) > 0)
            {
                return true;
            }
            return false;
        }


        public bool DeleteSalesOrderItem(Expression<Func<OM_OrderItem, bool>> saleOrderItem)
        {
            if (DbRepository.Delete(saleOrderItem) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region  Get one or manay Object


        public OM_Product GetProduct(Expression<Func<OM_Product, bool>> fuc)
        {
            return DbRepository.GetModel(fuc);

        }

        public OM_Order GetSalesOrder(Expression<Func<OM_Order, bool>> fuc)
        {
            return DbRepository.GetModel(fuc);

        }


        public IList<OM_Order> GetSalesOrderList<Tkey>(PageListParameter<OM_Order, Tkey> parameter, out int count)
        {
            var result = DbRepository.GetPagedList<OM_Order, Tkey>(parameter, out count);
            return result;
        }

        public IList<OM_OrderItem> GetSalesOrderItemList(Expression<Func<OM_OrderItem, bool>> fuc)
        {
            return DbRepository.GetList(fuc);

        }
        public OM_OrderItem GetSalesOrderItem(Expression<Func<OM_OrderItem, bool>> fuc)
        {
            return DbRepository.GetModel(fuc);

        }
        #endregion

        #region Function
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Tkey">sort 的排序字段类型</typeparam>
        /// <param name="parameter"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IList<OM_Product> GetProductList<Tkey>(PageListParameter<OM_Product, Tkey> parameter, out int count)
        {
            return DbRepository.GetPagedList(parameter, out count);

        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="salesOrder"></param>
        /// <returns></returns>
        public bool SaveSalesOrder(OM_SalesOrderDataObject salesOrder)
        {
            OM_Order order = salesOrder.ToDTO();
            var user = DbRepository.GetModel<OM_User>(s => s.Account.ToUpper() == salesOrder.CardCode.ToUpper() && s.IsDel == false);
            if (user == null)
                throw new GenericException("客户代码不存在，保持草稿失败");

            order.DocStatus = ((int)OM_DocStatusEnum.未提交).ToString();
            order.TotalPrice = salesOrder.TotalPrice;
            order.DocEntry = GetLastDocEntry();
            order.NoteNotice = GetShortRemarks(order.Remarks, 5);
            var salesOrderHead = DbRepository.Add(order);
            if (salesOrderHead <= 0)
                throw new GenericException("保存草稿失败");


            var orderResult = DbRepository.GetModel<OM_Order>(s => s.Guid == order.Guid);

            List<OM_OrderItem> items = new List<OM_OrderItem>();
            foreach (var item in salesOrder.SalesOrderLine)
            {
                OM_OrderItem oi = new OM_OrderItem()
                {
                    Guid = Guid.NewGuid().ToString().ToUpper(),
                    Currency = item.Currency,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    TotalPrice = (item.CustomerPrice ?? item.Price) * item.Quantity,
                    Remarks = item.Remarks,
                    Order_Guid = orderResult.Guid,
                    DocEntry = orderResult.DocEntry,
                    InnerPrice = item.InnerPrice,
                    CustomerPrice = item.CustomerPrice
                };
                items.Add(oi);
            }

            var salesOrderLine = DbRepository.AddRange(items);

            if (salesOrderLine <= 0)
                throw new GenericException("保存草稿失败");

            return true;
        }


        public int GetLastDocEntry()
        {
            var count = DbRepository.ExecuteQuery<int>("select COUNT(Guid) FROM [OrderManagement].[dbo].[OM_Order]");
            int result = count.FirstOrDefault() + 1;
            return result;
        }

        public string GetShortRemarks(string remark, int length)
        {
            if (remark.Length <= 5)
                return remark;
            else
                return remark.Substring(0, length) + "...";
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="salesOrder"></param>
        /// <returns></returns>
        public bool UpdateSalesOrder(OM_SalesOrderDataObject salesOrder)
        {
            OM_Order temp = salesOrder.ToDTO();

            var order = DbRepository.GetModel<OM_Order>(s => s.Guid == salesOrder.Guid);
            if (order == null)
                throw new GenericException("订单号不存在，修改草稿失败");

            var user = DbRepository.GetModel<OM_User>(s => s.Account.ToUpper() == salesOrder.CardCode.ToUpper() && s.IsDel == false);
            if (user == null)
                throw new GenericException("客户代码不存在，修改草稿失败");

            order.CardCode = salesOrder.CardCode;
            order.CardName = user.Name;
            order.Remarks = salesOrder.Remarks;
            order.DocDate = salesOrder.DocDate;
            order.DocDueDate = salesOrder.DocDueDate;
            order.NoteNotice = GetShortRemarks(order.Remarks, 5);
            var salesOrderHead = DbRepository.Update(order);
            if (salesOrderHead <= 0)
                throw new GenericException("修改草稿失败");

            List<OM_OrderItem> items = DbRepository.GetList<OM_OrderItem>(s => s.Order_Guid == salesOrder.Guid);
            List<OM_OrderItem> removeItem = new List<OM_OrderItem>();
            foreach (var item in salesOrder.SalesOrderLine)  //temp
            {
                foreach (var i in items)  //数据库
                {
                    if (string.IsNullOrWhiteSpace(item.Guid))
                    {
                        item.Guid = Guid.NewGuid().ToString();
                        item.DocEntry = salesOrder.DocEntry;
                        item.Order_Guid = salesOrder.Guid;
                        DbRepository.Add(item);
                    }
                    else
                    {  //不为空并且在数据库中找不到对应
                        var exist = salesOrder.SalesOrderLine.Where(s => s.Guid == i.Guid);
                        if ((exist == null || exist.Count() == 0) && !removeItem.Contains(i))
                        {
                            removeItem.Add(i);
                        }
                    }

                    if (i.Guid == item.Guid)
                    {
                        i.ItemCode = item.ItemCode;
                        i.ItemName = item.ItemName;
                        i.Remarks = item.Remarks;
                        i.InnerPrice = item.InnerPrice;
                        i.CustomerPrice = item.CustomerPrice;
                        i.Price = item.Price;
                        i.Quantity = item.Quantity;
                        i.TotalPrice = item.Price * item.Quantity;
                    }
                }
            }

            var salesOrderLine = DbRepository.UpdateRange(items);


            foreach (var delItem in removeItem)
            {
                DbRepository.RealDelete<OM_OrderItem>(s => s.Guid == delItem.Guid);
            }



            if (salesOrderLine <= 0)
                throw new GenericException("修改草稿失败");

            return true;
        }

        public bool UpdateSalesOrderStatusByCommit(string orderGuid)
        {
            var salesOrder = this.GetSalesOrder(s => s.Guid == orderGuid);

            if (salesOrder == null)
            {
                throw new GenericException("当前订单不存在,请检查数据");
            }
            if (salesOrder.DocDueDate == null)
            {
                throw new GenericException("当前订单未填写交货日期,请填写,保存为草稿后再提交单据");
            }

            salesOrder.DocStatus = ((int)OM_DocStatusEnum.已提交).ToString();
            if (DbRepository.Update(salesOrder) > 0)
            {
                return true;

            }

            throw new GenericException("更新失败,请联系系统管理员");
        }
        public OM_B1InfomationDTO UpdateSalesOrderStatusByToSAP(string orderGuid)
        {
            OM_B1InfomationDTO b1Infomation = null;
            try
            {
                var salesOrder = this.GetSalesOrder(s => s.Guid == orderGuid);
                if (salesOrder == null)
                {
                    throw new GenericException("当前订单不存在,请检查数据");
                }

                OM_SalesOrderDataObject salesOrderDataObject = salesOrder.ToDTO();
                salesOrderDataObject.SalesOrderLine = this.GetSalesOrderItemList(s => s.Order_Guid == orderGuid).ToList();
                if (salesOrderDataObject.SalesOrderLine == null)
                {
                    throw new GenericException("当前订单行不存在,请检查数据");
                }

                //对接SAP实现
                b1Infomation = SaveForSAP(salesOrderDataObject);
                if (b1Infomation.JFZStatus == OMDocStatus.Commit || b1Infomation.GSStatus == OMDocStatus.Commit)
                {
                    if (b1Infomation.GSCode == 200 && b1Infomation.JFZCode == 200)
                    {
                        salesOrder.DocStatus = ((int)OM_DocStatusEnum.已对接).ToString();
                        if (DbRepository.Update(salesOrder) > 0)
                        {
                            LogHelper.Info("Start Update SalesOrder scuess");
                            return b1Infomation;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                b1Infomation = new OM_B1InfomationDTO();
                b1Infomation.IsException = true;
                b1Infomation.ExceptionMessage = ex.ToString();
                LogHelper.Error(ex.ToString());
                throw new GenericException(ex.ToString());
            }

            return b1Infomation;

        }

        private OM_B1InfomationDTO SaveForSAP(OM_SalesOrderDataObject salesOrder)
        {
            bool gsResult = false;
            bool jfzResult = false;
            OM_B1InfomationDTO b1Informaion = new OM_B1InfomationDTO();
            OM_SalesOrderDataObject GSSalesOrder = new OM_SalesOrderDataObject();
            GSSalesOrder.Guid = salesOrder.Guid;
            GSSalesOrder.DocEntry = salesOrder.DocEntry;
            GSSalesOrder.CardCode = salesOrder.CardCode;
            GSSalesOrder.CardName = salesOrder.CardName;
            GSSalesOrder.DocDate = salesOrder.DocDate;
            GSSalesOrder.DocDueDate = salesOrder.DocDueDate;
            OM_SalesOrderDataObject JFZSalesOrder = new OM_SalesOrderDataObject();
            JFZSalesOrder.Guid = salesOrder.Guid;
            JFZSalesOrder.DocEntry = salesOrder.DocEntry;
            JFZSalesOrder.CardCode = salesOrder.CardCode;
            JFZSalesOrder.CardName = salesOrder.CardName;
            JFZSalesOrder.DocDate = salesOrder.DocDate;
            JFZSalesOrder.DocDueDate = salesOrder.DocDueDate;
            foreach (var item in salesOrder.SalesOrderLine)
            {
                var pruduct = GetProduct(c => c.ItemCode == item.ItemCode);
                if (pruduct.GroupType == "0")
                {
                    GSSalesOrder.SalesOrderLine.Add(item);
                }
                else
                {
                    JFZSalesOrder.SalesOrderLine.Add(item);
                }
            }
            if (GSSalesOrder.SalesOrderLine.Count <= 0)
            {
                b1Informaion.GSCode = 200;
                b1Informaion.GSStatus = OMDocStatus.Unchange;
                b1Informaion.GSMessage = "高山药业行数据为0，不允许添加";
                LogHelper.Error("GSCompany's salesline is zero");
                gsResult = true;
            }
            else
            {
                gsResult = B1Company.SBOCompany.SaveSalesOrderDraft(GSSalesOrder, b1Informaion);
            }
            if (JFZSalesOrder.SalesOrderLine.Count <= 0)
            {
                b1Informaion.JFZCode = 200;
                b1Informaion.JFZStatus = OMDocStatus.Unchange;
                b1Informaion.JFZMessage = "金方子行数据为0，不允许添加";
                LogHelper.Error("JFZCompany's salesline is zero");
                jfzResult = true;
            }
            else
            {
                LogHelper.Info("JFZCompany' has Start");
                LogHelper.Info(string.Format("JFZSalesOrder.SalesOrderLine.Count is :" + JFZSalesOrder.SalesOrderLine.Count.ToString()));
                jfzResult = B1Company.SBOCompany.SaveSalesOrderDraftToJFZ(JFZSalesOrder, b1Informaion);
            }
            if (gsResult & jfzResult)
            {
                B1Company.SBOCompany.Dispose();
                return b1Informaion;

            }
            return b1Informaion;

        }
        /// <summary>
        /// 获取当前单据对象的集合
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        public OM_SalesOrderDataObject GetSalesOrderAndDetail(string salesOrder_Guid)
        {
            OM_SalesOrderDataObject salesOrderDataObject = this.GetSalesOrder(s => s.Guid == salesOrder_Guid).ToDTO();
            salesOrderDataObject.SalesOrderLine.AddRange(this.GetSalesOrderItemList(s => s.Order_Guid == salesOrder_Guid));

            foreach (var item in salesOrderDataObject.SalesOrderLine)
            {
                salesOrderDataObject.TotalPrice += item.TotalPrice;
            }
            return salesOrderDataObject;
        }
        private IList<OM_Order> GetSalesOrderList(Expression<Func<OM_Order, bool>> fuc)
        {
            return DbRepository.GetList(fuc);

        }
        /// <summary>
        /// 获取当前用户区域销售订单列表
        /// </summary>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        public List<OM_Order> GetCurrentSalesOrderList(string userGuid)
        {
            OM_UserRole currentRole = userManager.GetUserRole(c => c.User_Guid.ToLower().Equals(userGuid.ToLower()));
            List<OM_Order> listSalesOrder = new List<OM_Order>();
            if (currentRole.Role_Guid.ToUpper().Equals("A0DBEF93-F1AA-4C3A-828B-6DE0C99DA0D8"))
            {
                listSalesOrder.AddRange(this.GetSalesOrderList(u => u.DocEntry >= 0).ToList());
            }
            else
            {
                List<OM_User> listUser = userManager.GetAreaRoles(userGuid);

                foreach (var user in listUser)
                {
                    listSalesOrder.AddRange(this.GetSalesOrderList(s => s.User_Guid == user.Guid));
                }
            }

            return listSalesOrder;
        }


        public List<OM_ProductPrice> GetProducePricetList(Expression<Func<OM_ProductPrice, bool>> fuc)
        {
            return DbRepository.GetList(fuc);

        }
        /// <summary>
        /// 获取当前区域的产品价格清单列表
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        public List<OM_ProductPrice> GetCurrentProducePriceList(string itemCode, string cardCode)
        {
            //List<OM_ProductPrice> listProductPrice = new List<OM_ProductPrice>();

            if (string.IsNullOrEmpty(cardCode) || cardCode == null)
            {
                throw new GenericException("客户代码不能为空");
            }

            var user = userManager.GetUser(u => u.Guid == cardCode);

            if (user == null)
            {
                throw new GenericException("当前客户不存在");
            }
            return this.GetProducePricetList(p => p.Product_ItemCode.Trim() == itemCode.Trim() & user.Guid.Trim().ToLower() == p.User_Guid.Trim().ToLower()).ToList();
        }


        public bool HasChildProduct(string cardCode, string itemCode)
        {
            var result = DbRepository.GetList<OM_Product>(a => a.CardCode == cardCode && a.ParentId == itemCode && a.ParentId != a.ItemCode && a.IsDel == false);

            if (result == null || result.Count == 0)
            {
                return false;
            }
            else
                return true;
        }


        public List<OM_ProductInfo> GetChildProductRecursion(string cardCode, string itemCode, string userGuid)
        {
            //&& a.ParentId!=a.ItemCode 把自己排除
            var result = DbRepository.GetList<OM_Product>(a => a.CardCode == cardCode && a.ParentId == itemCode && a.ParentId != a.ItemCode && a.IsDel == false);
         
            if (result == null || result.Count == 0)
            {
                return null;
            }
            else
            {
                var infos = new List<OM_ProductInfo>();
                foreach (var item in result)
                {

                    string price = null;
                    var nodes = GetChildProductRecursion(item.CardCode, item.ItemCode, userGuid);

                    if (nodes == null || nodes.Count == 0)
                    {
                        var exist = StaticResource.UserProductPrices.Find(s => s.Product_ItemCode == item.ItemCode);  //GetCurrentProducePriceList(item.ItemCode, userGuid).Select(a => a.Price.ToString("0.00")).FirstOrDefault();
                        if (exist != null)
                            price = exist.Price.ToString("0.00");
                    }

                    OM_ProductInfo product = new OM_ProductInfo();
                    product.Price = price;
                    product.ItemCode = item.ItemCode;
                    product.ItemName = item.ItemName;
                    product.ChildNode = nodes;
                    if (nodes != null)
                        product.HasChildNode = true;
                    infos.Add(product);

                }
                return infos;
            }
        }


        #endregion


    }
}
