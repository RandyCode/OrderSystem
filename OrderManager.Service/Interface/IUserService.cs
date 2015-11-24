using OrderManager.Model.DTO;
using OrderManager.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace OrderManager.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IUserService" in both code and config file together.
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        OM_UserDetail Login(string userAccount, string password);

        [OperationContract]
        List<OM_LogDataObject> GetCurrentUserLogs(string cipher, string userId);

        [OperationContract]
        void ResetPassword(string cipher, string userGuid, string newPwd);

        [OperationContract]
        void UpdatePassword(string cipher, string userGuid, string oldPwd, string newPwd);

        [OperationContract]
        List<OM_User> GetCurrentUserList(string cipher, string userGuid);

        [OperationContract]
        OM_User GetUser(string cipher, string userGuid);

        [OperationContract]
        bool SaveMessageBoard(string cipher, OM_MessageBoard msgBoard);

        [OperationContract]
        IList<OM_ProductInfo> LoadProductSorts(string cipher,string parentCode = null);

        [OperationContract]
        List<OM_MessageBoard> GetCurrentUserMessageBoard(string cipher, string userId);

        [OperationContract]
        OM_MessageBoard GetMessageBoardModel(string cipher, string guid);

        [OperationContract]
        IList<OM_Order> GetOrderList(string cipher, string userGuid);

        [OperationContract]
        string SaveSalesOrder(string cipher, OM_SalesOrderDataObject obj);

        [OperationContract]
        void UpdateSalesOrder(string cipher, OM_SalesOrderDataObject obj);

        [OperationContract]
        OM_SalesOrderDataObject GetSalesOrderAndDetail(string cipher, string salesOrder_Guid);

        [OperationContract]
        List<OM_Order> GetCurrentSalesOrderList(string cipher, string userGuid);

        [OperationContract]
        List<OM_ProductPrice> GetCurrentProducePriceList(string cipher, string itemCode, string userGuid);

        [OperationContract]
        List<OM_User> GetCurrentUserByCardCode(string cipher, string userGuid);

        [OperationContract]
        IList<OM_ProductInfo> GetProductList(string cipher, string CardCode, string searchKey, int pageIndex,int pageSize,bool isLoad=false);  

        [OperationContract]
        int GetProductListCount(string cipher, string CardCode, string searchKey);


        [OperationContract]
        void UpdateSalesOrderStatusByCommit(string cipher, string orderGuid);

        [OperationContract]
        OM_B1InfomationDTO UpdateSalesOrderStatusByToSAP(string cipher, string orderGuid);
    }
}

