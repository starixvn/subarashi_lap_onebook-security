using System.Collections.Generic;

using Newtonsoft.Json;

namespace LAP.ONEBook.Security
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1">Permissions object</typeparam>
    /// <typeparam name="T2">Currency Object</typeparam>
    public class DefaultAuthInfo<T1, T2>
    {
        [JsonProperty("<VendorID>k__BackingField")]
        private string LegacyVendorID { set { VendorID = value; } }
        public string VendorID { get; set; }

        [JsonProperty("<SiteName>k__BackingField")]
        private string LegacySiteName { set { SiteName = value; } }
        public string SiteName { get; set; }

        [JsonProperty("<UserSEQ>k__BackingField")]
        private int LegacyUserSEQ { set { UserSEQ = value; } }
        public int UserSEQ { get; set; }

        [JsonProperty("<UserID>k__BackingField")]
        private string LegacyUserID { set { UserID = value; } }
        public string UserID { get; set; }

        [JsonProperty("<UserName>k__BackingField")]
        private string LegacyUserName { set { UserName = value; } }
        public string UserName { get; set; }

        [JsonProperty("<Language>k__BackingField")]
        private string LegacyLanguage { set { Language = value; } }
        public string Language { get; set; }

        [JsonProperty("<ActionLink>k__BackingField")]
        private string LegacyActionLink { set { ActionLink = value; } }
        public string ActionLink { get; set; }

        [JsonProperty("<IsAdminstrator>k__BackingField")]
        private bool LegacyIsAdminstrator { set { IsAdminstrator = value; } }
        public bool IsAdminstrator { get; set; }

        [JsonProperty("<Permissions>k__BackingField")]
        private IEnumerable<T1> LegacyPermissions { set { Permissions = value; } }
        public IEnumerable<T1> Permissions { get; set; }

        [JsonProperty("<Currency>k__BackingField")]
        private IEnumerable<T2> LegacyCurrency { set { Currency = value; } }
        public IEnumerable<T2> Currency { get; set; }

        [JsonProperty("<WhiteLabelID>k__BackingField")]
        private int? LegacyWhiteLabelID { set { WhiteLabelID = value; } }
        public int? WhiteLabelID { get; set; }

        [JsonProperty("<IsMasterLicensee>k__BackingField")]
        private bool LegacyIsMasterLicensee { set { IsMasterLicensee = value; } }
        public bool IsMasterLicensee { get; set; }
    }
}
