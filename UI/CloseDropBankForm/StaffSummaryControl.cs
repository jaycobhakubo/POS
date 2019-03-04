#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc
#endregion

//US4436: Close a bank from the POS

using System.Globalization;
using System.Windows.Forms;

namespace GTI.Modules.POS.UI
{
    public partial class StaffSummaryControl : UserControl
    {
        #region constructor
        public StaffSummaryControl(string title, decimal value)
        {
            InitializeComponent();

            NameLabel.Text = title;
            ValueLabel.Text = string.Format("{0:C}", value);
        }
        #endregion

        #region properties
        public decimal Value
        {
            get
            {
                decimal valueAmount;
                decimal.TryParse(ValueLabel.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out valueAmount);
                return valueAmount;
            }
            set
            {
                ValueLabel.Text = string.Format("{0:C}", value);
            }
        }
        #endregion

    }
}
