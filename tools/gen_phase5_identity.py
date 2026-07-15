# -*- coding: utf-8 -*-
"""Generate Phase 5 MenuItem CRUD."""
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


gen_simple_crud(
    "MenuItem",
    "MenuItems",
    [
        ("ProductId", "Guid", 0),
        ("Code", "string", 64),
        ("Title", "string", 128),
        ("Route", "string", 256),
        ("PermissionCode", "string", 128),
    ],
    None,
)
print("Phase 5 MenuItem generation done.")
