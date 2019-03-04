#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply: © 2016 FortuNet, Inc.
#endregion
//Product Center > Coupons: Award coupons automatically

using System.Globalization;
using System.IO;
using System.Text;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    public class AwardAutoPlayerCouponMessage : ServerMessage
    {
        #region local fields

        private readonly int m_playerId;
        private readonly decimal m_amount;
        private readonly int m_sessionPlayedId;
        #endregion

        #region Contructors
        public AwardAutoPlayerCouponMessage(int playerId, decimal amount, int sessionPlayedId)
        {
            m_id = 18225;
            m_strMessageName = "Award Auto Player Coupon Message";
            m_playerId = playerId;
            m_amount = amount;
            m_sessionPlayedId = sessionPlayedId;
        }
        #endregion

        #region Member Methods

        /// <summary>
        /// packs the request to send to the server
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            //send player id
            requestWriter.Write(m_playerId);

            //convert to string
            var amountString = m_amount.ToString(CultureInfo.InvariantCulture);

            //send length
            requestWriter.Write((ushort)amountString.Length);

            //send string array
            requestWriter.Write(amountString.ToCharArray());

            //send session played id
            requestWriter.Write(m_sessionPlayedId);

            // Set the bytes to be sent.
            m_requestPayload = requestStream.ToArray();

            // Close the streams.
            requestWriter.Close();

        }
        #endregion
    }
}
