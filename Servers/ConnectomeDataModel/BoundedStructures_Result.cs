//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConnectomeDataModel
{
    using System;
    
    public partial class BoundedStructures_Result
    {
        public long ID { get; set; }
        public long TypeID { get; set; }
        public string Notes { get; set; }
        public bool Verified { get; set; }
        public string Tags { get; set; }
        public double Confidence { get; set; }
        public byte[] Version { get; set; }
        public Nullable<long> ParentID { get; set; }
        public System.DateTime Created { get; set; }
        public string Label { get; set; }
        public string Username { get; set; }
        public System.DateTime LastModified { get; set; }
    }
}
