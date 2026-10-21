"""Builds the low-poly chair and saves model/chair.blend and model/chair.fbx.

Run from the repository root (exit code 1 if the script fails):
    blender -b --python-exit-code 1 -P model/make_chair.py

Units are metres, Z is up, the chair faces -Y (Blender's front view).
Colours come from unity/Assets/Scripts/Visual/Palette.cs, so the chair matches the game.
"""

import re
from pathlib import Path

import bmesh
import bpy

MODEL_DIR = Path(__file__).resolve().parent
PALETTE_FILE = MODEL_DIR.parent / "unity" / "Assets" / "Scripts" / "Visual" / "Palette.cs"
BLEND_FILE = MODEL_DIR / "chair.blend"
FBX_FILE = MODEL_DIR / "chair.fbx"

SEAT_TOP = 0.45
SEAT_THICKNESS = 0.06
SEAT_WIDTH = 0.44
SEAT_DEPTH = 0.42
LEG_SIZE = 0.04
LEG_INSET = 0.01
BACK_THICKNESS = 0.03
CHAIR_HEIGHT = 0.90

BEVEL_WIDTH = 0.006
BEVEL_SEGMENTS = 2

# Same shades as the game's materials: Wood = Darken(Peach, 0.4) like the fence and tree trunks.
WOOD_DARKEN = 0.4
WOOD_ROUGHNESS = 0.9
FABRIC_ROUGHNESS = 0.8


def main():
    reset_scene()
    palette = read_palette()
    wood = create_material("Wood", darken(palette["Peach"], palette["Ink"], WOOD_DARKEN), WOOD_ROUGHNESS)
    fabric = create_material("Fabric", palette["Rose"], FABRIC_ROUGHNESS)

    parts = create_parts(wood, fabric)
    apply_transforms(parts)
    for part in parts:
        add_bevel(part)
        part.data.shade_flat()

    check_dimensions(parts)
    save_blend()
    export_fbx()


def reset_scene():
    """Starts from an empty factory scene, whatever the user's startup file contains."""
    bpy.ops.wm.read_factory_settings(use_empty=True)
    units = bpy.context.scene.unit_settings
    units.system = "METRIC"
    units.scale_length = 1.0


def read_palette():
    """Reads the five base colours (sRGB, 0..1) from the game's Palette.cs."""
    if not PALETTE_FILE.is_file():
        raise FileNotFoundError(f"Palette not found: {PALETTE_FILE}")

    pattern = r"public static readonly Color (\w+) = FromHex\(0x([0-9A-Fa-f]{6})\);"
    found = re.findall(pattern, PALETTE_FILE.read_text(encoding="utf-8"))
    palette = {name: hex_to_srgb(value) for name, value in found}
    for name in ("Peach", "Rose", "Ink"):
        if name not in palette:
            raise ValueError(f"Colour {name} is missing in {PALETTE_FILE}")
    return palette


def hex_to_srgb(value):
    rgb = int(value, 16)
    return ((rgb >> 16) / 255.0, ((rgb >> 8) & 0xFF) / 255.0, (rgb & 0xFF) / 255.0)


def darken(color, ink, amount):
    """Palette.Darken: blends towards Ink in sRGB, exactly like Color.Lerp in the game."""
    return tuple(c + (i - c) * amount for c, i in zip(color, ink))


def srgb_to_linear(channel):
    """Principled BSDF expects scene-linear colours; the palette is sRGB."""
    return channel / 12.92 if channel <= 0.04045 else ((channel + 0.055) / 1.055) ** 2.4


def create_material(name, srgb, roughness):
    material = bpy.data.materials.new(name)
    if material.node_tree is None:
        raise RuntimeError(f"Material {name} has no node tree")

    principled = next((node for node in material.node_tree.nodes if node.type == "BSDF_PRINCIPLED"), None)
    if principled is None:
        raise RuntimeError(f"Material {name} has no Principled BSDF node")

    linear = tuple(srgb_to_linear(c) for c in srgb)
    principled.inputs["Base Color"].default_value = (*linear, 1.0)
    principled.inputs["Roughness"].default_value = roughness
    # The colour of the material in Solid view, so the viewport matches the render.
    material.diffuse_color = (*linear, 1.0)
    return material


