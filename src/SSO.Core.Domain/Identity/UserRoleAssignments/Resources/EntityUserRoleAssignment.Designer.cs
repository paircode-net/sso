namespace SSO.Core.Domain.Identity.UserRoleAssignments.Resources {
    using System;
    public sealed class EntityUserRoleAssignment {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityUserRoleAssignment() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.UserRoleAssignments.Resources.EntityUserRoleAssignment", typeof(EntityUserRoleAssignment).Assembly);
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
        public static string RoleId {
            get { return ResourceManager.GetString("RoleId", resourceCulture); }
        }
        public static string OrganizationId {
            get { return ResourceManager.GetString("OrganizationId", resourceCulture); }
        }
        public static string ProductId {
            get { return ResourceManager.GetString("ProductId", resourceCulture); }
        }
    }
}
