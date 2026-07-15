namespace SSO.Core.Domain.Identity.RolePermissions.Resources {
    using System;
    public sealed class EntityRolePermission {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityRolePermission() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.RolePermissions.Resources.EntityRolePermission", typeof(EntityRolePermission).Assembly);
                }
                return resourceMan;
            }
        }
        public static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }
        public static string RoleId {
            get { return ResourceManager.GetString("RoleId", resourceCulture); }
        }
        public static string PermissionId {
            get { return ResourceManager.GetString("PermissionId", resourceCulture); }
        }
    }
}
