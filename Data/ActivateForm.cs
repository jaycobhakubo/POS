using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GTI.Modules.POS.Data
{
    class ActivateForm
    {

        public static bool NOW(string formName)
        {
            bool IsFormExists = false;
            try
            {
                formName = formName.Trim().ToUpper();
                FormCollection fc = Application.OpenForms;
                foreach (Form frm in fc)
                {

                    if (frm.Name.ToUpper() == formName)
                    {

                        frm.Visible = true;
                        frm.Activate();
                        frm.BringToFront();
                        frm.Show();
                        //if (formName == "loginFullWin")
                        //{

                        //}


                      //  frm.Location = new Point(WindowsDefaultLocation.PointA, WindowsDefaultLocation.PointB);
                        IsFormExists = true;
                        break;

                    }
                }
            }
            catch (Exception e) { MessageBox.Show(e.Message); }
            return IsFormExists;
        }

    }
}
