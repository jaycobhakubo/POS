#region Copyright
// This is an unpublished work protected under the copyright laws of the
// United States and other countries.  All rights reserved.  Should
// publication occur the following will apply:  © 2008 GameTech
// International, Inc.
#endregion

//US4721: (US4324) POS: Support scheduling discounts
//US4719: (US4717) POS: Support scheduling device fees
//US4724: (US4722) POS: Support point multiplier

using System;
using System.Globalization;
using System.Collections.Generic;
using GTI.Modules.POS.Properties;
using GTI.Modules.Shared;

namespace GTI.Modules.POS.Business
{
    /// <summary>
    /// Represents a menu of buttons on the POS.
    /// </summary>
    internal class POSMenu : IEquatable<POSMenu>
    {
        #region Member Variables
        protected int m_id;
        protected string m_name;
        protected byte m_buttonsPerPage;
        protected Dictionary<byte, MenuButton[]> m_pages = new Dictionary<byte, MenuButton[]>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the POSMenu class.
        /// </summary>
        /// <param name="id">The id of the menu.</param>
        /// <param name="name">The name of the menu.</param>
        /// <param name="buttonsPerPage">The maximum number of buttons on 
        /// a single page.</param>
        /// <exception cref="System.ArgumentException">buttonsPerPage is less 
        /// than 0.</exception>
        public POSMenu(int id, string name, byte buttonsPerPage)
        {
            if(buttonsPerPage < 0)
                throw new ArgumentException(Resources.ButtonsPerPage, "buttonsPerPage");

            m_id = id;
            m_buttonsPerPage = buttonsPerPage;
            m_name = name;
        }
        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two POSMenu instances are equal.
        /// </summary>
        /// <param name="obj">The POSMenu to compare with the 
        /// current POSMenu.</param>
        /// <returns>true if the specified POSMenu is equal to the current 
        /// POSMenu; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            POSMenu menu = obj as POSMenu;

            if(menu == null) 
                return false;
            else
                return Equals(menu);
        }

        /// <summary>
        /// Serves as a hash function for a POSMenu. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current POSMenu.</returns>
        public override int GetHashCode()
        {
            return m_id.GetHashCode();
        }

        /// <summary>
        /// Determines whether two POSMenu instances are equal.
        /// </summary>
        /// <param name="other">The POSMenu to compare with the 
        /// current POSMenu.</param>
        /// <returns>true if the specified POSMenu is equal to the current 
        /// POSMenu; otherwise, false.</returns>
        public bool Equals(POSMenu other)
        {
            return (other != null &&
                    m_id == other.m_id);
        }

        /// <summary>
        /// Adds a menu button to the specified page and position.
        /// </summary>
        /// <param name="page">The page to add the button to.</param>
        /// <param name="position">The position where the button will be
        /// placed.</param>
        /// <param name="button">The button to be added.</param>
        /// <exception cref="System.ArgumentException">page is less than 
        /// 0 or position is less than 0 or greater than buttonsPerPage.
        /// </exception>
        public void AddButton(byte page, byte position, MenuButton button)
        {
            if(page < 0)
                throw new ArgumentException(Resources.InvalidPage);

            if(position < 0 || position >= m_buttonsPerPage)
                throw new ArgumentException(Resources.InvalidPosition);

            // If the page doesn't exist, create it.
            if(!m_pages.ContainsKey(page))
                m_pages.Add(page, new MenuButton[m_buttonsPerPage]);

            m_pages[page][position] = button;
        }

        /// <summary>
        /// Finds the first button in the menu for the given package.
        /// </summary>
        /// <param name="packageID">ID of the package to find the button for.</param>
        /// <returns>PackageButton for the requested package or null if not found.</returns>
        public PackageButton GetPackageButton(int packageID, bool notLocked = false)
        {
            MenuButton[] buttons = GetPage(1, true);

            for (byte p = 1; buttons != null; p++)
            {
                foreach (MenuButton mb in buttons)
                {
                    PackageButton pb = mb as PackageButton;

                    if (pb != null && pb.Package.Id == packageID && (notLocked? !pb.IsLocked : true))
                        return pb;
                }

                buttons = GetPage(p, true);
            }

            return null;
        }
        
