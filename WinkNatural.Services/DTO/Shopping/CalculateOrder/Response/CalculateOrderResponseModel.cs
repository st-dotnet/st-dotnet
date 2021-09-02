﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.DTO.Shopping.CalculateOrder.Response
{
    public class CalculateOrderResponseModel
    {
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public List<OrderDetailResponse> Details { get; set; }

        public IEnumerable<ShipMethod> ShipMethods { get; set; }
    }

    public class OrderDetailResponse
    {
        public Guid UniqueIdentifier { get; set; }
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }

        public int ItemID { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceEach { get; set; }
        public decimal PriceTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal WeightEach { get; set; }
        public decimal Weight { get; set; }
        public decimal BVEach { get; set; }
        public decimal BV { get; set; }
        public decimal CVEach { get; set; }
        public decimal CV { get; set; }
        public bool IsVirtual { get; set; }

        public string ImageUrl { get; set; }

        public decimal Other1Each { get; set; }
        public decimal Other1 { get; set; }
        public decimal Other2Each { get; set; }
        public decimal Other2 { get; set; }
        public decimal Other3Each { get; set; }
        public decimal Other3 { get; set; }
        public decimal Other4Each { get; set; }
        public decimal Other4 { get; set; }
        public decimal Other5Each { get; set; }
        public decimal Other5 { get; set; }
        public decimal Other6Each { get; set; }
        public decimal Other6 { get; set; }
        public decimal Other7Each { get; set; }
        public decimal Other7 { get; set; }
        public decimal Other8Each { get; set; }
        public decimal Other8 { get; set; }
        public decimal Other9Each { get; set; }
        public decimal Other9 { get; set; }
        public decimal Other10Each { get; set; }
        public decimal Other10 { get; set; }
    }
}
