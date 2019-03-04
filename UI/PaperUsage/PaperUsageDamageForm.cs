#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2016 FortuNet, Inc.
#endregion

//US4955: POS > Paper Usage: Damaged
//DE13701: Does not preserver date of damaged items

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI.PaperUsage
{
    internal partial class PaperUsageDamageForm : EliteGradientForm
    {
        #region Local Fields

        private readonly PaperUsageItemControl m_paperUsageItemControl;
        private readonly PaperUsageItem m_paperUsageItem;
        private readonly IWin32Window m_owner;
        private readonly PointOfSale m_pointOfSale;
        #endregion

        #region Constructor
        internal PaperUsageDamageForm(IWin32Window owner, PointOfSale pointOfSale, PaperUsageItemControl paperItemControl)
        {
            InitializeComponent();

            m_owner = owner;
            m_pointOfSale = pointOfSale;
            m_paperUsageItem = paperItemControl.Item;
            m_paperUsageItemControl = paperItemControl;

            NameLabel.Text = string.Format("{0} {1}", NameLabel.Text, m_paperUsageItem.Name);
            SerialLabel.Text = string.Format("{0} {1}", SerialLabel.Text, m_paperUsageItem.Serial);
            //clear
            DamagedListBox.Items.Clear();
            
            ////add damaged to list
            foreach (var damaged in m_paperUsageItem.DamagedList)
            {
                AddItemToSortedList(damaged);
            }

            DamagedListBox.SelectedItem = null;
            RemoveButton.Enabled = false;
        }
        #endregion

        #region Events

        public event EventHandler<PaperUsageUnscannedChangedEventArgs> UnscannedChangedEvent;

        #endregion

        #region Properies
        public bool IsModified { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Handles the Click event of the Add Damaged button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AddDamagedClick(object sender, EventArgs e)
        {
            int audit;
            if (!int.TryParse(AuditNumberTextBox.Text, out audit))
            {
                return;
            }

            if (audit <= 0)
            {
                return;
            } 
            
            var damagedItem = new PaperUsageDamagedItem
            {
                AuditNumber = audit
            };

            //DE13701
            //add to paper usage item
            if (!m_paperUsageItem.DamagedList.Exists(i => i.AuditNumber == audit))
            {
                if (m_paperUsageItem.IsBarcodedPaper)
                {
                    //need to remove if in skipped list
                    if (m_paperUsageItem.UnscannedList.Contains(audit))
                    {
                        //add to damaged list
                        m_paperUsageItem.DamagedList.Add(damagedItem);
                        //remove from unscanned
                        m_paperUsageItem.UnscannedList.Remove(audit);

                        //add item to UI
                        AddItemToSortedList(damagedItem);
                        IsModified = true;

                        //raise event
                        RaiseUnscannedChangedEvent(new PaperUsageUnscannedChangedEventArgs(m_paperUsageItem, audit, false));
                    }
                    else
                    {
                        if (audit < m_paperUsageItem.AuditStart || audit > m_paperUsageItem.AuditEnd)
                        {
                            //cannot add paper if out of range
                            POSMessageForm.Show(m_owner, m_pointOfSale, "Unable to add damage. The audit must be within start/end range");
                        }
                        else if (m_paperUsageItem.SkipList.Contains(audit))
                        {
                            //cannot add paper if paper is skipped
                            POSMessageForm.Show(m_owner, m_pointOfSale, "Unable to add damage. The audit number is a manufacturer skip");
                        }
                        else if (m_paperUsageItemControl.IsModified)
                        {
                            POSMessageForm.Show(m_owner, m_pointOfSale,
                                "Unable to add damage. The paper item has been modified and may need to be saved before adding damaged items");
                        }
                        else
                        {
                            //cannot add a paper that has been sold
                            //it first must be voided or exchanged before 
                            //it can be marked as damaged
                            POSMessageForm.Show(m_owner, m_pointOfSale,
                                "Unable to add damage. The paper may have been sold and must first be exchanged or voided");
                        }

                        AuditNumberTextBox.Text = string.Empty;
                        return;
                    }
                }
                else
                {
                    if (audit < m_paperUsageItem.AuditStart || audit > m_paperUsageItem.AuditEnd)
                    {
                        //cannot add paper if out of range
                        POSMessageForm.Show(m_owner, m_pointOfSale, "Unable to add damage. The audit must be within start/end range");
                        AuditNumberTextBox.Text = string.Empty;
                        return;
                    }

                    //add to damaged list
                    m_paperUsageItem.DamagedList.Add(damagedItem);

                    AddItemToSortedList(damagedItem);
                    IsModified = true;
                }
            }

            //clear text
            AuditNumberTextBox.Text = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the Remove Damaged button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RemoveDamagedClick(object sender, EventArgs e)
        {
            if(DamagedListBox.SelectedIndex < 0)
            {
                return;
            }
        
            var damagedItem = DamagedListBox.Items[DamagedListBox.SelectedIndex] as PaperUsageDamagedItem;

            if (damagedItem == null)
            {
                return;
            }
            
            //remove audit from listbox
            if (DamagedListBox.Items.Contains(damagedItem))
            {
                DamagedListBox.Items.Remove(DamagedListBox.SelectedItem);
                IsModified = true;
                RemoveButton.Enabled = false;
            }
            
            //remove from paper usage item
            if (m_paperUsageItem.DamagedList.Contains(damagedItem))
            {
                m_paperUsageItem.DamagedList.Remove(damagedItem);

                //need to add it to skipped list if its barcoded paper
                if (m_paperUsageItem.IsBarcodedPaper && !m_paperUsageItem.UnscannedList.Contains(damagedItem.AuditNumber))
                {
                    m_paperUsageItem.UnscannedList.Add(damagedItem.AuditNumber);
                    RaiseUnscannedChangedEvent(new PaperUsageUnscannedChangedEventArgs(m_paperUsageItem, damagedItem.AuditNumber, true));
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Close control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CloseClick(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the KeyPress event of the AuditNumberTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs"/> instance containing the event data.</param>
        private void AuditNumberTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            var textbox = sender as TextBox;

            if (textbox == null)
            {
                return;
            }

            CheckForBarcodeScanned(textbox, e.KeyChar);
        }

        /// <summary>
        /// Checks for barcode scanned.
        /// </summary>
        /// <param name="textbox">The textbox.</param>
        /// <param name="character">The character.</param>
        private void CheckForBarcodeScanned(TextBox textbox, char character)
        {
            if (character != (char)Keys.Enter)
            {
                return;
            }
            if (textbox == null)
            {
                return;
            }

            if (textbox.Text.Length >= 12 && textbox.Text.Contains(m_paperUsageItem.Serial))
            {
                //assume we got a barcode scan
                var auditString = textbox.Text.Substring(textbox.Text.Length - 5);
                var auditNumber = auditString.Base32To10();
                textbox.Text = auditNumber.ToString();
            }

            AddDamagedClick(textbox, EventArgs.Empty);

            textbox.Text = string.Empty;
        }
        
        /// <summary>
        /// Raises the unscanned changed event.
        /// </summary>
        /// <param name="args">The <see cref="PaperUsageUnscannedChangedEventArgs"/> instance containing the event data.</param>
        private void RaiseUnscannedChangedEvent(PaperUsageUnscannedChangedEventArgs args)
        {
            var handler = UnscannedChangedEvent;

            if (handler != null)
            {
                handler(this, args);
            }

        }

        #region KeyPad Events

        private void PressThisKey(object sender, EventArgs e)
        {
            SendKeyPress(((Button)sender).Text);
        }

        private void ClearButtonClick(object sender, EventArgs e)
        {
            AuditNumberTextBox.Text = string.Empty;
            AuditNumberTextBox.Focus();
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            SendKeyPress("\x08");
        }

        private void SendKeyPress(string number)
        {
            AuditNumberTextBox.Focus();

            SendKeys.SendWait(number);
        }
        #endregion

        private void DamagedListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var damagedItem = DamagedListBox.SelectedItem as PaperUsageDamagedItem;
            if (damagedItem != null)
            {
                var isInRange = m_paperUsageItem.AuditStart <= damagedItem.AuditNumber && m_paperUsageItem.AuditEnd >= damagedItem.AuditNumber;
                RemoveButton.Enabled = m_paperUsageItem.CanBeRemovedDamagedList.Contains(damagedItem.AuditNumber) && isInRange || !m_paperUsageItem.IsBarcodedPaper;
            }
        }

        private void AddItemToSortedList(PaperUsageDamagedItem damagedItem)
        {
            var addedFlag = false;
            for (int i = 0; i < DamagedListBox.Items.Count; i++)
            {
                var control = DamagedListBox.Items[i] as PaperUsageDamagedItem;

                if (control == null)
                {
                    continue;
                }

                if (damagedItem.AuditNumber < control.AuditNumber)
                {
                    DamagedListBox.Items.Insert(i, damagedItem);
                    addedFlag = true;
                    break;
                }
            }

            if (!addedFlag)
            {
                DamagedListBox.Items.Add(damagedItem);
            }

        }

        #endregion

    }
}
