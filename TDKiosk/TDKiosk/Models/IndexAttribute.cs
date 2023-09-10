using System;

namespace TDKiosk
{
    class IndexAttribute : Attribute
    {
        public int Index { get; private set; }

        public IndexAttribute(int index)
        {
            this.Index = index;
        }
    }


    class VideoSourceAttribute : Attribute
    {
        public string Source { get; private set; }

        public VideoSourceAttribute(string index)
        {
            this.Source = index;
        }
    }
}
