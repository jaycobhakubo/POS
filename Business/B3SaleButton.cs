
using System.Drawing;
using System.Globalization;
using GTI.Modules.POS.Properties;
using GTI.Modules.POS.UI;
using GTI.Modules.Shared;

//US4381: (US4337) POS: Quick Sale buttons

namespace GTI.Modules.POS.Business
{
    internal class B3Button : MenuButton
    {
        private decimal m_credit;

        public B3Button(PointOfSale pos, int credit = 0)
            : base(pos)
        {
            //if 0 then button is not a quick sale button
            ButtonType = credit == 0 ? B3ButtonType.Credit : B3ButtonType.QuickSale;
            m_credit = credit;

            switch (ButtonType)
            {
                case B3ButtonType.Credit:

                    Text = string.Format("Enter Amount");
                    break;

                case B3ButtonType.QuickSale:

                    Text = string.Format("Quick Sale ${0}.00", credit);
                    break;
            }

            GraphicId = 5;
        }

        public B3Button(PointOfSale pos, B3ButtonType type)
            : base(pos)
        {
            ButtonType = type;
            if (type != B3ButtonType.RetrieveAccount)
            {
                return;
            }

            Text = "Retrieve   Account";

            GraphicId = 5;
        }

        internal enum B3ButtonType
        {
            QuickSale,
            Credit,
            RetrieveAccount
        }

        public override void Click(System.Windows.Forms.IWin32Window sender, object argument)
        {
            var quantity = 1;


            if (ButtonType == B3ButtonType.Credit)
            {
                DisplayMode displayMode;
                bool use00;

                lock (m_pos.Settings.SyncRoot)
                {
                    displayMode = m_pos.Settings.DisplayMode;
                    use00 = m_pos.Settings.Use00ForCurrencyEntry;
                }

                // Prompt for the credit amount.
                KeypadForm amountForm = new KeypadForm(m_pos, displayMode, false);
                amountForm.GetKeypad().Use00AsDecimalPoint = !use00;

                // Rally TA7464
                if (m_pos.CurrentSale != null)
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

                if ((decimal)amountForm.Value == 0) //ignore a $0 credit
                {
                    amountForm.Dispose();
                    return;
                }

                // Rally TA7464
                if (m_pos.CurrentSale != null)
                    m_credit = m_pos.CurrentSale.RemoveSalesExchangeRate((decimal)amountForm.Value);
                else
                    m_credit = (decimal)amountForm.Value;

                amountForm.Dispose();
            }
            else if (ButtonType == B3ButtonType.RetrieveAccount)
            {
               m_pos.ShowB3RetrieveAccountForm();
            }

            if (ButtonType != B3ButtonType.RetrieveAccount)
            {
                m_pos.AddSaleItem(m_pos.CurrentSession, quantity, new B3Credit { Amount = m_credit });    
            }
        }

        internal B3ButtonType ButtonType { get; private set; }
    }
}

