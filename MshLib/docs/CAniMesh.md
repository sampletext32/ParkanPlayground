# CAniMesh / MSH Loading and Joint Bounds — Key Summary

## New findings discovered in this chat

* `tag` is best understood as a runtime **loaded submodel tag** for pieces created from one `.msh` load call.
* `tag == 0` is the primary/root `.msh` model.
* `tag != 0` means an attached/additional `.msh` whose source node `0` is skipped and used as a virtual attach root.
* One `CAniMesh` can aggregate multiple `.msh` resources into a single flat `pieces_vector`.
* Attached `.msh` pieces are not kept as separate models at runtime; their nodes are remapped into absolute `CAniMesh::pieces_vector` indices.
* `attach_parent_absolute_piece_index` is an absolute index in `CAniMesh::pieces_vector`, not a local index inside the attached `.msh`.
* The first runtime piece created by each `.msh` load should be marked as a submodel/subtree root.
* `MSH_PIECE_FLAG_TAG_ROOT` should be renamed to `MSH_PIECE_FLAG_SUBMODEL_ROOT` or `MSH_PIECE_FLAG_LOADED_SUBTREE_ROOT`.
* `ComputeJointBoundingBox` is the authoritative recursive joint/subtree bounds function.
* `ComputeJointBoundingSphere` uses specialized fast paths for single-piece and root whole-mesh bounds, but for non-root subtree bounds it delegates to `ComputeJointBoundingBox` and wraps the resulting AABB in a sphere.
* Geometry-less pieces are valid helper/socket joints: their bounds become a point or zero-radius sphere at the joint transform origin.
* The renamed filter `g_mesh_filter_only_subtree_and_exclude_default_bounds` clarifies that cached “default filter” bounds are really cached bounds for a specific reduced subtree filter.

---

## Key function: `CAniMesh::AppendMshResourcePieces (AniMesh.dll/sub_1000ac70)`

```
typedef struct GmsgAppendResourcePayload_CAniMesh {
    char archive_name[32];
    char msh_archive_entry_name[32];
    uint msh_tag;
    uint attach_parent_absolute_piece_index;
    uint material_id_hi;
} GmsgAppendResourcePayload_CAniMesh;
```

Core behavior:

```text
One call loads one .msh resource.
All pieces created by that call receive the same tag.
The created pieces are appended to CAniMesh::pieces_vector.
```

For `tag == 0`:

```text
source MSH 0x01 node 0 -> piece[0]
source MSH 0x01 node 1 -> piece[1]
source MSH 0x01 node 2 -> piece[2]
...
```

For `tag != 0`:

```text
source MSH 0x01 node 0 is skipped
source MSH 0x01 node 1 -> first newly created piece
source MSH 0x01 node 2 -> next newly created piece
...
```

Attached parent remap rule:

```text
source parent == 0
    -> attach_parent_absolute_piece_index

source parent > 0
    -> first_new_piece_index + (source_parent - 1)

source parent == 0xFFFF
    -> -1 / no parent
```

This means internal parent hierarchy inside the attached `.msh` is preserved, but all indices are converted to absolute `pieces_vector` indices.

---

Runtime piece flag:

```c
#define MSH_PIECE_FLAG_SUBMODEL_ROOT 0x01000000
```

Meaning:

```text
Set on the first runtime piece created by one .msh load call.
For tag == 0, this is the main model root.
For tag != 0, this is the attached submodel/subtree root.
```

### This function creates a runtime representation of a .msh 0x01 piece

```
typedef struct MSH_piece {
    uint msh_tag_0x00;
    int local_parent_index_base;
    uint msh0x01_node_index;
    undefined4 field_12;
    undefined4 material_id;
    EMshPieceFlags flags;
    int parent_piece_index;
    EMeshPieceState state;
    Matrix4x4 world_pose_matrix;
    Matrix4x4 mesh_space_pose_matrix;
    Matrix4x4 local_pose_matrix;
    Quaternion orientation_blend_start_quat;
    Quaternion orientation_blend_end_quat;
    float anim1_time_start;
    float anim1_time_target;
    float anim2_time_start;
    float anim2_time_target;
    bool is_pose_cache_valid;
    bool exclude_from_pose_update_order;
    bool uses_local_anim_blend;
    undefined1 has_orientation_blend;
    float local_anim_transition_progress;
    float local_anim_blend_factor;
    float cached_anim1_sample_time;
    float cached_anim2_sample_time;
    float render_phase_0x124??;
    undefined4 material_phase_0x128??;
    MSH_Reader * msh_reader;
} MSH_piece;
```

---

## Key type: `MSH_0x01_node`

`MSH` component `0x01` is a source node / piece table.

Important fields:

```c
typedef struct MSH_0x01_node {
    uint16_t flags;
    uint16_t parent_index_or_link;
    uint16_t anim_map_start_0x13;
    uint16_t fallback_key_0x08;
    uint16_t msh02_slot_indices_by_state_and_lod[3][5];
} MSH_0x01_node;
```

Meaning:

