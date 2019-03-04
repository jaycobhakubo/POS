#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

// Rally TA5748

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GTI.Modules.POS.Business;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Data
{
    // FIX: DE4037
    /// <summary>
    /// The possible status return codes from the Get Paper Start Numbers
    /// server message.
    /// </summary>
    internal enum GetPaperStartNumbersReturnCode
    {
        MissingPermLib = -71
    }

    /// <summary>
    /// A structure containing all the start numbers for a product.
    /// </summary>
    internal struct ProductStartNumbers : IEquatable<ProductStartNumbers>
    {
        #region Member Variables
        public int SessionPlayedId;
        public int ProductId;
        public List<StartNumber> StartNumbers;
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two ProductStartNumbers instances are equal.
        /// </summary>
        /// <param name="obj">The ProductStartNumbers to compare with the 
        /// current ProductStartNumbers.</param>
        /// <returns>true if the specified ProductStartNumbers is equal to the
        /// current ProductStartNumbers; otherwise, false.</returns>
        public override bool Equals(object obj)
        {           
            if(obj is ProductStartNumbers)
                return Equals((ProductStartNumbers)obj);
            else
                return false;
        }

        /// <summary>
        /// Serves as a hash function for a ProductStartNumbers. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current ProductStartNumbers.</returns>
        public override int GetHashCode()
        {
            return (SessionPlayedId.GetHashCode() ^ ProductId.GetHashCode());
        }

        /// <summary>
        /// Determines whether two ProductStartNumbers instances are equal.
        /// </summary>
        /// <param name="other">The ProductStartNumbers to compare with the 
        /// current ProductStartNumbers.</param>
        /// <returns>true if the specified ProductStartNumbers is equal to the
        /// current ProductStartNumbers; otherwise, false.</returns>
        public bool Equals(ProductStartNumbers other)
        {
            return (SessionPlayedId == other.SessionPlayedId &&
                    ProductId == other.ProductId);
        }
        #endregion
    }

    /// <summary>
    /// Represents a Get Paper Start Numbers message.
    /// </summary>
    internal class GetPaperStartNumbersMessage : ServerMessage
    {
        #region Member Variables
        protected Dictionary<ProductStartNumbers, short> m_productQtys = new Dictionary<ProductStartNumbers, short>();
        protected List<ProductStartNumbers> m_startNumbers = new List<ProductStartNumbers>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the GetPaperStartNumbersMessage 
        /// class.
        /// </summary>
        public GetPaperStartNumbersMessage()
        {
            m_id = 6054; // Get Paper Start Numbers
            m_strMessageName = "Get Paper Start Numbers";
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Adds a product to the list to retrieve start numbers for.
        /// </summary>
        /// <param name="sessionPlayedId">The session played id the daily
        /// package product is associated to.</param>
        /// <param name="productId">The daily package product id of the product
        /// to return start numbers for.</param>
        /// <param name="quantity">The count of start to be returned.</param>
        public void AddProduct(int sessionPlayedId, int productId, short quantity)
        {
            // Does this product already exist?
            ProductStartNumbers product = new ProductStartNumbers();
            product.SessionPlayedId = sessionPlayedId;
            product.ProductId = productId;

            if(!m_productQtys.ContainsKey(product))
                m_productQtys.Add(product, 0); // Add new.

            m_productQtys[product] += quantity;
        }

        /// <summary>
        /// Prepares the request to be sent to the server.
        /// </summary>
        protected override void PackRequest()
        {
            // Create the streams we will be writing to.
            MemoryStream requestStream = new MemoryStream();
            BinaryWriter requestWriter = new BinaryWriter(requestStream, Encoding.Unicode);

            // Product Count
            requestWriter.Write((ushort)m_productQtys.Count);

            // Product Session/Id/Quantity
            foreach(KeyValuePair<ProductStartNumbers, short> pair in m_productQtys)
            {
                // Session Played Id
                requestWriter.Write(pair.Key.SessionPlayedId);

                // Daily Package Product Id
                requestWriter.Write(pair.Key.ProductId);

                // Quantity
                requestWriter.Write(pair.Value);
            }

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
            // Reset the values.
            m_startNumbers.Clear();

            base.UnpackResponse();

            // Create the streams we will be reading from.
            MemoryStream responseStream = new MemoryStream(m_responsePayload);
            BinaryReader responseReader = new BinaryReader(responseStream, Encoding.Unicode);

            // Try to unpack the data.
            try
            {
                // Seek past return code.
                responseReader.BaseStream.Seek(sizeof(int), SeekOrigin.Begin);
                
                // Get the count of products
                ushort productCount = responseReader.ReadUInt16();

                for(ushort x = 0; x < productCount; x++)
                {
                    ProductStartNumbers nums = new ProductStartNumbers();
                    nums.StartNumbers = new List<StartNumber>();

                    // Session Played Id
                    nums.SessionPlayedId = responseReader.ReadInt32();

                    // Daily Package Product Id
                    nums.ProductId = responseReader.ReadInt32();

                    // Count of start numbers.
                    ushort numCount = responseReader.ReadUInt16();

                    for(ushort y = 0; y < numCount; y++)
                    {
                        StartNumber startNum = new StartNumber();

                        // Paper Start Number
                        startNum.Number = responseReader.ReadInt32();

                        nums.StartNumbers.Add(startNum);
                    }

                    m_startNumbers.Add(nums);
                }
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
        /// Gets a list of start numbers received from the server.
        /// </summary>
        public IEnumerable<ProductStartNumbers> StartNumbers
        {
            get
            {
                return m_startNumbers;
            }
        }
        #endregion
    }
}