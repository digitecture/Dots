using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace DotsProj

{
    public class dots_devInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "FloorPlanAutomation";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                return null;
            }
        }
        public override string Description
        {
            get
            {
                return "floor_plan_automation";
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
                return "Nirvik Saha";
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
