using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OrderManager.Model.Models
{
    /// <summary>
    /// ��ҳʵ�� 
    /// </summary>
    /// <typeparam name="T">ʵ��</typeparam>
    /// <typeparam name="Tkey">������������</typeparam>
    public sealed class PageListParameter<T,Tkey>
    {
        public Expression<Func<T, bool>> whereLambda { get; set; }
        public Expression<Func<T, Tkey>> orderByLambda { get; set; }
        public int pageSize { get; set; }
        public int pageIndex { get; set; }
        public bool isAsc { get; set; }
    }
}
