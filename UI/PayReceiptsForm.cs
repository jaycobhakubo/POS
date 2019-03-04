// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2007 GameTech
// International, Inc.

using System;
using System.Windows.Forms;
using System.Collections;
using System.Globalization;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Properties;
using GTI.Controls;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.UI
{
    // TODO Revisit PayReceiptsForm.

    /// <summary>
    /// The form that allows the viewing and voiding of receipts.
    /// </summary>
    internal partial class PayReceiptsForm : POSForm
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ViewReceiptsForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        public PayReceiptsForm(PointOfSale parent, DisplayMode displayMode)
            : base(parent, displayMode)
        {
            InitializeComponent();
            ApplyDisplayMode();
        }
        #endregion

        #region Member variables

        ArrayList winnerList = new ArrayList();

        #endregion

        #region Member Methods
        /// <summary>
        /// Sets the settings of this form based on the current display mode.
        /// </summary>
        protected override void ApplyDisplayMode()
        {
            base.ApplyDisplayMode();

            // This is a dialog, so override the default size.
            Size = m_displayMode.LargeDialogSize;

            if(m_displayMode is CompactDisplayMode)
                BackgroundImage = Resources.PayReceiptsBack800;
        }

        /// <summary>
        /// Handles the keypad's enter button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void KeyPadEnterClick(object sender, EventArgs e)
        {
            /*
            if(m_keypad.Value != 0.0M)
            {
                // Remove the previous results.
                m_superPickList.Items.Clear();

                // Spawn a new thread to find receipts and wait until done.
                WaitForm waitForm = new WaitForm(m_displayMode);
                waitForm.WaitImage = Resources.Waiting;

                // m_parent.FindSuperPicks(waitForm, (uint)m_keypad.Value);
                waitForm.ShowDialog(); //Block until we are done

                if (m_parent.LastAsyncException != null)
                {
                    if(m_parent.LastAsyncException is ServerCommException)
                        m_parent.ServerCommFailed();
                    else
                    {
                        MessageForm msg = new MessageForm(m_displayMode, m_parent.LastAsyncException.Message, MessageFormTypes.OK, 0, false);
                        msg.ShowDialog();
                    }
                }
                else
                {
                    // Add the super pick(s) to the result list.
                    // SuperPickListItem[] superPicks = m_parent.LastFindSuperPicksResults;
                    /*
                    if (superPicks != null && superPicks.Length > 0)
                    {
                        m_superPickList.Items.AddRange(superPicks);
                        m_superPickList.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageForm msg = new MessageForm(m_displayMode, Resources.NoSuperPicksFound, MessageFormTypes.OK, 0, false);
                        msg.ShowDialog();
                    }
                }

                // Reset.
                m_keypad.Clear();
            }
            else
            {
                //MessageForm msg = new MessageForm(m_displayMode, Resources.ViewReceiptsNoValue, MessageFormTypes.OK, 0, false);
                //msg.ShowDialog();
            }
            */
        }

        /// <summary>
        /// Handles the receipt list up button click.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ReceiptListUpClick(object sender, EventArgs e)
        {
            /*
            if(m_superPickList.Items.Count > 0)
            {
                if(m_superPickList.SelectedIndices.Count == 0)
                    m_superPickList.SelectedIndex = 0;
                else
                {
                    int newIndex = m_superPickList.SelectedIndex - 1;

                    if (newIndex >= 0)
                        m_superPickList.SelectedIndex = newIndex;
                }
            }
            */
        }

        /// <summary>
        /// Handles the receipt list down button click.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ReceiptListDownClick(object sender, EventArgs e)
        {
            /*
            if(m_superPickList.Items.Count > 0)
            {
                if(m_superPickList.SelectedIndices.Count == 0)
                    m_superPickList.SelectedIndex = 0;
                else
                {
                    int newIndex = m_superPickList.SelectedIndex + 1;

                    if(newIndex < m_superPickList.Items.Count)
                        m_superPickList.SelectedIndex = newIndex;
                }
            }
            */
        }

        /// <summary>
        /// Handles the winners list up button click.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>        
        private void WinnersListUpButtonClick(object sender, EventArgs e)
        {
            /*
            if (m_superPickWinnersList.Items.Count > 0)
            {
                if (m_superPickWinnersList.SelectedIndices.Count == 0)
                    m_superPickWinnersList.SelectedIndex = 0;
                else
                {
                    int newIndex = m_superPickWinnersList.SelectedIndex - 1;

                    if (newIndex >= 0)
                        m_superPickWinnersList.SelectedIndex = newIndex;
                }
            }
            */
        }

        /// <summary>
        /// Handles the winners list down button click.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>        
        private void WinnersListDownButtonClick(object sender, EventArgs e)
        {
            /*
            if (m_superPickWinnersList.Items.Count > 0)
            {
                if (m_superPickWinnersList.SelectedIndices.Count == 0)
                    m_superPickWinnersList.SelectedIndex = 0;
                else
                {
                    int newIndex = m_superPickWinnersList.SelectedIndex + 1;

                    if (newIndex < m_superPickWinnersList.Items.Count)
                        m_superPickWinnersList.SelectedIndex = newIndex;
                }
            }
            */
        }


        /// <summary>
        /// Handles the scan barcode button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ScanBarcodeClick(object sender, EventArgs e)
        {
            /*
            ScanBarcodeForm scanBarcodeForm = new ScanBarcodeForm(m_parent, m_displayMode);

            if (scanBarcodeForm.ShowDialog() == DialogResult.OK)
            {
                m_keypad.Value = Convert.ToDecimal(scanBarcodeForm.BarcodeNumber, CultureInfo.CurrentCulture);
            }
            */
        }

        /// <summary>
        /// Handles the view player's info button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ViewPlayerInfoClick(object sender, EventArgs e)
        {
            /*
            if (m_parent.LastSupetPickPlayerNumber != null && m_parent.LastSupetPickPlayerNumber != string.Empty)
            {
                ViewPlayerInfoForm viewPlayerInfoForm = new ViewPlayerInfoForm(m_parent, m_displayMode);

                viewPlayerInfoForm.FirstName = m_parent.LastSuperPickPlayerFName;
                viewPlayerInfoForm.LastName = m_parent.LastSuperPickPlayerLName;
                viewPlayerInfoForm.Comments = Resources.VerifyPlayerId;
                if (m_parent.LastSuperPickPlayerPic != null)
                {
                    viewPlayerInfoForm.PlayerImage = m_parent.LastSuperPickPlayerPic;
                }

                viewPlayerInfoForm.ShowDialog();
            }
            else
            {
                MessageForm msg = new MessageForm(m_displayMode, Resources.NoSuperPickPlayerInfo, MessageFormTypes.OK, 0, false);
                msg.ShowDialog();
            }
            */
        }

        /// <summary>
        /// Handles the add winner numbers button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void AddWinnerNumbersClick(object sender, EventArgs e)
        {
            /*
            if (m_superPickList.Items.Count > 0)
            {
                if (m_superPickList.SelectedIndices.Count == 0)
                {
                    MessageForm msg = new MessageForm(m_displayMode, Resources.SelectSuperPickItem, MessageFormTypes.OK, 0, false);
                    msg.ShowDialog();
                }
                else
                {
                    // string superPickWinner = m_parent.LastSupetPickTransactionNumber.ToString().PadRight(16, ' ') + m_superPickList.SelectedItem;
                    /*
                    // Check for duplicates...
                    int index = m_superPickWinnersList.Items.IndexOf(superPickWinner);
                    if (index == -1)
                    {
                        m_superPickWinnersList.Items.Add(superPickWinner);

                        // Add to the winners array.
                        int i = m_superPickList.SelectedIndex;

                        SuperPickListItem[] superPicks = m_parent.LastFindSuperPicksResults;

                        // Product Type
                        string productType = superPicks[i].Type.ToString();
                        int iProductType = 0;
                        if (productType == "Single")
                            iProductType = 7;
                        else if (productType == "Double")
                            iProductType = 8;

                        // Hotball Sale Id
                        string hotballSaleId = superPicks[i].HotBallSaleId.ToString();

                        string winnerListItem = m_parent.LastSupetPickPlayerNumber + ',' + iProductType.ToString() + "," + hotballSaleId;

                        winnerList.Add(winnerListItem);
                    }
                }
            }
            else
            {
                MessageForm msg = new MessageForm(m_displayMode, Resources.NoSuperPickList, MessageFormTypes.OK, 0, false);
                msg.ShowDialog();
            }
            */
        }

        /// <summary>
        /// Handles the remove winner numbers button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void RemoveWinnerNumbersClick(object sender, EventArgs e)
        {
            /*
            if (m_superPickWinnersList.Items.Count > 0)
            {
                if (m_superPickWinnersList.SelectedIndices.Count == 0)
                {
                    MessageForm msg = new MessageForm(m_displayMode, Resources.NoSuperPickWinnerSelected, MessageFormTypes.OK, 0, false);
                    msg.ShowDialog();
                }
                else
                {
                    // Remove from the winners array.
                    int i = m_superPickWinnersList.SelectedIndex;
                    winnerList.RemoveAt(i);

                    m_superPickWinnersList.Items.Remove(m_superPickWinnersList.SelectedItem);
                }
            }
            */
        }

        /// <summary>
        /// Handles the exit button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void PayClick(object sender, EventArgs e)
        {
            /*
            // Spawn a new thread to find receipts and wait until done.
            WaitForm waitForm = new WaitForm(m_displayMode);
            waitForm.WaitImage = Resources.Waiting;

            // Set the super pick winner list
            // m_parent.LastSuperPickWinnerList = winnerList;

            // Pay the super pick winners
            // m_parent.PaySuperPicks(waitForm);
            waitForm.ShowDialog(); //Block until we are done

            if(m_parent.LastAsyncException != null)
            {
                if(m_parent.LastAsyncException is ServerCommException)
                    m_parent.ServerCommFailed();
                else
                {
                    MessageForm msg = new MessageForm(m_displayMode, m_parent.LastAsyncException.Message, MessageFormTypes.OK, 0, false);
                    msg.ShowDialog();
                }
            }
            /*else
            {
                // Add the pay super pick(s) to the result list.
                // PaySuperPickListItem[] paySuperPicks = m_parent.LastPaySuperPicksResults;
                
                if(paySuperPicks != null && paySuperPicks.Length > 0)
                {
                    // Print out the recipts
                    m_parent.PrintPayoutReceipt(false, true);
                }
                else
                {
                    MessageForm msg = new MessageForm(m_displayMode, Resources.NoSuperPicksFound, MessageFormTypes.OK, 0, false);
                    msg.ShowDialog();
                }
            }

            m_parent.LastPaySuperPicksResults = null;
            m_parent.LastFindSuperPicksResults = null;
            m_parent.LastSuperPickWinnerList = null;
                
            Close();
            */
        }

        /// <summary>
        /// Handles the exit button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ExitClick(object sender, EventArgs e)
        {
            /*
            // Clear player's info
            
            m_parent.LastSupetPickPlayerNumber = null;
            m_parent.LastSuperPickPlayerFName = null;
            m_parent.LastSuperPickPlayerLName = null;
            m_parent.LastSuperPickPlayerPic = null;
            */
            Close();
        }
        #endregion
    }
}