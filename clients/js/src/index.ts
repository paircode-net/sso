/** Claim names aligned with SSO.Shared SsoClaimTypes (feature 00004). */
export const SsoClaimTypes = {
  organizationId: "organization_id",
  branchId: "branch_id",
  permissions: "permissions",
  permissionVersion: "perm_ver",
} as const;

export type SsoJwtPayload = {
  sub?: string;
  organization_id?: string;
  branch_id?: string;
  permissions?: string | string[];
  perm_ver?: string;
  [key: string]: unknown;
};

/** Decode JWT payload without verifying signature (verify via BFF / API gateway). */
export function parseJwtPayload(accessToken: string): SsoJwtPayload {
  const parts = accessToken.split(".");
  if (parts.length < 2) {
    throw new Error("Invalid JWT");
  }

  const json = base64UrlDecode(parts[1]);
  return JSON.parse(json) as SsoJwtPayload;
}

export function getPermissions(payload: SsoJwtPayload): string[] {
  const raw = payload.permissions ?? payload[SsoClaimTypes.permissions];
  if (raw == null) {
    return [];
  }

  if (Array.isArray(raw)) {
    return [...new Set(raw.map(String))];
  }

  return [String(raw)];
}

export function getPermissionVersion(payload: SsoJwtPayload): string | undefined {
  const value = payload.perm_ver ?? payload[SsoClaimTypes.permissionVersion];
  return value == null ? undefined : String(value);
}

export function requirePermission(
  payload: SsoJwtPayload,
  ...permissionCodes: string[]
): boolean {
  if (permissionCodes.length === 0) {
    return false;
  }

  const granted = new Set(getPermissions(payload).map((p) => p.toLowerCase()));
  return permissionCodes.some((code) => granted.has(code.toLowerCase()));
}

export function getOrganizationId(payload: SsoJwtPayload): string | undefined {
  const value = payload.organization_id ?? payload[SsoClaimTypes.organizationId];
  return value == null ? undefined : String(value);
}

export function getBranchId(payload: SsoJwtPayload): string | undefined {
  const value = payload.branch_id ?? payload[SsoClaimTypes.branchId];
  return value == null ? undefined : String(value);
}

function base64UrlDecode(input: string): string {
  const padded = input + "=".repeat((4 - (input.length % 4)) % 4);
  const b64 = padded.replace(/-/g, "+").replace(/_/g, "/");
  return Buffer.from(b64, "base64").toString("utf8");
}
