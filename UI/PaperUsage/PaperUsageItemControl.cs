#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc.
#endregion

//US4955: POS > Paper Usage: Damaged
//US3516: Return inventory
//DE13701: Does not preserve date of damages

using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI.PaperUsage
{
    internal partial class PaperUsageItemControl : UserControl
    {
        #region Local Variables

        //paper usage item data
        private readonly PaperUsageItem m_item;
        private readonly IWin32Window m_owner;
        private readonly PointOfSale m_pointOfSale;

        private bool m_hasFocus;
        private readonly Color m_selectedBackgroundColor = Color.FromArgb(255, 39, 74, 117);
        private readonly Color m_unselectedBackgroundColor = Color.FromArgb(255, 95, 87, 83);
        #endregion

        #region Constructor

        public PaperUsageItemControl(IWin32Window owner, PointOfSale pointOfSale, PaperUsageItem item, bool isNewItem = false)
        {
            InitializeComponent();

            if (pointOfSale.Settings.DisplayMode is CompactDisplayMode)
            {
                this.SuspendLayout();
                this.ProductLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.ProductLabel.Location = new System.Drawing.Point(0, 8);
                this.ProductLabel.Size = new System.Drawing.Size(89, 17);
                this.SerialLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.SerialLabel.Location = new System.Drawing.Point(90, 8);
                this.SerialLabel.Size = new System.Drawing.Size(79, 17);
                this.QuantityLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.QuantityLabel.Location = new System.Drawing.Point(276, 8);
                this.QuantityLabel.Size = new System.Drawing.Size(40, 17);
                this.SkipsLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.SkipsLabel.Location = new System.Drawing.Point(318, 8);
                this.SkipsLabel.Size = new System.Drawing.Size(40, 17);
                this.ValueLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.ValueLabel.Location = new System.Drawing.Point(482, 6);
                this.ValueLabel.Size = new System.Drawing.Size(50, 17);
                this.StartAuditTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.StartAuditTextBox.Location = new System.Drawing.Point(180, 6);
                this.StartAuditTextBox.Size = new System.Drawing.Size(43, 21);
                this.EndAuditTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.EndAuditTextBox.Location = new System.Drawing.Point(227, 6);
                this.EndAuditTextBox.Size = new System.Drawing.Size(43, 21);
                this.BonanzaLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.BonanzaLabel.Location = new System.Drawing.Point(444, 8);
                this.BonanzaLabel.Size = new System.Drawing.Size(40, 17);
                this.PriceTextbox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.PriceTextbox.Location = new System.Drawing.Point(422, 6);
                this.PriceTextbox.Size = new System.Drawing.Size(55, 21);
                this.DamageButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.DamageButton.Location = new System.Drawing.Point(367, 6);
                this.DamageButton.Size = new System.Drawing.Size(40, 21);
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.Size = new System.Drawing.Size(536, 36);
                this.ResumeLayout(false);
                this.PerformLayout();
            }

            if (item == null)
            {
                return;
            }

            m_item = new PaperUsageItem();
            m_owner = owner;
            m_pointOfSale = pointOfSale;

            //attach events. 
            StartAuditTextBox.TextChanged += AuditTextBoxTextChanged;
            EndAuditTextBox.TextChanged += AuditTextBoxTextChanged;
            
            UpdatePaperUsageItem(item, isNewItem);
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> TextBoxFocusChanged;

        public event EventHandler<EventArgs> ItemModified;

        public event EventHandler<PaperUsageUnscannedChangedEventArgs> UnscannedChangedEvent;

        public event EventHandler<EventArgs> TotalModified;

        public event EventHandler<EventArgs> FocusEvent;

        #endregion

        #region Properties

        public string InventoryProductName
        {
            get
            {
                return ProductLabel.Text;
            }
        }

        public string SerialNumber
        {
            get
            {
                return SerialLabel.Text;
            }
        }

        public int StartAudit
        {
            get
            {
                int audit;
                if (int.TryParse(StartAuditTextBox.Text, out audit))
                {
                    return audit;
                }

                return 0;


            }
        }

        public int EndAudit
        {
            get
            {
                int audit;
                if (int.TryParse(EndAuditTextBox.Text, out audit))
                {
                    return audit;
                }

                return 0;
            }
        }

        public int Quantity
        {
            get
            {
                int quantity;
                if (int.TryParse(QuantityLabel.Text, out quantity))
                {
                    return quantity;
                }

                return 0;
            }
        }

        public int Skips
        {
            get
            {
                int skips;
                if (int.TryParse(SkipsLabel.Text, out skips))
                {
                    return skips;
                }

                return 0;
            }
        }

        public int Damaged
        {
            get
            {
                int damaged;
                if (int.TryParse(DamageButton.Text, out damaged))
                {
                    return damaged;
                }

                return 0;
            }
        }

        public decimal Price
        {
            get
            {
                decimal price;
                if (decimal.TryParse(PriceTextbox.Text, out price))
                {
                    return decimal.Round(price, 2);
                }

                return 0;
            }
        }

        public decimal Value
        {
            get
            {
                decimal total;
                if (decimal.TryParse(ValueLabel.Text, out total))
                {
                    return decimal.Round(total, 2);
                }

                return 0;
            }
        }

        public bool IsModified { get; private set; }

        public PaperUsageItem Item
        {
            get
            {
                return m_item;
            }
        }

        public bool IsValid
        {
            get { return string.IsNullOrEmpty(InvalidMessage); }
        }

        public string InvalidMessage { get; set; }
        
        #endregion

        #region Methods

        public void UpdatePaperUsageItem(PaperUsageItem item, bool isModified = false)
        {
            m_item.CopyValues(item);

            //detach events while we update the items.
            StartAuditTextBox.TextChanged -= AuditTextBoxTextChanged;
            EndAuditTextBox.TextChanged -= AuditTextBoxTextChanged;

            //init values for UI
            ProductLabel.Text = item.Name;
            ProductNameToolTip.SetToolTip(ProductLabel, item.Name);
            SerialLabel.Text = item.Serial;
            SerialNumberToolTip.SetToolTip(SerialLabel, item.Serial);
            StartAuditTextBox.Text = item.AuditStart.ToString();

            //set End audit
            var endAudit = item.AuditEnd + 1;
            //make sure end audit is not a skip
            for (var i = 1; i < item.SkipList.Count + 1; i++)
            {
                //if end audit is in a skip, then go increment and continue
                if (item.SkipList.Contains(item.AuditEnd + i))
                {
                    continue;
                }

                //found an audit number that is not in the skip list. Set and break
                endAudit = item.AuditEnd + i;
                break;
            }

            EndAuditTextBox.Text = endAudit.ToString();

            //quantity
            QuantityLabel.Text = item.Quantity.ToString();
            QuantityToolTip.SetToolTip(QuantityLabel, QuantityLabel.Text);

            //damaged total and tool tip
            DamageButton.Text = item.DamagedList.Count(i => i.AuditNumber >= item.AuditStart && i.AuditNumber <= endAudit).ToString();//DE13701
            DamagedToolTip.SetToolTip(DamageButton, "Press to add damages");

            //price
            PriceTextbox.Text = string.Format("{0}", decimal.Round(item.Price, 2).ToString("F"));

            //total value
            ValueLabel.Text = string.Format("{0}", 0);
            ValueToolTip.SetToolTip(ValueLabel, ValueLabel.Text);

            //hidden. Not supported yet
            BonanzaLabel.Text = item.BonazaTrades.ToString();

            //set default
            IsModified = isModified;
            m_hasFocus = false;

            //update total
            UpdateTotal();

            //get skip total
            var skipTotal = item.SkipList.Count(i => i >= item.AuditStart && i <= endAudit);
            SkipsLabel.Text = skipTotal.ToString();

            //add skips and set a tool tip
            if (skipTotal > 0)
            {
                const int maxAuditNumberPerLine = 10;
                var count = 0;
                StringBuilder stringBuilder = new StringBuilder("Skips: ");
                stringBuilder.AppendLine();
                foreach (var skipsNumber in item.SkipList.Where(i => i >= item.AuditStart && i <= endAudit))
                {
                    stringBuilder.Append(string.Format("{0}, ", skipsNumber));
                    count++;

                    if (count >= maxAuditNumberPerLine)
                    {
                        stringBuilder.AppendLine(string.Empty);
                        count = 0;
                    }
                }

                //set tooltip
                var toolTip = stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length - 2);
                SkipsToolTip.SetToolTip(SkipsLabel, toolTip);
            }


            //attach events. 
            StartAuditTextBox.TextChanged += AuditTextBoxTextChanged;
            EndAuditTextBox.TextChanged += AuditTextBoxTextChanged;
        }


        /// <summary>
        /// Handles the TextChanged event for the Start and End TextBox controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AuditTextBoxTextChanged(object sender, EventArgs e)
        {
            //check to see if value is valid
            if (!ValidatePaperUsageItem())
            {
                RaiseItemModifiedEvent();
                return;
            }

            //update item start/end # 
            m_item.AuditStart = StartAudit;
            m_item.AuditEnd = EndAudit - 1;

            UpdateQuantity();
            RaiseItemModifiedEvent();
        }

        /// <summary>
        /// Handles the event for Price TextBox text changed Event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PriceTextBoxTextChanged(object sender, EventArgs e)
        {
            UpdatePrice();
            RaiseItemModifiedEvent();
        }

        /// <summary>
        /// Handles the KeyPress event for the AuditNumberTextBox controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs"/> instance containing the event data.</param>
        private void AuditNumberTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsNumber(e.KeyChar) ||
                 (e.KeyChar == (char)Keys.Back) ||
                 (e.KeyChar == (char)Keys.Delete)) ||
                (e.KeyChar == '.'))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the KeyPress event for the PriceTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs"/> instance containing the event data.</param>
        private void PriceTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            //Only allow a max of seven characters
            if (char.IsNumber(e.KeyChar) && PriceTextbox.Text.Length >= 7)
            {
                e.Handled = true;
                return;
            } 

            //Only allow numeric values in the textbox
            if (!(char.IsNumber(e.KeyChar) ||
                (e.KeyChar == (char)Keys.Back) ||
                (e.KeyChar == (char)Keys.Delete) ||
                (e.KeyChar == (char)Keys.Enter)) ||
                ((e.KeyChar == '.') && PriceTextbox.Text.Contains("."))) //only allow one decimal character
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the DamageButton Button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void DamageButton_Click(object sender, EventArgs e)
        {
            UpdateSelected(true);
            var damagedForm = new PaperUsageDamageForm(m_owner, m_pointOfSale, this)
            {
                StartPosition = FormStartPosition.CenterScreen
            };

            //attach event. listen if unscanned audit number was changed
            damagedForm.UnscannedChangedEvent += (s, args) =>
            {
                var handler = UnscannedChangedEvent;
                if (handler != null)
                {
                    handler(s, args);
                }
            };

            //show dialog
            damagedForm.ShowDialog(this);

            //if not modified then return
            if (!damagedForm.IsModified)
            {
                return;
            }

            //update damaged button and return
            DamageButton.Text = m_item.DamagedList.Count(i => i.AuditNumber >= m_item.AuditStart && i.AuditNumber <= m_item.AuditEnd).ToString();//DE13701
            UpdateQuantity();
            RaiseItemModifiedEvent();
        }

        /// <summary>
        /// Users the control clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void UserControlClicked(object sender, EventArgs e)
        {
            UpdateSelected(true);
        }

        /// <summary>
        /// Handles TextBox enter event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TextBoxEnter(object sender, EventArgs e)
        {
            UpdateSelected(true);

            var handler = TextBoxFocusChanged;

            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        /// Validates the paper usage item.
        /// </summary>
        /// <returns></returns>
        private bool ValidatePaperUsageItem()
        {
            //clear validation message
            InvalidMessage = String.Empty;

            if (string.IsNullOrEmpty(StartAuditTextBox.Text))
            {
                InvalidMessage = "Start cannot be left blank";
                return false;
            }

            if (string.IsNullOrEmpty(EndAuditTextBox.Text))
            {
                InvalidMessage = "End cannot be left blank";
                return false;
            }

            //validation
            if (StartAudit > EndAudit)
            {
                InvalidMessage = "Start must be less than end";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the quantity.
        /// </summary>
        private void UpdateQuantity()
        {
            //update quantity label
            QuantityLabel.Text = m_item.Quantity.ToString();
            QuantityToolTip.SetToolTip(QuantityLabel, QuantityLabel.Text);
            
            //update skips
            SkipsLabel.Text = m_item.SkipList.Count(i => i >= m_item.AuditStart && i <= m_item.AuditEnd).ToString();

            //update total
            UpdateTotal();
        }

        /// <summary>
        /// Updates the price.
        /// </summary>
        private void UpdatePrice()
        {
            //update price
            m_item.Price = Price;

            //update total label
            UpdateTotal();
        }

        /// <summary>
        /// Updates the total.
        /// </summary>
        private void UpdateTotal()
        {
            //update value textbox
            ValueLabel.Text = string.Format("{0}", decimal.Round(Price * Quantity, 2).ToString("F"));
            ValueToolTip.SetToolTip(ValueLabel, ValueLabel.Text);

            //raise total modified event
            var handler = TotalModified;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the item modified event.
        /// </summary>
        private void RaiseItemModifiedEvent()
        {
            IsModified = true;

            //raise item modifiend event
            var handler = ItemModified;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Updates the selected.
        /// </summary>
        /// <param name="hasFocus">if set to <c>true</c> [has focus].</param>
        private void UpdateSelected(bool hasFocus)
        {
            //already has focus, do not need to update
            if (m_hasFocus == hasFocus)
            {
                return;
            }

            UpdateSelectedBackground(hasFocus);

            //raise event focus
            var handler = FocusEvent;
            if (handler != null)
            {
                handler(hasFocus ? this : null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Updates the selected background.
        /// </summary>
        /// <param name="hasFocus">if set to <c>true</c> [has focus].</param>
        public void UpdateSelectedBackground(bool hasFocus)
        {
            //update background 
            BackColor = hasFocus ? m_selectedBackgroundColor : m_unselectedBackgroundColor;

            //update flag
            m_hasFocus = hasFocus;
        }
        
        #endregion

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
