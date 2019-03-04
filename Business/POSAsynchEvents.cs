using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Business
{
    /// ==========================================================================
    /// 
    /// The purpose of this file is to contain all of the different asynchronous data handling structures
    /// 
    /// ==========================================================================


    /// US4809
    /// <summary>
    /// Provides data for the GetPlayerCompleted event.
    /// </summary>
    internal class GetPlayerEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the GetPlayerEventArgs class.
        /// </summary>
        /// <param name="player">The player found.</param>
        public GetPlayerEventArgs(Player player)
        {
            Player = player;
        }

        /// <summary>
        /// Initializes a new instance of the GetPlayerEventArgs class.
        /// </summary>
        /// <param name="player">The player found.</param>
        /// <param name="same">True if this player is the same as the current player.</param>
        public GetPlayerEventArgs(Player player, bool same)
        {
            Player = player;
            PlayerDidNotChange = same;
        }

        /// <summary>
        /// Initializes a new instance of the GetPlayerEventArgs class.
        /// </summary>
        /// <param name="ex">The exception encountered while looking up the player.</param>
        public GetPlayerEventArgs(Exception ex)
        {
            Error = ex;
        }

        /// <summary>
        /// Initializes a new instance of the GetPlayerEventArgs class.
        /// </summary>
        /// <param name="ex">The exception encountered while looking up the player.</param>
        /// <param name="same">True if this player is the same as the current player.</param>
        public GetPlayerEventArgs(Exception ex, bool same)
        {
            Error = ex;
            PlayerDidNotChange = same;
        }

        /// <summary>
        /// Initializes a new instance of the GetPlayerEventArgs class.
        /// </summary>
        /// <param name="player">The player found.</param>
        /// <param name="ex">The exception encountered while looking up the player.</param>
        public GetPlayerEventArgs(Player player, Exception ex)
        {
            Player = player;
            Error = ex;
        }

        /// <summary>
        /// Initializes a new instance of the GetPlayerEventArgs class.
        /// </summary>
        /// <param name="player">The player found.</param>
        /// <param name="ex">The exception encountered while looking up the player.</param>
        /// <param name="same">True if this player is the same as the current player.</param>
        public GetPlayerEventArgs(Player player, Exception ex, bool same)
        {
            Player = player;
            Error = ex;
            PlayerDidNotChange = same;
        }
        #endregion

        #region Member Variables
        /// <summary>
        /// Gets the player found or null an error occurred.
        /// </summary>
        public Player Player
        {
            get;
            protected set;
        }

        /// <summary>
        /// The error encountered while getting the player information
        /// </summary>
        public Exception Error
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets if this player is the same as the current player.
        /// </summary>
        public bool PlayerDidNotChange
        {
            get;
            set;
        }
        #endregion
    }

    /// US4809
    /// <summary>
    /// Provides data for the BusyStatusChanged event.
    /// </summary>
    internal class BusyChangedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the BusyChangedEventArgs class.
        /// </summary>
        /// <param name="player">The player found.</param>
        public BusyChangedEventArgs(bool isBusyValue)
        {
            IsBusy = isBusyValue;
        }
        #endregion

        #region Member Variables

        /// <summary>
        /// The error encountered while getting the player information
        /// </summary>
        public bool IsBusy
        {
            get;
            private set;
        }
        #endregion
    }
}
