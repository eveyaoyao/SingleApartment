//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace sln_SingleApartment.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tMember
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tMember()
        {
            this.Activity = new HashSet<Activity>();
            this.FavoriteList = new HashSet<FavoriteList>();
            this.Information = new HashSet<Information>();
            this.Lease = new HashSet<Lease>();
            this.MemberInformationCategory = new HashSet<MemberInformationCategory>();
            this.Message = new HashSet<Message>();
            this.Order = new HashSet<Order>();
            this.Participant = new HashSet<Participant>();
            this.RoomFavorite = new HashSet<RoomFavorite>();
            this.RoomInformation = new HashSet<RoomInformation>();
        }
    
        public int fMemberId { get; set; }
        public string fMemberName { get; set; }
        public string fAccount { get; set; }
        public string fPassword { get; set; }
        public string fEmail { get; set; }
        public string fPhone { get; set; }
        public Nullable<int> fAge { get; set; }
        public string fSex { get; set; }
        public Nullable<System.DateTime> fBirthDate { get; set; }
        public string fSalary { get; set; }
        public string fCareer { get; set; }
        public string fImage { get; set; }
        public Nullable<int> fRoomId { get; set; }
        public Nullable<bool> fLeave { get; set; }
        public string fRole { get; set; }
        public string fActivityMessage { get; set; }
        public string fActivityEmail { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Activity> Activity { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FavoriteList> FavoriteList { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Information> Information { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Lease> Lease { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MemberInformationCategory> MemberInformationCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Message> Message { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Order { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Participant> Participant { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RoomFavorite> RoomFavorite { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RoomInformation> RoomInformation { get; set; }
    }
}
