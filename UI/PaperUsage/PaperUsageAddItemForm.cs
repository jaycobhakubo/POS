#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc.
#endregion

//US4955: POS > Paper Usage: Damaged
//DE13701: Does not preserver date of damaged items

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI.PaperUsage
{
    internal partial class PaperUsageAddItemForm : EliteGradientForm
    {
        #region Local Fields
        private readonly PointOfSale m_pointOfSale;
        private readonly PaperUsageForm m_paperUsageForm;

        private string m_currentSerialListProductName;
        private Control m_activeControl;
        
        #endregion

        #region Constructor
        internal PaperUsageAddItemForm(PointOfSale pointOfSale, PaperUsageForm paperUsageForm)
        {
            InitializeComponent();

            m_pointOfSale = pointOfSale;
            m_paperUsageForm = paperUsageForm;

            SerialComboBox.DisplayMember = "Item1";

            StartAuditTextBox.Text = string.Empty;

            AddButton.Enabled = false;
            
            SerialComboBox.Focus();
        }
        #endregion
        
        #region Properies

        public bool IsModified { get; private set; }
        
        public PaperUsageItem NewItem { get; private set; }
        
        private List<Tuple<string, string>> ProductSerialList { get; set; }
        
        #endregion

        #region Methods

        private void PaperUsageAddItemForm_Load(object sender, EventArgs e)
        {
            m_pointOfSale.RunWorker(Resources.WaitFormGetPaperUsage, GetInventorySerialNumbers, null, GetInventorySerialNumbersComplete);
            m_pointOfSale.ShowWaitForm(this); // window has to be initialized first when sending "this" as a parent. Halts here until background worker is complete
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var serial = SerialComboBox.Text;
            var auditStart = int.Parse(StartAuditTextBox.Text);
            var args = new object[2];
            args[0] = auditStart;
            args[1] = serial;

            //make sure the start number doesn't overlap existing items
            foreach (var control in m_paperUsageForm.PaperUsageFlowPanel.Controls.OfType<PaperUsageItemControl>())
            {
                //see if the serial numbers match
                if (control.Item.Serial != serial)
                {
                    continue;
                }

                if ((control.Item.AuditStart <= auditStart &&
                    control.Item.AuditEnd >= auditStart) ||
                    (control.Item.AuditStart == auditStart &&
                    control.Item.AuditEnd == auditStart-1))
                {
                    errIcon.Visible = true;
                    ErrorMessageLabel.Visible = true;
                    ErrorMessageLabel.Text = string.Format("Serial {0} cannot have overlapping audit ranges", control.Item.Serial);
                    return;
                }
            }


            m_pointOfSale.RunWorker(Resources.WaitFormGetPaperUsage, GetInventoryItemDetails, args, GetInventoryItemDetailsComplete);
            m_pointOfSale.ShowWaitForm(this); // window has to be initialized first when sending "this" as a parent. Halts here until background worker is complete

            if (NewItem == null)
            {
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SerialComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Keys)e.KeyChar == Keys.Enter)
            {
                SendKeys.Send("{Tab}");
            }
        }

        private void SerialComboBox_TextChanged(object sender, EventArgs e)
        {
            //if not valid serial, then clear product name and disable button
            if (!IsValidSerialNumber(SerialComboBox.Text))
            {
                ProductNameLabel.Text = string.Empty;
                EnableAddButton();
            }
        }

        private void SerialComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //unbox
            var tuple = SerialComboBox.SelectedItem as Tuple<string, string>;

            if (tuple != null)
            {
                //set product name
                ProductNameLabel.Text = tuple.Item2;
                //enable button
                EnableAddButton();
            }
        }

        private void StartAuditTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //only allow numeric
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            } 
            
            if ((Keys)e.KeyChar == Keys.Enter && AddButton.Enabled)
            {
                AddButton_Click(this, EventArgs.Empty);
            }
        }

        private void StartAuditTextBox_TextChanged(object sender, EventArgs e)
        {
            EnableAddButton();
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            errIcon.Visible = false;
            ErrorMessageLabel.Visible = false;
            ErrorMessageLabel.Text = String.Empty;
            
            m_activeControl = sender as Control;
        }

        private void EnableAddButton()
        {
            //make sure serial and start are valid
            if (IsValidSerialNumber(SerialComboBox.Text) &&
                !string.IsNullOrEmpty(StartAuditTextBox.Text))
            {
                AddButton.Enabled = true;
            }
            else
            {
                AddButton.Enabled = false;
            }
        }

        private bool IsValidSerialNumber(string serial)
        {
            foreach (var item in SerialComboBox.Items)
            {
                var tuple = item as Tuple<string, string>;

                if (tuple != null && tuple.Item1 == serial)
                {
                    return true;
                }
            }
            return false;
        }

        private void GetInventoryItemDetails(object sender, DoWorkEventArgs e)
        {
            var args = e.Argument as object[];
            if (args == null)
            {
                return;
            }

            var auditStart = (int)args[0];
            var serial = (string)args[1];
                
            GetInventoryItemBySerialAuditMessage message;
            
            try
            {
                message = new GetInventoryItemBySerialAuditMessage(serial, auditStart);
                message.Send();
            }
            catch (ServerException ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetPaperUsageFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            if (message.ReturnCode == 0)
            {
                if (message.InventoryItemId == 0)
                {
                    throw new Exception("Unable to find product serial with audit range");
                }

                //box results into object array
                var resultObj = new object[5];
                resultObj[0] = message.InventoryItemId;
                resultObj[1] = message.ProductName;
                resultObj[2] = string.IsNullOrEmpty(message.Price) ? 0.ToString() : message.Price;
                resultObj[3] = auditStart;
                resultObj[4] = serial;

                e.Result = resultObj;
            }
        }

        private void GetInventoryItemDetailsComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                int inventoryId;
                string productName;
                string price;
                int auditStart;
                string serial;

                try
                {
                    //result data
                    var resultObj = (object[])e.Result;
                    inventoryId = (int)resultObj[0];
                    productName = (string)resultObj[1];
                    price = (string)resultObj[2];
                    auditStart = (int)resultObj[3];
                    serial = (string) resultObj[4];
                }
                catch (Exception)
                {
                    throw new Exception("Unable to cast result set from GetInventoryItemDetails");
                }

                NewItem = new PaperUsageItem()
                {
                    Name = productName,
                    InventoryItemId = inventoryId,
                    Price = decimal.Parse(price),
                    AuditStart = auditStart,
                    AuditEnd = auditStart-1,
                    DamagedList = new List<PaperUsageDamagedItem>(),
                    CanBeRemovedDamagedList = new List<int>(),
                    BonazaTrades = 0,
                    SkipList = new List<int>(),
                    UnscannedList = new List<int>(),
                    Serial = serial
                };
            }
            else
            {
                errIcon.Visible = true;
                ErrorMessageLabel.Visible = true;
                ErrorMessageLabel.Text = e.Error.Message;
            }

            m_pointOfSale.CloseWaitForm();
        }

        private void GetInventorySerialNumbers(object sender, DoWorkEventArgs e)
        {
            //load invetory serial numbers
            GetInventorySerialNumbersMessage message;

            try
            {
                message = new GetInventorySerialNumbersMessage(true);
                message.Send();
            }
            catch (ServerException ex)
            {
                throw new POSException(string.Format(CultureInfo.CurrentCulture, Resources.GetPaperUsageFailed, ServerExceptionTranslator.FormatExceptionMessage(ex)), ex);
            }

            if (message != null)
            {
                e.Result = message.InventoryItems;
            }
        }

        private void GetInventorySerialNumbersComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                var inventoryItems = (List<Tuple<string, string>>)e.Result;
                ProductSerialList = inventoryItems;

                //UpdateProductComboBoxList();
                //UpdateSerialComboBoxList();
                SerialComboBox.DataSource = ProductSerialList;
                //ProductComboBox.DataSource = ProductSerialList;
                SerialComboBox.DisplayMember = "Item1";
                SerialComboBox.SelectedIndex = -1;
                //ProductComboBox.DisplayMember = "Item2";
            }

            m_pointOfSale.CloseWaitForm();
        }


        #region KeyPad Events

        private void PressThisKey(object sender, EventArgs e)
        {
            m_activeControl.Focus();

            //check to see if a combobox
            if (m_activeControl is ComboBox)
            {
                //Once focuse, combobox will auto select all text, so we want to go to end
                SendKeys.Send("{End}");
            }
                
            SendKeyPress(((Button)sender).Text);
        }

        private void ClearButtonClick(object sender, EventArgs e)
        {
            if (m_activeControl is TextBox)
            {
                m_activeControl.Text = string.Empty;
            }
            else if (m_activeControl is ComboBox)
            {
                m_activeControl.Text = String.Empty;
            }
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            m_activeControl.Focus();
            
            //check to see if a combobox
            if (m_activeControl is ComboBox)
            {
                //Once focuse, combobox will auto select all text, so we want to go to end
                SendKeys.Send("{End}");
            }
            SendKeyPress("\x08");
        }

        private void SendKeyPress(string number)
        {
            SendKeys.SendWait(number);
        }
        #endregion

        private void SerialComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{Tab}");
            }

        }

        #endregion
    }
}
