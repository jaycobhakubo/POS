using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTI.Modules.Shared;
using GTI.Modules.Shared.Business;

namespace GTI.Modules.POS.Business
{
    public class SessionInfo : IEquatable<SessionInfo>, IComparable<SessionInfo>
    {
        /// <summary>
        /// Session Info Constructor
        /// </summary>
        /// <param name="sessionNumber"></param>
        /// <param name="sessionPlayedId"></param>
        /// <param name="programName"></param>
        /// <param name="isMaxValidationEnabled"></param>
        /// <param name="isDeviceFeesEnabled"></param>
        /// <param name="isAutoDiscountsEnabled"></param>
        /// <param name="pointsMultiplier"></param>
        /// <param name="sessionMaxCardLimit"></param>
        /// <param name="isPreSale"></param>
        /// <param name="gameToGameCategoriesDictionary"></param>
        /// <param name="gameCategoryList"></param>
        /// <param name="gamingDate"></param>
        public SessionInfo(short sessionNumber, int sessionPlayedId, string programName, bool isMaxValidationEnabled, 
            bool isDeviceFeesEnabled, bool isAutoDiscountsEnabled, int pointsMultiplier, int sessionMaxCardLimit, bool isPreSale,
            Dictionary<int, List<int>> gameToGameCategoriesDictionary, List<GameCategory> gameCategoryList, DateTime? gamingDate)
        {
            //All properties are read only to protect data from being changed 
            SessionNumber = sessionNumber;
            SessionPlayedId = sessionPlayedId;
            ProgramName = programName;
            IsMaxValidationEnabled = isMaxValidationEnabled;
            IsAutoDiscountsEnabled = isAutoDiscountsEnabled;
            IsDeviceFeesEnabled = isDeviceFeesEnabled;
            PointsMultiplier = pointsMultiplier;
            SessionMaxCardLimit = sessionMaxCardLimit;
            IsPreSale = isPreSale;
            GameToGameCategoriesDictionary = gameToGameCategoriesDictionary ?? new Dictionary<int, List<int>>(); // don't let these be null, instead have an empty list
            GameCategoryList = gameCategoryList ?? new List<GameCategory>(); // don't let these be null, instead have an empty list
            GamingDate = gamingDate;
        }

        /// <summary>
        /// Gets or sets the session for this item (or 0 for no session).
        /// </summary>
        public short SessionNumber { get; private set; }

        /// <summary>
        /// Gets or sets The database session played id associated with this 
        /// item (or 0 for no session).
        /// </summary>
        public int SessionPlayedId { get; private set; }

        /// <summary>
        /// Gets or sets the name of the program for this item (or null 
        /// for no program).
        /// </summary>
        public string ProgramName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether maximum validation is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is maximum validation enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsMaxValidationEnabled { get; private set; }

        /// <summary>
        /// Get or set the default validation package for this session.
        /// </summary>
        public ValidationPackage ValidationPackage{ get; set; }

        /// <summary>
        /// Get or set the default validation package button for this session.
        /// </summary>
        public object DefaultValidationPackageButton{ get; set; }

        //US4719: (US4717) POS: Support scheduling device fees
        /// <summary>
        /// Gets or sets a value indicating whether device fees is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is device fees enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeviceFeesEnabled { get; private set; }

        //US4721: (US4324) POS: Support scheduling discounts
        /// <summary>
        /// Gets or sets a value indicating whether automatic discounts is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is automatic discounts enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsAutoDiscountsEnabled { get; private set; }

        //US4724: (US4722) POS: Support point multiplier
        /// <summary>
        /// Gets or sets the points multiplier.
        /// </summary>
        /// <value>
        /// The points multiplier.
        /// </value>
        public int PointsMultiplier { get; private set; }

        //US5287
        /// <summary>
        /// Gets or sets the maximum card limit.
        /// </summary>
        /// <value>
        /// The maximum card limit.
        /// </value>
        public int SessionMaxCardLimit { get; private set; }

        //US5546        
        /// <summary>
        /// Gets or sets a value indicating if the session is a pre sale.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is pre sale; otherwise, <c>false</c>.
        /// </value>
        public bool IsPreSale { get; private set; }

        //US5328
        /// <summary>
        /// Gets or sets the game categories maximum card limit dictionary.
        /// </summary>
        /// <value>
        /// The game categories maximum card limit dictionary.
        /// Key: session game played ID
        /// Value: game category ID
        /// </value>
        public Dictionary<int, List<int>> GameToGameCategoriesDictionary { get; private set; }

        //US5328
        /// <summary>
        /// Gets or sets the game category maximum card limits.
        /// </summary>
        /// <value>
        /// The game category maximum card limits.
        /// </value>
        public List<GameCategory> GameCategoryList { get; private set; }

        //US5546
        public DateTime? GamingDate { get; private set; }

        public override string ToString()
        {
            return SessionNumber.ToString();
        }

        public bool Equals(SessionInfo other)
        {
            return other != null &&
                   other.IsAutoDiscountsEnabled == IsAutoDiscountsEnabled &&
                   other.IsDeviceFeesEnabled == IsDeviceFeesEnabled &&
                   other.IsMaxValidationEnabled == IsMaxValidationEnabled &&
                   other.IsPreSale == IsPreSale &&
                   other.SessionMaxCardLimit == SessionMaxCardLimit &&
                   other.SessionNumber == SessionNumber &&
                   other.SessionPlayedId == SessionPlayedId &&
                   other.GamingDate == GamingDate &&
                   other.PointsMultiplier == PointsMultiplier &&
                   other.ProgramName == ProgramName;
        }

        public int CompareTo(SessionInfo other)
        {
            var result = 0;

            if (other.GamingDate.HasValue && GamingDate.HasValue)
            {
                result = DateTime.Compare(GamingDate.Value, other.GamingDate.Value);
            }
            if (GamingDate.HasValue && !other.GamingDate.HasValue)
            {
                return 1;
            }
            if (!GamingDate.HasValue && other.GamingDate.HasValue)
            {
                return -1;
            }

            if (result == 0)
            {
                return SessionNumber.CompareTo(other.SessionNumber);
            }

            return result;
        }
    }
}
