namespace SSO.Core.Domain.Identity.Permissions.Resources {
    using System;
    public sealed class EntityPermission {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityPermission() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.Permissions.Resources.EntityPermission", typeof(EntityPermission).Assembly);
                }
                return resourceMan;
            }
        }
        public static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }
        public static string Code {
            get { return ResourceManager.GetString("Code", resourceCulture); }
        }
        public static string Name {
            get { return ResourceManager.GetString("Name", resourceCulture); }
        }
    }
}
