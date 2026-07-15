# -*- coding: utf-8 -*-
"""Generate Phase 3 Identity authz aggregates. Does NOT overwrite IdentityAuditableEntity."""
from pathlib import Path
import importlib.util

SPEC = importlib.util.spec_from_file_location(
    "gen_phase1", Path(__file__).with_name("gen_phase1_identity.py")
)
gen = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(gen)

ROOT = gen.ROOT


def gen_simple_crud(entity, plural, props, unique_prop):
    gen.gen_simple_crud(entity, plural, props, unique_prop)
    map_path = ROOT / f"SSO.Infrastructures.Data/Identity/EntityMappings/{entity}Map.cs"
    text = map_path.read_text(encoding="utf-8").replace("datetimeoffset", "datetime2")
    map_path.write_text(text, encoding="utf-8")
    print("fixed datetime2", map_path.name)


print("Generating Phase 3 CRUDs...")
gen_simple_crud("Permission", "Permissions", [("Code", "string", 128), ("Name", "string", 256)], "Code")
gen_simple_crud("Role", "Roles", [("Code", "string", 128), ("Name", "string", 256)], "Code")
gen_simple_crud(
    "RolePermission",
    "RolePermissions",
    [("RoleId", "Guid", 0), ("PermissionId", "Guid", 0)],
    None,
)
gen_simple_crud(
    "ClientProductBinding",
    "ClientProductBindings",
    [("ClientId", "string", 128), ("ProductId", "Guid", 0)],
    "ClientId",
)
gen_simple_crud(
    "Branch",
    "Branches",
    [
        ("OrganizationId", "Guid", 0),
        ("Name", "string", 128),
        ("Code", "string", 64),
    ],
    None,
)
gen_simple_crud(
    "UserRoleAssignment",
    "UserRoleAssignments",
    [
        ("UserId", "Guid", 0),
        ("RoleId", "Guid", 0),
        ("OrganizationId", "Guid", 0),
        ("ProductId", "Guid", 0),
    ],
    None,
)

print("Phase 3 CRUD generation done.")
