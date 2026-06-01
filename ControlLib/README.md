
# Parkan Control System RE Notes

High-level model
----------------
The Control system is the runtime logic layer that binds an object scheme to:
- mesh pieces / MSH nodes,
- control points / CPT records,
- damage/life pieces / NDP records,
- functional item records / CTL items,
- command hooks / CTL command lists.

A robot is not represented by one file. The scheme tree chooses which resources
are loaded and how they attach to each other.

Observed resource group:
- MSH = visual/animated mesh, node tree, pieces, sockets.
- WEA = material text table. Managed separately; not part of Control logic.
- CPT = named control points / sockets / effect anchors / weapon axes.
- NDP = per-piece damage/life records and damage effect resources.
- CTL = control root params, state graph, piece bindings, items, commands.

Root object loading:
1. Load MSH.
2. Load WEA/materials separately.
3. Load CPT records into runtime CPT refs.
4. Load NDP records into runtime NDP refs.
5. Load CTL:
    - root/base control limits,
    - optional control state graph,
    - edge weight matrix,
    - piece bindings,
    - item records,
    - command hook table,
    - command lists.
6. Apply piece binding defaults and item binding defaults.
7. Run on-load command list.

Attached object loading:

Attached resources, such as turrets and guns, use the same CPT/NDP/CTL files
but their local piece indices are remapped into the parent runtime mesh.

Strong current rule:
- local/source piece 0 is a virtual attach root.
- local/source piece N > 0 maps to first runtime piece for that MSH tag + N - 1.

NDP record 0 is skipped for attached objects because it corresponds to that
virtual root/attach node.

Scheme AttachInd meaning:

Depends on scheme node category:
- Part / Armour / Ammo:
AttachInd selects an existing CTL item slot to configure/replace.
- Turret / Cannon:
AttachInd selects an MSH attach piece/socket in the parent object.

This is strongly supported by chassis, turret, gun, and ammo relationship.

CControl mental model
---------------------
CControlBase is the owner of the whole runtime control state for one composed
object or attached control resource. It owns:
- base/effective movement limits,
- CPT refs,
- NDP refs,
- piece bindings,
- item instances,
- state graph,
- command lists,
- resource/effect/object references.

base_control_limits:
- Best understood as immutable design-time/root CTL limits.
- It points into the raw CTL root params and is used as baseline/cap.

runtime_control_limits:
- Best understood as mutable effective limits used by simulation.
- It starts as a copy of base_control_limits, but is recomputed from damage,
engine output, wheel/drive integrity, and runtime speed modifiers.

Important runtime movement formula:
effective movement depends on:
- engine output scale,
- average left/right drive integrity,
- current mobility integrity / max mobility integrity,
- runtime tangential speed scale.

CItem mental model
------------------
CTL items are functional subsystems, not just visual mesh pieces.

Examples:
- `0x05` Engine
- `0x13` Power supply / battery
- `0x09` Force shield
- `0x0A` Detection shield
- `0x0F` Repair system
- `0x1B` Armour
- `0x08` Sensor/radar
- `0x04` Targeting/fire-control
- `0x02` Projectile weapon
- `0x01` Orientation actuator / turret drive
- `0x14` Wheel mobility unit
- `0x03` Mobility support
- `0x15` Deflector

Item records contain:
- item type,
- local piece index,
- flags,
- optional indices / state fields,
- common float params,
- optional resource ref,
- binding refs into CTL piece bindings,
- optional item name.

Most item types share the same CTL binary parser.
Types 0x02 and 0x1E do extra runtime initialization, but currently do not appear
to consume extra bytes from the CTL stream.

Repair system note:

CItemBase::SetItemRuntimeState(state) changes runtime state, marks net sync,
and resets transition/progress.
Repair system treats state/command 0x40 as toggle:
- if not ON 0x20 -> becomes ON 0x20
- if already ON 0x20 -> becomes OFF 0
Critical decay forces repair systems OFF by calling SetItemRuntimeState(0).

