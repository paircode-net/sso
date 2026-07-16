import assert from "node:assert/strict";
import test from "node:test";
import {
  getPermissionVersion,
  getPermissions,
  parseJwtPayload,
  requirePermission,
} from "./index.js";

function encode(obj: unknown): string {
  return Buffer.from(JSON.stringify(obj), "utf8")
    .toString("base64")
    .replace(/=/g, "")
    .replace(/\+/g, "-")
    .replace(/\//g, "_");
}

test("parseJwtPayload + requirePermission", () => {
  const payload = {
    sub: "u1",
    permissions: ["sso.access", "hq.reports"],
    perm_ver: "42",
    organization_id: "11111111-1111-1111-1111-111111111111",
  };
  const token = `hdr.${encode(payload)}.sig`;
  const parsed = parseJwtPayload(token);
  assert.deepEqual(getPermissions(parsed).sort(), ["hq.reports", "sso.access"]);
  assert.equal(getPermissionVersion(parsed), "42");
  assert.equal(requirePermission(parsed, "hq.reports"), true);
  assert.equal(requirePermission(parsed, "missing"), false);
});
