typedef struct CLandscape {
    IComponent * IComponent;
    CGameObject game_object;
    ITerrain * ITerrain;
    IMesh2 * IMesh2;
    I0x1 * I0x1;
    IControl * IControl;
    I0x22 * IUnknown_0x22;
    undefined4 01_data_ptr;
    MSH_02_file * msh_02_data_ptr;
    undefined4 0b_data_ptr;
// collapse unknown fields
    int[3][5] field_532;
// collapse unknown fields
    undefined4 IComponent_owner;
    KindaArray array_0x278;
    int _count_unk_obj_array_elemsize_212;
    undefined4 vertices_count;
    uint 15_element_count;
// collapse unknown fields
    Vector3 * 03_vertices_data_ptr;
    int 03_vertices_stride;
    undefined4 04_data_ptr;
    undefined4 04_stride;
// collapse unknown fields
    undefined4 05_data_ptr;
    undefined4 05_stride;
    undefined4 12_microtexture_mapping_data_ptr;
    undefined4 12_microtexture_mapping_stride;
// collapse unknown fields
    MshMetaForLandscape * mshmeta_ptr;
// collapse unknown fields
    KindaArray array_0x7a40;
    BoundingBox bounding_box_from_msh_02;
    IMatManager * * IMatManager;
    undefined4 wear_ptr;
    IEffectManager * * ResolvedIEffectManager;
    ISystemArealMap * * ResolvedISystemArealMap;
    ICollManager * * ResolvedICollManager;
    ISoundPool * * ISoundPool;
    I3DSound * * I3DSound;
    ILightManager * * ILightManager_owned;
    KindaArray array_0x7c00;
    KindaArray array_0x7c0c;
    CShade * Shade;
    undefined4 I3DRender??;
    undefined4 field_31776;
    undefined4 flags_of_mesh_part;
    INResFile * * INResFile;
    KindaArray array_0x7c2c;
    undefined4 current_visible_primitives_count;
    IMesh2 * * meshes;
    IGameObject * * game_objects;
// collapse unknown fields
    KindaArray array_0x7c84;
    undefined4 * field_31888;
    undefined4 _CheckMaxBasementAngleStep;
    CollisionContext m_CollisionContext;
    BoundingBox bounding_box;
} CLandscape;