        /// <summary>
        /// Finds the first button in the menu for the given discount.
        /// </summary>
        /// <param name="packageID"></param>
        /// <returns></returns>
        public DiscountButton GetDiscountButton(int discountID)
        {
            MenuButton[] buttons = GetPage(1, true);

            for (byte p = 1; buttons != null; p++)
            {
                foreach (MenuButton mb in buttons)
                {
                    DiscountButton db = mb as DiscountButton;

                    if (db != null && db.Discount.Id == discountID)
                        return db;
                }

                buttons = GetPage(p, true);
            }

            return null;
        }
        
        /// <summary>
        /// Removes a button located at the specified page and position.
        /// If that page and position does not contain a button, nothing
        /// happens.
        /// </summary>
        /// <param name="page">The page the button is on.</param>
        /// <param name="position">The position where the button is 
        /// located.</param>
        /// <exception cref="System.ArgumentException">page is less than 
        /// 0 or position is less than 0 or greater than buttonsPerPage.
        /// </exception>
        public void RemoveButton(byte page, byte position)
        {
            if(page < 0)
                throw new ArgumentException(Resources.InvalidPage);

            if(position < 0 || position >= m_buttonsPerPage)
                throw new ArgumentException(Resources.InvalidPosition);

            // Check to see if the page exists.
            if(m_pages.ContainsKey(page))
                m_pages[page][position] = null;
        }

        /// <summary>
        /// Returns an array of menu buttons for the specified page.
        /// </summary>
        /// <param name="page">The desired page of buttons.</param>
        /// <returns>An array of menu buttons.</returns>
        /// <exception cref="System.ArgumentException">page is less than 
        /// 0.</exception>
        public MenuButton[] GetPage(byte page, bool ifExists = false)
        {
            if(page < 0)
                throw new ArgumentException(Resources.InvalidPage);

            // Return a zero length array if the page doesn't exist.
            if (!m_pages.ContainsKey(page))
            {
                if (ifExists)
                    return null;
                else
                    return new MenuButton[0];
            }
            else
            {
                return m_pages[page];
            }
        }

        /// <summary>
        /// Removes all the pages and buttons on a menu.
        /// </summary>
        public void ClearButtons()
        {
            m_pages = new Dictionary<byte, MenuButton[]>();
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the menu's id.
        /// </summary>
        public int Id
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the menu's name.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets the number pages the menu currently contains.
        /// </summary>
        public int PageCount
        {
            get
            {
                return m_pages.Count;
            }
        }

        public Tuple<int, List<MenuButton>> SimplePOSKioskButtons
        {
            get
            {
                int pages = 0;
                List<MenuButton> kioskButtons = new List<MenuButton>();
                MenuButton[] buttonsOnPage;

                for(int p = 1; p <= PageCount; p++)
                {
                    if (m_pages.TryGetValue((byte)p, out buttonsOnPage))
                    {
                        List<MenuButton> buttons = new List<MenuButton>(buttonsOnPage);
                        List<MenuButton> ourButtons = buttons.FindAll(b => b != null && !b.IsLocked && b is PackageButton);

                        if (ourButtons.Count > 0)
                        {
                            kioskButtons.AddRange(ourButtons);
                            pages++;
                        }
                    }
                }

                kioskButtons.Sort(
                                    delegate (MenuButton x, MenuButton y)
                                    {
                                        int c = x.Page.CompareTo(y.Page);

                                        if (c == 0) //same page, check location
                                            c = x.Position.CompareTo(y.Position);

                                        return c;
                                    }
                                 );

                return new Tuple<int, List<MenuButton>>(pages, kioskButtons);
            }
        }

        public List<MenuButton> HybridPOSKioskButtons
        {
            get
            {
                List<MenuButton> kioskButtons = new List<MenuButton>();
                MenuButton[] buttonsOnPage;

                if (m_pages.TryGetValue(1, out buttonsOnPage))
                {
                    List<MenuButton> buttons = new List<MenuButton>(buttonsOnPage);
                    List<MenuButton> ourButtons = buttons.FindAll(b => b != null && !b.IsLocked && b is PackageButton);

                    if (ourButtons.Count > 0)
                        kioskButtons.AddRange(ourButtons);
                }

                kioskButtons.Sort(
                                    delegate(MenuButton x, MenuButton y)
                                    {
                                        int c = x.Page.CompareTo(y.Page);

                                        if (c == 0) //same page, check location
                                            c = x.Position.CompareTo(y.Position);

                                        return c;
                                    }
                                 );

                return kioskButtons;
            }
        }

        public int BlankKioskPagesAtEnd
        {
            get
            {
                int page = PageCount;
                MenuButton[] buttonsOnPage;

                for (; page > 0; page--)
                {
                    if (m_pages.TryGetValue((byte)page, out buttonsOnPage))
                    {
                        List<MenuButton> buttons = new List<MenuButton>(buttonsOnPage);

                        if (buttons.Exists(b => b != null && !b.IsLocked))
                            break;
                    }
                }

                return PageCount - page;
            }
        }
        #endregion
    }


