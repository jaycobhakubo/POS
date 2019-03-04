namespace GTI.Modules.POS.Business
{
    public class B3Session
    {        
        #region Constructors

        /// <summary>
        /// Constructs a Session object
        /// </summary>
        /// <param name="sessionNumber">The session number.</param>
        /// <param name="sessionActive">Flag to set if the session is active or not.</param>
        /// <param name="name"></param>
        /// <param name="startTime"></param>
        public B3Session(int sessionNumber, bool sessionActive, string name, string startTime, string endTime)
        {
            SessionNumber = sessionNumber;
            Active = sessionActive;
            OperatorName = name;
            SessionStartTime = startTime;
            SessionEndTime = endTime;
        }
        #endregion

        #region Member Properties

        /// <summary>
        /// Get or sets the session number.
        /// </summary>
        public int SessionNumber
        {
            get;
            private set;
        } 

        public string OperatorName 
        { 
            get; 
            private set; 
        }

        public string SessionStartTime
        {
            get;
            private set;
        }

        public string SessionEndTime
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets or set weather the session is active or not.
        /// </summary>
        public bool Active
        {
            get;
            private set;
        }

        public string DisplayName
        {
            get { return string.Format("{0} {1}", SessionNumber, OperatorName); }
        }

        #endregion
    }
}
