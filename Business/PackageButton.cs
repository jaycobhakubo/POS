#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

//US5117: POS: Automatically add package X when package Y has been added Z times

using System;
using System.Windows.Forms;
using System.Globalization;
using GTI.Modules.Shared;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.UI;
using GTI.Modules.POS.Properties;
using System.Collections.Generic;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// A menu button that represents a package.
    /// </summary>
    internal class PackageButton : MenuButton
    {
        #region Member Variables
        protected Package m_package;
        protected SessionInfo m_Session;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the PackageButton class.
        /// </summary>
        /// <param name="pos">The instance of the PointOfSale class.</param>
        /// <param name="package">The package to associate with this 
        /// button.</param>
        /// <exception cref="System.ArgumentNullException">pos is a null 
        /// reference.</exception>
        /// <exception cref="System.ArgumentNullException">package is a null 
        /// reference.</exception>
        public PackageButton(PointOfSale pos, Package package, SessionInfo session)
            : base(pos)
        {
            if(package == null)
                throw new ArgumentNullException("package");

            m_package = package;

            if (session == null)
                throw new ArgumentNullException("session");

            m_Session = session;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Handles when the package button is clicked.
        /// </summary>
        /// <param name="sender">Any object that implements IWin32Window 
        /// that represents the top-level window that will own any modal 
        /// dialog boxes.</param>
        /// <param name="argument">The quantity of packages to add the 
        /// sale.</param>
        public override void Click(IWin32Window sender, object argument)
        {
            int quantity = 1;
            int auditNumber = -1;
            string serialNumber = "";
            int cardCount = 0;
            bool udpateDiscounts = true;

            if (argument != null)
            {
                //check the length of the argument
                if (argument is int)
                {
                    quantity = (int)argument;
                }
                else if (argument is object[])
                {
                    object[] values = (object[])argument;

                    //US5117
                    //add a parameter if the package was auto added from a discount,
                    //then we do not want to update discounts
                    if (values.Length == 2)
                    {
                        if (values[0] != null)
                            quantity = (int)values[0];
                        if (values[1] != null)
                            udpateDiscounts = (bool)values[1];
                    }
                    else if (values.Length == 4)
                    {
                        if (values[0] != null)
                            quantity = (int)values[0];
                        if (values[1] != null)
                            auditNumber = (int)values[1];
                        if (values[2] != null)
                            serialNumber = (string)values[2];
                        if (values[3] != null)
                            cardCount = (int)values[3];
                    }
                }
            }

            // Make a clone of the package to add to the sale.
            Package clone = new Package(m_package);

            // PDTS 964
            // Add and check for any optional products.
            clone.CloneProducts(m_package, sender, m_pos);

            // Does this package contain any open credit products?
            Product[] products = clone.GetProducts();

            for(int x = 0; x < products.Length; x++)
            {
                if(products[x].Type == ProductType.CreditRefundableOpen ||
                   products[x].Type == ProductType.CreditNonRefundableOpen)
                {
                    DisplayMode displayMode;
                    bool use00;

                    lock(m_pos.Settings.SyncRoot)
                    {
                        displayMode = m_pos.Settings.DisplayMode;
                        use00 = m_pos.Settings.Use00ForCurrencyEntry;
                    }

                    // Prompt for the credit amount.
                    KeypadForm amountForm = new KeypadForm(m_pos, displayMode, false);
                    amountForm.GetKeypad().Use00AsDecimalPoint = !use00;

                    // Rally TA7464
                    if(m_pos.CurrentSale != null)
                    {
                        amountForm.CurrencySymbol = m_pos.CurrentSale.SaleCurrency.Symbol;
                        amountForm.Message = string.Format(CultureInfo.CurrentCulture, Resources.OpenCreditAmount, m_pos.CurrentSale.SaleCurrency.ISOCode);
                    }
                    else
                    {
                        amountForm.CurrencySymbol = m_pos.DefaultCurrency.Symbol;
                        amountForm.Message = string.Format(CultureInfo.CurrentCulture, Resources.OpenCreditAmount, m_pos.DefaultCurrency.ISOCode);
                    }

                    amountForm.BigButtonText = Resources.ButtonOk;

                    amountForm.ShowDialog(sender);

                    // Rally TA7464
                    if(m_pos.CurrentSale != null)
                        products[x].Price = m_pos.CurrentSale.RemoveSalesExchangeRate((decimal)amountForm.Value);
                    else
                        products[x].Price = (decimal)amountForm.Value;
                    
                    amountForm.Dispose();
                }

                if (products[x].Type == ProductType.Paper && serialNumber.Length > 0 && auditNumber != -1)
                {
                    PaperBingoProduct paperProduct = products[x] as PaperBingoProduct;

                    if (paperProduct != null)
                    {
                        PaperPackInfo info = new PaperPackInfo();

                        info.AuditNumber = auditNumber;
                        info.SerialNumber = serialNumber;
                        paperProduct.PackInfo.Add(info);

                        if (paperProduct.BarcodedPaper && cardCount != 0)
                            paperProduct.CardCount = (short)cardCount;
                    }
                }
            }

            // Are there any Crystal Ball Bingo products we have to process 
            // those before adding the package to the sale?
            // Rally US505
            IEnumerable<CrystalBallCardCollection> cbbCards = null;

            if(clone.HasCrystalBall)
                cbbCards = m_pos.CrystalBallManager.ProcessCrystalBall(m_pos.CurrentSale, clone, quantity, m_pos.WeAreAPOSKiosk && m_pos.SellingForm.KioskForm != null? m_pos.SellingForm.KioskForm : sender);

            m_pos.AddSaleItem(m_pos.CurrentSession, clone, quantity, m_isPlayerRequired, cbbCards, udpateDiscounts);//US5117
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the package associated with this button.
        /// </summary>
        public Package Package
        {
            get
            {
                return m_package;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("Package");

                m_package = value;
            }
        }

        public SessionInfo Session
        {
            get
            {
                return m_Session;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Session");

                m_Session = value;
            }
        }
        #endregion
    }
}
