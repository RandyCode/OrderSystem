
using OrderManager.Model.DTO;
using OrderManager.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager.Manager
{
    public interface IOrderManger
    {
        #region Save Method
        bool SaveProduct(OM_Product product);

        bool SaveSalesOrder(OM_Order saleOrder);
        bool SaveSalesOrderItem(OM_OrderItem saleOrderItem);
        #endregion

        #region Update Method
        bool UpdateProduct(OM_Product product);

        bool UpdateSalesOrderItem(OM_OrderItem saleOrderItem);
        #endregion

        #region Update Method
        bool DeleteProduct(Expression<Func<OM_Product, bool>> product);

        bool DeleteSalesOrderItem(Expression<Func<OM_OrderItem, bool>> saleOrderItem);
        #endregion

        #region Get ont or manay object


        OM_Product GetProduct(Expression<Func<OM_Product, bool>> fuc);

        OM_Order GetSalesOrder(Expression<Func<OM_Order, bool>> fuc);
        IList<OM_Order> GetSalesOrderList<Tkey>(PageListParameter<OM_Order, Tkey> parameter, out int count);

        OM_OrderItem GetSalesOrderItem(Expression<Func<OM_OrderItem, bool>> fuc);
        #endregion

        #region Fuction

        List<OM_ProductPrice> GetProducePricetList(Expression<Func<OM_ProductPrice, bool>> fuc);

        IList<OM_Product> GetProductList<Tkey>(PageListParameter<OM_Product, Tkey> parameter, out int count);
        /// <summary>
        /// 保存销售订单
        /// </summary>
        /// <param name="salesOrder"></param>
        /// <returns></returns>
        bool SaveSalesOrder(OM_SalesOrderDataObject salesOrder);
        /// <summary>
        /// 更新销售订单
        /// </summary>
        /// <param name="salesOrder"></param>
        /// <returns></returns>
        bool UpdateSalesOrder(OM_SalesOrderDataObject salesOrder);
        /// <summary>
        /// 获取当前单据对象的集合
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        OM_SalesOrderDataObject GetSalesOrderAndDetail(string salesOrder_Guid);

        /// <summary>
        /// 获取当前用户区域销售订单列表
        /// </summary>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        List<OM_Order> GetCurrentSalesOrderList(string userGuid);

        /// <summary>
        /// 获取当前区域的产品价格清单列表
        /// </summary>
        /// <param name="itemCode"></param>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        List<OM_ProductPrice> GetCurrentProducePriceList(string itemCode, string cardCode);


        /// <summary>
        /// 提交状态
        /// </summary>
        /// <param name="orderGuid"></param>
        /// <returns></returns>
        bool UpdateSalesOrderStatusByCommit(string orderGuid);
        /// <summary>
        /// 对接SAP状态
        /// </summary>
        /// <param name="orderGuid"></param>
        /// <returns></returns>
        OM_B1InfomationDTO UpdateSalesOrderStatusByToSAP(string orderGuid);

        List<OM_ProductInfo> GetChildProductRecursion(string cardCode, string itemCode, string userGuid);

        bool HasChildProduct(string cardCode, string itemCode);
        #endregion

    }
}
