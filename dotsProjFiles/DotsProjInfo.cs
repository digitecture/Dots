using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace DotsProj
{
    public class DotsProjInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "DotsProj";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                return Properties.Resources.dots;
            }
        }
        public override string Description
        {
            get
            {
                return "Design Optimization Tool Set";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("2f25bc08-f462-456c-be30-932b256fe305");
            }
        }

        public override string AuthorName
        {
            get
            {
                return "nirvik saha";
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
