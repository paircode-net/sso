namespace SSO.Core.Domain.Identity.Memberships.Resources {
    using System;
    public sealed class EntityMembership {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityMembership() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.Memberships.Resources.EntityMembership", typeof(EntityMembership).Assembly);
                }
                return resourceMan;
            }
        }
        public static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }
        public static string UserId {
            get { return ResourceManager.GetString("UserId", resourceCulture); }
        }
        public static string OrganizationId {
            get { return ResourceManager.GetString("OrganizationId", resourceCulture); }
        }
    }
}
