#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008-2009 GameTech
// International, Inc.
#endregion

// Rally US658

using System;
using System.IO;
using System.Text;
using System.Globalization;
using GTI.Modules.Shared;
using GTI.Modules.POS.Properties;

namespace GTI.Modules.POS.Data
{
    /// <summary>
    /// Represents the Player Card Swipe server message.
    /// </summary>
    internal class PlayerCardSwipeMessage : ServerMessage
    {
        #region Member Variables
        protected int m_playerId;
        protected string m_magCardNum;
        protected bool m_enterRaffle;
        protected string m_playerFirstName;
        protected string m_playerLastName;
        protected decimal m_currentPoints;
        protected decimal m_pointsEarned;
        protected int m_PIN;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the PlayerCardSwipeMessage class.
        /// </summary>
        /// <param name="playerId">The player's id (or 0 if the magnetic card
        /// is used).</param>
        /// <param name="magCardNumber">The player's magnetic card
        /// number (or null if player id is used).</param>
        /// <param name="enterRaffle">true to enter the player in the raffle;
        /// otherwise false.</param>
        /// <exception cref="System.ArgumentException">magCardNumber is
        /// too long.</exception>
        public PlayerCardSwipeMessage(int playerId, string magCardNumber, bool enterRaffle)
        {
            m_id = 8017; // Player Card Swipe
            m_strMessageName = "Player Card Swipe";
            m_playerId = playerId;
            MagneticCardNumber = magCardNumber;
            CardPIN = 0;
            m_enterRaffle = enterRaffle;
        }

        /// <summary>
        /// Initializes a new instance of the PlayerCardSwipeMessage class.
        /// </summary>
        /// <param name="playerId">The player's id (or 0 if the magnetic card
        /// is used).</param>
        /// <param name="magCardNumber">The player's magnetic card
        /// number (or null if player id is used).</param>
        /// <param name="enterRaffle">true to enter the player in the raffle;
        /// otherwise false.</param>
        /// <param name="PIN">Card PIN (0=no PIN & -1=use saved PIN).</param>
        /// <exception cref="System.ArgumentException">magCardNumber is
        /// too long.</exception>
        public PlayerCardSwipeMessage(int playerId, string magCardNumber, bool enterRaffle, int PIN)
        {
            m_id = 8017; // Player Card Swipe
            m_strMessageName = "Player Card Swipe";
            m_playerId = playerId;
            MagneticCardNumber = magCardNumber;
            CardPIN = PIN;
            m_enterRaffle = enterRaffle;
        }
        
        #endregion

        #region Member Methods
        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Player Id
            requestWriter.Write(m_playerId);

            // Magnetic Card Number
            if(!string.IsNullOrEmpty(m_magCardNum))
            {
                requestWriter.Write((ushort)m_magCardNum.Length);
                requestWriter.Write(m_magCardNum.ToCharArray());
            }
            else
                requestWriter.Write((ushort)0);

            // Enter In Raffle
            requestWriter.Write(m_enterRaffle);

            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();
        }

        /// <summary>
        /// Parses the response received from the server.
        /// </summary>
        protected override void UnpackResponse()
        {
            // Clear existing.
            m_playerId = 0;
            m_playerFirstName = null;
            m_playerLastName = null;
            m_currentPoints = 0M;
            m_pointsEarned = 0M;

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);

                // Player Id
                m_playerId = responseReader.ReadInt32();

                // Player First Name
                ushort stringLen = responseReader.ReadUInt16();
                m_playerFirstName = new string(responseReader.ReadChars(stringLen));

                // Player Last Name
                stringLen = responseReader.ReadUInt16();
                m_playerLastName = new string(responseReader.ReadChars(stringLen));

                // Skip Current Tier Id
                responseReader.ReadInt32();

                // Current Points
                stringLen = responseReader.ReadUInt16();
                string tempDec = new string(responseReader.ReadChars(stringLen));

                if(!string.IsNullOrEmpty(tempDec))
                    m_currentPoints = decimal.Parse(tempDec, CultureInfo.InvariantCulture);

                // Points Earned For Swipe
                stringLen = responseReader.ReadUInt16();
                tempDec = new string(responseReader.ReadChars(stringLen));

                if(!string.IsNullOrEmpty(tempDec))
                    m_pointsEarned = decimal.Parse(tempDec, CultureInfo.InvariantCulture);
            }
            catch(EndOfStreamException e)
            {
                throw new MessageWrongSizeException(m_strMessageName, e);
            }
            catch(Exception e)
            {
                throw new ServerException(m_strMessageName, e);
            }

            // Close the streams.
            responseReader.Close();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the player's id used to swipe (or 0 if the magnetic
        /// card is used). After the message is sent, it will be the player id
        /// received from the server (or 0 if the player wasn't found).
        /// </summary>
        public int PlayerId
        {
            get
            {
                return m_playerId;
            }
            set
            {
                m_playerId = value;

                if(m_playerId != 0)
                    m_magCardNum = null;
            }
        }

        /// <summary>
        /// Gets or sets the player's mag. card number used to swipe (or null
        /// if player id is used).
        /// </summary>
        /// <exception cref="System.ArgumentException">MagneticCardNumber is
        /// too long.</exception>
        public string MagneticCardNumber
        {
            get
            {
                return m_magCardNum;
            }
            set
            {
                if(value == null || (value != null && value.Length <= StringSizes.MaxMagneticCardLength))
                {
                    m_magCardNum = value;

                    if(m_magCardNum != null)
                        m_playerId = 0;
                }
                else
                    throw new ArgumentException("MagneticCardNumber" + Resources.WrongSize, "MagneticCardNumber");
            }
        }

        /// <summary>
        /// Sets the PIN for the player card (0=no PIN & -1=use saved PIN)
        /// </summary>
        public int CardPIN
        {
            set
            {
                m_PIN = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to enter the player into the raffle.
        /// </summary>
        public bool EnterRaffle
        {
            get
            {
                return m_enterRaffle;
            }
            set
            {
                m_enterRaffle = value;
            }
        }

        /// <summary>
        /// Gets the player's first name received from the server.
        /// </summary>
        public string PlayerFirstName
        {
            get
            {
                return m_playerFirstName;
            }
        }

        /// <summary>
        /// Gets the player's last name received from the server.
        /// </summary>
        public string PlayerLastName
        {
            get
            {
                return m_playerLastName;
            }
        }

        /// <summary>
        /// Gets the player's current point total received from the server.
        /// </summary>
        public decimal CurrentPoints
        {
            get
            {
                return m_currentPoints;
            }
        }

        /// <summary>
        /// Gets the amount of points earned for this swipe received from the
        /// server.
        /// </summary>
        public decimal PointsEarned
        {
            get
            {
                return m_pointsEarned;
            }
        }
        #endregion
    }
}