using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace DotsProj
{
    public class GenMassFromCrvsInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "GenMassFromCrvs";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                return Properties.Resources.genCrvs;
            }
        }
        public override string Description
        {
            get
            {
                return "Generate building masses from base curves";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("59d64f5a-adb2-4fb9-aa23-c8bf6a9b3f38");
            }
        }

        public override string AuthorName
        {
            get
            {
                return "nirvik_saha";
            }
        }
        public override string AuthorContact
        {
            get
            {
                return "nirviksaha@gatech.edu";
            }
        }
    }
}
