using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace dots_dev
{
    public class dots_devInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "dotsdev";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("00e0df20-7fd0-4fb8-97eb-1b02abbf35dd");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