    /// <summary>
    /// Represents an association between a session/program and a POSMenu.
    /// </summary>
    internal class POSMenuListItem : IEquatable<POSMenuListItem>, IComparable<POSMenuListItem>, IComparable
    {
        #region Member Variables
        protected POSMenu m_menu;
        #endregion

        #region Constructors

        #endregion

        #region Member Methods
        /// <summary>
        /// Determines whether two POSMenuListItem instances are equal.
        /// </summary>
        /// <param name="obj">The POSMenuListItem to compare with the 
        /// current POSMenuListItem.</param>
        /// <returns>true if the specified POSMenuListItem is equal to the 
        /// current POSMenuListItem; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            POSMenuListItem item = obj as POSMenuListItem;

            if(item == null) 
                return false;
            else
                return Equals(item);
        }

        /// <summary>
        /// Serves as a hash function for a POSMenuListItem. 
        /// GetHashCode is suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </summary>
        /// <returns>A hash code for the current POSMenuListItem.</returns>
        public override int GetHashCode()
        {
            // Rally US738
            return (m_menu.GetHashCode() ^ Session.GetHashCode() ^ Session.SessionPlayedId.GetHashCode());
        }

        /// <summary>
        /// Returns a string that represents the current POSMenuListItem.
        /// </summary>
        /// <returns>A string that represents the current 
        /// POSMenuListItem.</returns>
        public override string ToString()
        {
            string returnVal = string.Empty;

            if (Session.IsPreSale)
            {
                if (Session.GamingDate != null)
                {
                    returnVal += string.Format("{0}: {1} - {2}", Session.GamingDate.Value.ToString("MM/dd/yy"), Session.SessionNumber, m_menu.Name);
                }
            }
            else if (Session != null)
            {
                if(Session.SessionNumber != 0)
                    returnVal += Session.SessionNumber.ToString(CultureInfo.CurrentCulture) + " - ";

                returnVal += m_menu.Name;
            }

            return returnVal;
        }

        /// <summary>
        /// Determines whether two POSMenuListItem instances are equal.
        /// </summary>
        /// <param name="other">The POSMenuListItem to compare with the 
        /// current POSMenuListItem.</param>
        /// <returns>true if the specified POSMenuListItem is equal to the 
        /// current POSMenuListItem; otherwise, false.</returns>
        public bool Equals(POSMenuListItem other)
        {
            return (other != null &&
                    m_menu.Equals(other.m_menu) &&
                    Session == other.Session);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="obj">The object to compare with this object.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order 
        /// of the objects being compared. The return value has the following 
        /// meanings: Less than zero - This object is less than the other 
        /// parameter. Zero - This object is equal to other. Greater than 
        /// zero - This object is greater than other.</returns>
        /// <exception cref="System.ArgumentException">obj is not a 
        /// POSMenuListItem.</exception>
        public int CompareTo(object obj)
        {
            POSMenuListItem item = obj as POSMenuListItem;

            if(item == null)
                throw new ArgumentException(Resources.NotAClass + "POSMenuListItem");
            else
                return CompareTo(item);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">The object to compare with this object.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order 
        /// of the objects being compared. The return value has the following 
        /// meanings: Less than zero - This object is less than the other 
        /// parameter. Zero - This object is equal to other. Greater than 
        /// zero - This object is greater than other.</returns>
        public int CompareTo(POSMenuListItem other)
        {
            if(other == null)
                return 1;

            if (Session != null && other.Session != null)
            {
                return Session.CompareTo(other.Session);
            }
            return -1;
        }
        #endregion

        #region Member Properties
        /// <summary>
        /// Gets or sets the POSMenu for this item.
        /// </summary>
        public POSMenu Menu
        {
            get
            {
                return m_menu;
            }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("Menu");

                m_menu = value;
            }
        }

        public SessionInfo Session { get; set; }

    }

    #endregion
}
