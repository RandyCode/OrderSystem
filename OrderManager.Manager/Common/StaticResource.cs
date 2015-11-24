using OrderManager.Model.Models;
using System;
using System.Collections.Generic;

namespace OrderManager.Manager
{


 public class StaticResource
 {
     static StaticResource()
     {
         UserProductPrices = new List<OM_ProductPrice>();
     }

     public static List<OM_ProductPrice> UserProductPrices { get; set; }
 }


}
