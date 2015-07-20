﻿using AutoMapper;
using OrderManager.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager.Model.DTO
{
    [DataContract]
    public class OM_ProductInfo
    {

        [DataMember]
        public string ItemCode { get; set; }

        [DataMember]
        public string ItemName { get; set; }

        [DataMember]
        public string Price { get; set; }


    }


}