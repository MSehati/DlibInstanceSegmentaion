using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DlibInstanceSegmentaion.Model
{
    public class SegmentInfo
    {
        public string SegmentedImagePath { get; set; }
        public string SegmentedObjectTitle { get; set; }
        public string error { get; set; }
    }
}
