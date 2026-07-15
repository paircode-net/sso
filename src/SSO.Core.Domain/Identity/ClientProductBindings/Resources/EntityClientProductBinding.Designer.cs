namespace SSO.Core.Domain.Identity.ClientProductBindings.Resources {
    using System;
    public sealed class EntityClientProductBinding {
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;
        internal EntityClientProductBinding() { }
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    resourceMan = new global::System.Resources.ResourceManager("SSO.Core.Domain.Identity.ClientProductBindings.Resources.EntityClientProductBinding", typeof(EntityClientProductBinding).Assembly);
                }
                return resourceMan;
            }
        }
        public static global::System.Globalization.CultureInfo Culture {
            get { return resourceCulture; }
            set { resourceCulture = value; }
        }
        public static string ClientId {
            get { return ResourceManager.GetString("ClientId", resourceCulture); }
        }
        public static string ProductId {
            get { return ResourceManager.GetString("ProductId", resourceCulture); }
        }
    }
}