```text
MSH 0x01 node
    -> becomes an MSH_piece at runtime
    -> has parent_index_or_link
    -> has MSH01 flags
    -> maps LOD/state to MSH 0x02 geometry slots
```

---

## Key type: `MSH_02_geometry_slot`

`MSH` component `0x02` stores geometry slot metadata and local bounds.

Preferred structure:

```c
typedef struct MSH_02_geometry_slot {
    uint16_t tri_start_0x07;
    uint16_t tri_count_0x07;
    uint16_t batch_start_0x0d;
    uint16_t batch_count_0x0d;

    Vector3 local_minimum;
    Vector3 local_maximum;
    Sphere bounding_sphere;

    float base_xy_area;
    float base_volume;

    uint32_t opaque_0x38;
    uint32_t opaque_0x3C;
    uint32_t opaque_0x40;
} MSH_02_geometry_slot;
```

## Key function: `ResolveMsh0x02SlotBy_LOD_and_state`


The bounds functions call it as default geometry lookup:

```c
MSH_02_geometry_slot *
ResolveMsh0x02SlotBy_LOD_and_state(
    MSH_piece *this,
    EMeshPieceLodLevel lod_level,
    EMeshPieceState state
);

typedef enum EMeshPieceLodLevel {
    LOD_LEVEL_MAX_0 = 0,
    LOD_LEVEL_MINUS_1 = 1,
    LOD_LEVEL_MINUS_2 = 2,
    LOD_LEVEL_MINUS_3 = 3,
    LOD_LEVEL_MINUS_4 = 4,
} EMeshPieceLodLevel;

typedef enum EMeshPieceState {
    MODEL_STATE_DEFAULT = -1,
    MODEL_STATE_REGULAR = 0,
    MODEL_STATE_COLLAPSED = 1,
    _MODEL_STATE_UNKNOWN_2 = 2,
} EMeshPieceState;
```

---

## Key function: `IJointMesh_of_AniMesh::ComputeJointBoundingBox`

Preferred name:

```c
AniMesh_IJointMesh::ComputeJointBoundingBox
```

Core behavior:

```text
1. Read the joint/piece placement matrix in requested space.
2. Resolve default geometry slot for the queried piece.
3. If the piece has geometry:
   - build local box from slot local_minimum/local_maximum;
   - optionally scale by mesh_scale;
   - transform all corners by the joint matrix.
4. If the piece has no geometry:
   - create a degenerate box at joint transform origin.
5. If scope is single-piece, return.
6. If queried piece is root piece 0, return cached whole-mesh bounds.
7. Otherwise recursively include matching children, controlled by JointBoundsFilter and MSH01 flags.
```

Important meaning:

```text
This is the main recursive piece-tree bounds function.
```

Geometry-less piece meaning:

```text
No MSH 0x02 slot
    -> helper/socket joint
    -> point-sized bounds at joint transform origin
```

---

## Key function: `IJointMesh_of_AniMesh::ComputeJointBoundingSphere`

Preferred name:

```c
AniMesh_IJointMesh::ComputeJointBoundingSphere
```

Core behavior:

```text
If scope != SINGLE_PIECE and piece != 0:
    ComputeJointBoundingBox(...)
    Convert resulting AABB to center/radius sphere.

If scope != SINGLE_PIECE and piece == 0:
    Use cached whole-mesh or cached filtered mesh sphere.

If scope == SINGLE_PIECE:
    Use MSH_02_geometry_slot::bounding_sphere.
    If no geometry slot, return zero-radius sphere at joint origin.
```

Important meaning:

```text
Subtree sphere is not a tight recursive sphere.
It is an AABB-derived sphere from ComputeJointBoundingBox.
```

---

## Important conceptual model

```text
CAniMesh
    owns one flat pieces_vector

Each loaded .msh
    contributes one tagged group of pieces

MSH 0x01
    source node hierarchy inside one .msh

MSH_piece
    runtime node/piece inside CAniMesh::pieces_vector

parent_piece_index
    absolute runtime parent index in CAniMesh::pieces_vector

msh_tag
    tells which loaded .msh/submodel this runtime piece came from
```

Runtime result:

```text
Multiple .msh files become one combined piece tree.
The tag preserves source submodel grouping.
The parent indices define the actual runtime hierarchy.
```

---

## Example runtime structure

```text
CAniMesh pieces_vector

piece[0] body_root                     tag = 0, SUBMODEL_ROOT
├─ piece[1] left_track                 tag = 0
├─ piece[2] right_track                tag = 0
└─ piece[3] turret_socket              tag = 0
   └─ piece[4] turret_base             tag = 1, SUBMODEL_ROOT
      └─ piece[5] turret_rotor         tag = 1
         ├─ piece[6] cannon_socket     tag = 1
         │  └─ piece[8] cannon_body    tag = 2, SUBMODEL_ROOT
         └─ piece[7] rocket_socket     tag = 1
            └─ piece[9] launcher_body  tag = 3, SUBMODEL_ROOT
```

Key rule:

```text
tag groups pieces by loaded .msh.
parent_piece_index builds the actual hierarchy.
```
