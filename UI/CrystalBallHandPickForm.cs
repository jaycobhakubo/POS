#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using GTI.Controls;
using GTI.Modules.Shared;
using GTI.Modules.POS.Data;
using GTI.Modules.POS.Business;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.UI
{
    /// <summary>
    /// The form that allows customers to specify their Crystal Ball Bingo 
    /// numbers.
    /// </summary>
    internal partial class CrystalBallHandPickForm : POSForm
    {
        #region Member Variables
        protected CrystalBallCard[] m_cards;
        protected int m_currentCard;
        private DateTime m_idleSince = DateTime.Now;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the CrystalBallHandPickForm class.
        /// </summary>
        /// <param name="parent">The PointOfSale to which this form 
        /// belongs.</param>
        /// <param name="displayMode">The display mode used to show this 
        /// form.</param>
        /// <param name="cards">The array of Crystal Ball cards that will hold 
        /// the numbers selected.</param>
        /// <param name="numbersRequired">The amount of numbers that have to be 
        /// chosen for each card.</param>
        /// <exception cref="System.ArgumentNullException">parent or 
        /// displayMode is a null reference.</exception>
        /// <exception cref="System.ArgumentException">cards is a null 
        /// reference or contains no cards.</exception>
        public CrystalBallHandPickForm(PointOfSale parent, CrystalBallCard[] cards, int numbersRequired)
            : base(parent)
        {
            if(cards == null || cards.Length < 1)
                throw new ArgumentException("cards");

            InitializeComponent();
            ApplyDisplayMode();

            m_cards = cards;
            m_bingoNumberBoard.MaxSelectableItems = numbersRequired;
            UpdateDisplay();
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Sets the settings of this form based on the current display mode.
        /// </summary>
        protected override void ApplyDisplayMode()
        {
            base.ApplyDisplayMode();

            // Setup the number board.
            m_bingoNumberBoard.ScreenSize = m_panelMain.Size;

            if(m_displayMode is CompactDisplayMode)
            {
                Size = m_displayMode.CompactFormSize;

                // Adjust controls for small screen.
                BackgroundImage = Resources.CBBHandPickBack800;

                m_bingoNumberBoard.ElectronicCardImageUp = Resources.SmallPickElecUp;
                m_bingoNumberBoard.ElectronicCardImageDown = Resources.SmallPickElecDown;
                m_bingoNumberBoard.PaperCardImageUp = Resources.SmallPickPaperUp;
                m_bingoNumberBoard.PaperCardImageDown = Resources.SmallPickPaperDown;

                // Rally US505
                m_currentCardLabel.Location = new Point(12, 14);
                m_numbersChosenLabel.Location = new Point(526, 14);

                m_prevCardButton.Location = new Point(27, 75);
                m_nextCardButton.Location = new Point(713, 75);

                m_bingoNumberBoard.Location = new Point(15, 164);
                m_bingoNumberBoard.ButtonSize = new Size(48, 48);
                m_bingoNumberBoard.ButtonFont = new Font("Tahoma", 14F, FontStyle.Bold);
                m_bingoNumberBoard.ButtonMargin = new Padding(1, 1, 2, 1);
                m_bingoNumberBoard.ButtonPadding = new Padding(0, 0, 0, 0);
                m_bingoNumberBoard.Size = new Size(776, 256);

                m_finishedButton.Location = new Point(12, 543);
                m_finishedButton.Size = new Size(135, 50);

                m_clearAllButton.Location = new Point(225, 543);
                m_clearAllButton.Size = new Size(135, 50);

                m_clearCurrentButton.Location = new Point(440, 543);
                m_clearCurrentButton.Size = new Size(135, 50);

                m_cancelButton.Location = new Point(653, 543);
                m_cancelButton.Size = new Size(135, 50);
            }
            else
            {
                if (m_displayMode.IsWidescreen)
                {
                    Size = m_displayMode.WideFormSize;
                    BackgroundImage = Resources.CBBHandPickBackWide;
                }
                else //normal
                {
                    Size = m_displayMode.NormalFormSize;
                }

                if (m_displayMode.IsFullScreen)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Location = new Point(m_parent.SellingForm.Location.X + m_displayMode.OffsetForFullScreenX, m_parent.SellingForm.Location.Y + m_displayMode.OffsetForFullScreenY);
                }
                
                m_panelMain.Location = new Point(m_displayMode.EdgeAdjustmentForNormalToWideX, m_displayMode.EdgeAdjustmentForNormalToWideY);
                m_bingoNumberBoard.ElectronicCardImageUp = Resources.PickElecUp;
                m_bingoNumberBoard.ElectronicCardImageDown = Resources.PickElecDown;
                m_bingoNumberBoard.PaperCardImageUp = Resources.PickPaperUp;
                m_bingoNumberBoard.PaperCardImageDown = Resources.PickPaperDown;

                if (m_parent.WeAreAPOSKiosk) //make it look good on a kiosk
                {
                    //swap the cancel and finished buttons
                    Point finished = m_finishedButton.Location;

                    m_finishedButton.Location = m_cancelButton.Location;
                    m_cancelButton.Location = finished;

                    //add a little color and beef-up the fonts
                    m_cancelButton.ImageNormal = Resources.RedButtonUp;
                    m_cancelButton.ImagePressed = Resources.RedButtonDown;
                    m_cancelButton.Font = new Font(m_cancelButton.Font.FontFamily, 16, FontStyle.Bold);
                    m_cancelButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                    
                    m_finishedButton.ImageNormal = Resources.GreenButtonUp;
                    m_finishedButton.ImagePressed = Resources.GreenButtonDown;
                    m_finishedButton.Font = m_cancelButton.Font;
                    m_finishedButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                    m_clearAllButton.ImageNormal = Resources.OrangeButtonUp;
                    m_clearAllButton.ImagePressed = Resources.OrangeButtonDown;
                    m_clearAllButton.Font = m_cancelButton.Font;
                    m_clearAllButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                    m_clearCurrentButton.ImageNormal = Resources.WhiteButtonUp;
                    m_clearCurrentButton.ImagePressed = Resources.WhiteButtonDown;
                    m_clearCurrentButton.Font = m_cancelButton.Font;
                    m_clearCurrentButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                    m_nextLabel.Visible = true;
                    m_nextCardButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                    m_prevLabel.Visible = true;
                    m_prevCardButton.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;

                    m_bingoNumberBoard.UseClickSound = m_parent.Settings.UseKeyClickSoundsOnKiosk;
                }
            }
        }

        /// <summary>
        /// Updates the form to show the current card.
        /// </summary>
        protected void UpdateDisplay()
        {
            NotIdle();

            if(m_currentCard >= 0 && m_currentCard < m_cards.Length)
            {
                // Update which button types are showing on the board.
                if(m_cards[m_currentCard].Media == CardMedia.Electronic)
                    m_bingoNumberBoard.ImageGameKind = GameKind.Electronic;
                else
                    m_bingoNumberBoard.ImageGameKind = GameKind.Paper;

                // Update the labels and flashboard.
                m_currentCardLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.CBBCurrentCard, m_currentCard + 1, m_cards.Length);
                
                int numsChosen = 0;
                SortedList list = new SortedList();

                if(m_cards[m_currentCard].Face != null)
                {
                    numsChosen = m_cards[m_currentCard].Face.Length;

                    for(int x = 0; x < m_cards[m_currentCard].Face.Length; x++)
                    {
                        list.Add((int)m_cards[m_currentCard].Face[x], (int)m_cards[m_currentCard].Face[x]);
                    }
                }

                m_numbersChosenLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.CBBNumbersChosen, numsChosen, m_bingoNumberBoard.MaxSelectableItems);
                m_bingoNumberBoard.SelectedItemsInt = list;
            }

            // Update the next and previous buttons.
            m_prevCardButton.Enabled = (m_currentCard > 0);
            m_nextCardButton.Enabled = (m_cards.Length != 0 && m_currentCard < m_cards.Length - 1);

            if (m_parent.WeAreAPOSKiosk)
            {
                m_prevLabel.Visible = m_prevCardButton.Enabled;
                m_nextLabel.Visible = m_nextCardButton.Enabled;
            }

            UpdateFinishedButton();
        }

        /// <summary>
        /// Handles the previous card button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void PrevCardClick(object sender, EventArgs e)
        {
            m_currentCard--;

            if(m_currentCard < 0)
                m_currentCard = 0;

            UpdateDisplay();
        }

        /// <summary>
        /// Handles the next card button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void NextCardClick(object sender, EventArgs e)
        {
            m_currentCard++;

            if(m_currentCard > m_cards.Length - 1)
                m_currentCard--;

            UpdateDisplay();
        }

        /// <summary>
        /// Handles the BingoFlashBoard's BallClick event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="ballNumber">The total amount of numbers currently 
        /// selected.</param>
        private void NumberClick(object sender, int totalBallNumber)
        {
            NotIdle();

            if(m_currentCard >= 0 && m_currentCard < m_cards.Length)
            {
                SortedList numList = m_bingoNumberBoard.SelectedItems;

                // Update the face of the current card.
                if(numList.Values.Count == 0)
                    m_cards[m_currentCard].Face = null;
                else
                {
                    m_cards[m_currentCard].Face = new byte[numList.Values.Count];

                    for(int x = 0; x < numList.Values.Count; x++)
                    {
                        m_cards[m_currentCard].Face[x] = Convert.ToByte(numList.GetByIndex(x), CultureInfo.InvariantCulture);
                    }
                }

                int numsChosen = 0;

                // Update the number count.
                if(m_cards[m_currentCard].Face != null)
                    numsChosen = m_cards[m_currentCard].Face.Length;

                m_numbersChosenLabel.Text = string.Format(CultureInfo.CurrentCulture, Resources.CBBNumbersChosen, numsChosen, m_bingoNumberBoard.MaxSelectableItems);

                UpdateFinishedButton();
            }
        }

        private void UpdateFinishedButton()
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                if (AllCardsFilledIn()) //we are finished
                {
                    m_finishedButton.Text = "Finished";

                    if (m_finishedButton.ImageNormal != Resources.GreenButtonUp)
                    {
                        m_finishedButton.ImageNormal = Resources.GreenButtonUp;
                        m_finishedButton.ImagePressed = Resources.GreenButtonDown;
                    }

                    m_finishedButton.Enabled = true;
                }
                else if(m_parent.Settings.CBBQuickPickEnabled)
                {
                    m_finishedButton.Text = "Finish with Quick Pick(s)";

                    if (m_finishedButton.ImageNormal != Resources.PurpleButtonUp)
                    {
                        m_finishedButton.ImageNormal = Resources.PurpleButtonUp;
                        m_finishedButton.ImagePressed = Resources.PurpleButtonDown;
                    }

                    m_finishedButton.Enabled = true;
                }
                else
                {
                    m_finishedButton.Text = "Finished";

                    if (m_finishedButton.ImageNormal != Resources.GreenButtonUp)
                    {
                        m_finishedButton.ImageNormal = Resources.GreenButtonUp;
                        m_finishedButton.ImagePressed = Resources.GreenButtonDown;
                    }

                    m_finishedButton.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Handles the finish button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void FinishedClick(object sender, EventArgs e)
        {
            NotIdle();

            if (string.Compare(m_finishedButton.Text, "Finished", true) == 0)
            {
                if (AllCardsFilledIn())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    NotIdle();
                    m_kioskIdleTimer.Stop();
                    m_parent.ShowMessage(this, m_displayMode, Resources.CBBHandPickNotDone);
                    m_kioskIdleTimer.Start();
                    NotIdle();
                }
            }
            else //finish with quick picks
            {
                //mark any incomplete cards as quick picks (server clears them and fills them in)
                for (int x = 0; x < m_cards.Length; x++)
                {
                    if (m_cards[x].Face == null ||
                       (m_cards[x].Face != null && m_cards[x].Face.Length != m_bingoNumberBoard.MaxSelectableItems))
                    {
                        m_cards[x].IsQuickPick = true;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool AllCardsFilledIn()
        {
            // Make sure that all the cards have all the numbers required.
            bool allFilledIn = true;

            for (int x = 0; x < m_cards.Length; x++)
            {
                if (m_cards[x].Face == null ||
                   (m_cards[x].Face != null && m_cards[x].Face.Length != m_bingoNumberBoard.MaxSelectableItems))
                {
                    allFilledIn = false;
                    break;
                }
            }

            return allFilledIn;
        }

        /// <summary>
        /// Handles the clear all button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ClearAllClick(object sender, EventArgs e)
        {
            NotIdle();

            // Go through each card and delete its face.
            for(int x = 0; x < m_cards.Length; x++)
            {
                m_cards[x].Face = null;
            }

            m_currentCard = 0;

            UpdateDisplay();
        }

        /// <summary>
        /// Handles the clear current button's click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the 
        /// event data.</param>
        private void ClearCurrentClick(object sender, EventArgs e)
        {
            NotIdle();

            if(m_currentCard >= 0 && m_currentCard < m_cards.Length)
            {
                m_cards[m_currentCard].Face = null;

                UpdateDisplay();
            }
        }

        private void m_kioskIdleTimer_Tick(object sender, EventArgs e)
        {
            if (m_parent.GuardianHasUsSuspended)
            {
                NotIdle();
                return;
            }

            TimeSpan idleFor = DateTime.Now - m_idleSince;

            if (idleFor > TimeSpan.FromMilliseconds(m_parent.SellingForm.KioskIdleLimitInSeconds / 3 * 2000))
            {
                if (!m_timeoutProgress.Visible)
                {
                    m_timeoutProgress.Visible = true;
                    m_cancelButton.Pulse = m_parent.Settings.KioskTimeoutPulseDefaultButton;
                }

                m_timeoutProgress.Increment(m_kioskIdleTimer.Interval);

                if (m_timeoutProgress.Value >= m_timeoutProgress.Maximum)
                {
                    DialogResult = DialogResult.Cancel;
                    m_parent.SellingForm.ForceKioskTimeout(1);
                    Close();
                }
            }
        }

        private void NotIdle()
        {
            m_idleSince = DateTime.Now;
            m_timeoutProgress.Hide();
            m_timeoutProgress.Value = 0;
            m_cancelButton.Pulse = false;
        }

        private void CrystalBallHandPickForm_Shown(object sender, EventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_parent.SellingForm.NotIdle(true); //stop the selling form triggers
                NotIdle();
                m_timeoutProgress.Maximum = (m_parent.SellingForm.KioskIdleLimitInSeconds / 3) * 1000;
                m_kioskIdleTimer.Enabled = true;
            }
        }

        private void CrystalBallHandPickForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_parent.WeAreAPOSKiosk)
            {
                m_kioskIdleTimer.Enabled = false;
                m_parent.SellingForm.NotIdle(); //start the selling form triggers
            }
        }

        private void UserActivityDetected(object sender, EventArgs e)
        {
            NotIdle();
        }

        #endregion
    }
}