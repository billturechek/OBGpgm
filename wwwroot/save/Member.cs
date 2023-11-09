using System;
using System.Collections.Generic;

namespace OBGpgm.Models
{
    public partial class Member
    {
        public Member()
        {
            Photos = new HashSet<Photo>();
            Players = new HashSet<Player>();
            Portraits = new HashSet<Portrait>();
        }

        public int MemberId { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Zip { get; set; }
        public string? Telephone { get; set; }
        public string? Cellphone { get; set; }
        public string? Email { get; set; }
        public string? VillageId { get; set; }
        public int? Office { get; set; }
        public int? CurrentPlayerId { get; set; }
        public string? Evaluation { get; set; }
        public string? ShirtSize { get; set; }
        public DateTime? Bday { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime? CurrentSignUpDate { get; set; }
        public string? UserId { get; set; }
        public string? PassWord { get; set; }
        public bool HasPaidAnnualDues { get; set; }
        public bool HasPaidSessionPrizeFund { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public bool WillPlayNextSession { get; set; }
        public bool IsJabba { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsHonored { get; set; }
        public bool IsDeceased { get; set; }
        public bool GetsPrize { get; set; }
        public bool WillCaptainNextSession { get; set; }
        public bool WillCaptainIfNeeded { get; set; }
        public string? TeamNameIfCaptain { get; set; }
        public string? Village { get; set; }
        public string? Hometown { get; set; }
        public string? MovedFrom { get; set; }
        public string? Wife { get; set; }
        public string? YearMoved { get; set; }
        public string? Children { get; set; }
        public string? Grand { get; set; }
        public string? Great { get; set; }
        public string? Job { get; set; }
        public string? Interests { get; set; }
        public string? Military { get; set; }
        public string? YearsMilitary { get; set; }
        public bool WantsNoPicture { get; set; }
        public bool Snowbird { get; set; }
        public int? PortraitId { get; set; }

        public virtual Portrait? Portrait { get; set; }
        public virtual ICollection<Photo> Photos { get; set; }
        public virtual ICollection<Player> Players { get; set; }
        public virtual ICollection<Portrait> Portraits { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public string ReverseName { get { return $"{LastName}, {FirstName}"; } }
    }
}