Critical decay model
--------------------
Critical decay starts when:
- total current life drops below a critical reference threshold, OR
- wheel mobility integrity is catastrophically low.

For a 4-wheel unit, the wheel-trigger condition effectively means:
remaining wheel integrity <= 1 wheel-equivalent.

On entering critical decay:
- on-enter command list is executed,
- current total life is captured as critical_decay_reference_life_amount,
- decay damage is applied as:
  Settings.critical_decay_rate_per_second
    * critical_decay_reference_life_amount
    * deltaSeconds
- repair systems are forced off.

NDP mental model
----------------
NDP records represent per-piece life/damage contribution and reference the
effect resource used when that damage piece is destroyed/depleted.

NDP resources observed so far are explode_*.exp resources.
ControlNdpRef.damage_effect_resource_index indexes a loaded damage-effect cache.

NDP flag 0x10 is best understood as DEPLETED / DESTROYED.
Functions that compute active damage-effect bounds skip refs with this flag
unless explicitly told to include all/depleted refs.

CPT mental model
----------------
CPT records are named control points:
- effect anchors,
- muzzle axes,
- wheel contact points,
- camera/targeting points,
- sign/bounds points,
- turret center/direction points.

CptRecordRaw has two piece-like fields:
OwnerPieceIndex  at +0x04
TargetPieceIndex at +0x08

Their exact distinction is not fully final.
Debug model:
- OwnerPieceIndex is the piece the point is attached/owned by.
- TargetPieceIndex is the associated visual/functional piece.
  Many chassis CPT points are owner/root-owned but target a wheel piece.
  For debugging, always display both.

Piece binding mental model
--------------------------
CTL piece bindings connect:
- an MSH/local piece,
- optional CPT refs,
- animation frame/range data,
- default binding value,
- flags controlling animation default behavior.

Bindings are used by items through item binding refs.
Binding default value is used both as:
- fallback for item binding ref values,
- initial animation transition/progress value.

Known binding flags:
0x01 wrap default value
0x02 invert default value
0x04 skip animation init
0x08 auto-add/auto-reference by item system; exact target still provisional
0x100 conditional link target, added at runtime from MSH piece flags

Command system mental model
---------------------------
CTL command table has 21 hook slots.

Known slots:
- slot 0 = on_load
- slot 6 = on_enter_critical_decay
- slot 7 = on_exit_critical_decay

Other hook slots are still being mapped.

-1 means no command list.

Non-negative value indexes into the CTL command list array.

Command record is fixed 0x64 bytes:
- condition flags/masks,
- opcode,
- five raw args,
- resource ref.

Command args must be viewed both as int and float. Meaning is opcode-specific.
For example:
- CPT indices are ints.
- effect radius/scale/range can be float bit patterns.

Known command direction:
- 0x03 spawn effect using one CPT/bounds-like anchor.
- 0x04 spawn oriented effect from three CPT/control points.
- 0x16 spawn object from three CPT/control points.
- 0x0A start effect by local id/index.
- 0x0B request stop effect by local id/index.
- 0x12 activate effect.
- 0x13 deactivate effect.

Network/update timing model
---------------------------
CControlClassic::UpdateTick(now_ms) receives an absolute timestamp in milliseconds.
It is not a delta.

Important timing units:
- current_control_time_ms      absolute ms
- frame_delta_ms               ms since previous control tick
- last_slow_update_time_ms     absolute ms
- control_state_sim_time_ms    absolute ms loop cursor
- ProcessNdpLifeAndMobilityState(deltaSeconds) receives seconds
- UpdateItemRuntimeEffects(deltaSeconds) receives seconds

Network transform samples:

- Vector3 position;
- Quaternion rotation;
- float strafe_angle_z;

Size = 0x20.

Interpolation uses a normalized fraction, not seconds.