def create_parts(wood, fabric):
    """Six separate box objects: seat, back and four legs, positioned in metres."""
    seat_bottom = SEAT_TOP - SEAT_THICKNESS
    leg_x = SEAT_WIDTH / 2 - LEG_INSET - LEG_SIZE / 2
    leg_y = SEAT_DEPTH / 2 - LEG_INSET - LEG_SIZE / 2
    back_height = CHAIR_HEIGHT - SEAT_TOP

    parts = [
        create_box("Seat", (SEAT_WIDTH, SEAT_DEPTH, SEAT_THICKNESS), (0, 0, seat_bottom + SEAT_THICKNESS / 2), fabric),
        # The back stands on the rear edge of the seat (+Y is the back of the chair).
        create_box("Back", (SEAT_WIDTH, BACK_THICKNESS, back_height),
                   (0, SEAT_DEPTH / 2 - BACK_THICKNESS / 2, SEAT_TOP + back_height / 2), wood),
    ]
    # Left and right are the chair's own sides, as for someone sitting on it facing -Y: left is +X.
    legs = {"Leg_FrontLeft": (leg_x, -leg_y), "Leg_FrontRight": (-leg_x, -leg_y),
            "Leg_BackLeft": (leg_x, leg_y), "Leg_BackRight": (-leg_x, leg_y)}
    for name, (x, y) in legs.items():
        parts.append(create_box(name, (LEG_SIZE, LEG_SIZE, seat_bottom), (x, y, seat_bottom / 2), wood))
    return parts


def create_box(name, size, center, material):
    """A unit cube placed and sized through its transform; apply_transforms bakes it into the mesh."""
    mesh = bpy.data.meshes.new(name)
    cube = bmesh.new()
    bmesh.ops.create_cube(cube, size=1.0)
    cube.to_mesh(mesh)
    cube.free()
    mesh.materials.append(material)

    part = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(part)
    part.location = center
    part.scale = size
    return part


def apply_transforms(parts):
    """Ctrl+A → All Transforms. Applying the location also moves every origin to the world origin,
    which is the centre of the chair's base on the floor, so the chair stands on the ground in Unity."""
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


def add_bevel(part):
    """Slightly chamfered edges. The modifier stays live in the .blend; the FBX export applies it."""
    bevel = part.modifiers.new("Bevel", "BEVEL")
    bevel.width = BEVEL_WIDTH
    bevel.segments = BEVEL_SEGMENTS
    bevel.limit_method = "ANGLE"


def check_dimensions(parts):
    """Measures the evaluated geometry (bevel included) and fails if the proportions are off."""
    depsgraph = bpy.context.evaluated_depsgraph_get()
    points = []
    for part in parts:
        evaluated = part.evaluated_get(depsgraph)
        mesh = evaluated.to_mesh()
        points += [evaluated.matrix_world @ vertex.co for vertex in mesh.vertices]
        print(f"{part.name:<15} vertices {len(mesh.vertices):>3}  faces {len(mesh.polygons):>3}  "
              f"size {tuple(round(d, 3) for d in part.dimensions)}  material {part.data.materials[0].name}")
        evaluated.to_mesh_clear()

    low = [min(p[axis] for p in points) for axis in range(3)]
    high = [max(p[axis] for p in points) for axis in range(3)]
    print(f"chair bounds: x {low[0]:.3f}..{high[0]:.3f}  y {low[1]:.3f}..{high[1]:.3f}  z {low[2]:.3f}..{high[2]:.3f}")
    if abs(low[2]) > 1e-4 or abs(high[2] - CHAIR_HEIGHT) > 1e-4:
        raise RuntimeError(f"Chair should stand on z = 0 and be {CHAIR_HEIGHT} m tall")


def save_blend():
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_FILE))
    print(f"saved {BLEND_FILE}")


def export_fbx():
    """Settings for Unity: metres stay metres and Unity gets no extra rotation.

    axis_forward/axis_up convert Blender's Z-up space to Unity's Y-up space, and
    bake_space_transform bakes that conversion into the mesh data instead of leaving a -90° X rotation
    on every object (the option is marked experimental only for armatures and animation; these are static meshes).
    FBX_SCALE_ALL writes a unit scale of 1, so Unity imports 1 Blender metre as 1 Unity metre with scale 1, not 100.
    use_mesh_modifiers bakes the bevel; mesh_smooth_type FACE keeps the faces flat-shaded.
    """
    result = bpy.ops.export_scene.fbx(
        filepath=str(FBX_FILE),
        use_selection=False,
        object_types={"MESH"},
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
    )
    if result != {"FINISHED"}:
        raise RuntimeError(f"FBX export failed: {result}")
    print(f"exported {FBX_FILE}")


main()
