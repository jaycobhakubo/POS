#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;

namespace GTI.Modules.POS.Data
{
    /// <summary>
    /// Represents a POS module feature.
    /// </summary>
    internal enum POSFeature
    {
        OpenDiscounts = 1,
        SaleLockOverride = 2,
        CreditCashOut = 3,
        QuantitySale = 6,
        B3Sales = 45,
        B3Redeem = 46,
        RefundPresales = 63 //US5709
    }

    /// <summary>
    /// Represents a Receipt Management module feature.
    /// </summary>
    internal enum ReceiptManagementFeature
    {
        VoidSale = 4,
    }

    /// <summary>
    /// Represents a function button type.
    /// </summary>
    internal enum Function
    {
        TransferUnit = 1
    }

    // TTP 50114
    /// <summary>
    /// Represents how sales tendering is performed.
    /// </summary>
    internal enum TenderSalesMode
    {
        Off = 0,
        AllowNegative = 1,
        WarnNegative = 2,
        PreventNegative = 3,
        Quick = 4
    }
}
