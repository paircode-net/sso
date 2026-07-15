namespace SSO.Core.Domain.Identity.Organizations.Resources {
    using System;
    public sealed class EntityOrganization {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityOrganization() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.Organizations.Resources.EntityOrganization", typeof(EntityOrganization).Assembly);
                }
                return resourceMan;
            }
        }
        public static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }
        public static string Name {
            get { return ResourceManager.GetString("Name", resourceCulture); }
        }
        public static string Code {
            get { return ResourceManager.GetString("Code", resourceCulture); }
        }
    }
}
