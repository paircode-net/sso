namespace SSO.Core.Domain.Identity.MenuItems.Resources {
    using System;
    public sealed class EntityMenuItem {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityMenuItem() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.MenuItems.Resources.EntityMenuItem", typeof(EntityMenuItem).Assembly);
                }
                return resourceMan;
            }
        }
        public static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }
        public static string ProductId {
            get { return ResourceManager.GetString("ProductId", resourceCulture); }
        }
        public static string Code {
            get { return ResourceManager.GetString("Code", resourceCulture); }
        }
        public static string Title {
            get { return ResourceManager.GetString("Title", resourceCulture); }
        }
        public static string Route {
            get { return ResourceManager.GetString("Route", resourceCulture); }
        }
        public static string PermissionCode {
            get { return ResourceManager.GetString("PermissionCode", resourceCulture); }
        }
    }
}
