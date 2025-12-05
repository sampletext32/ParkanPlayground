// Decompiled C Code:

/* WARNING: Enum "DRAW_PRIMITIVE_FLAGS": Some values do not have unique names */

CLandscape * __thiscall
CLandscape::ctor(CLandscape *this,LPCSTR materialConfigName,char *landscapeBaseName,
                IComponent **owner)

{
  CGameObject *pCVar1;
  BoundingBox *pBVar2;
  char cVar3;
  float fVar4;
  float fVar5;
  float fVar6;
  ushort uVar7;
  int (*paiVar8) [5];
  undefined4 *puVar9;
  char *mem;
  ushort *mem_00;
  I3DRender **ppIVar10;
  I3DSound **ppIVar11;
  ISoundPool **ppIVar12;
  IMatManager **ppIVar13;
  undefined4 uVar14;
  ILightManager **ppIVar15;
  IComponent **ppIVar16;
  BOOL BVar17;
  SystemArealMap *this_00;
  INResFile **ppIVar18;
  nres_metadata_item *pnVar19;
  Vector3 *pVVar20;
  MSH_15_element *pMVar21;
  MSH_02_file *pMVar22;
  MshMetaForLandscape *pMVar23;
  MSH_15_element *puVar20;
  int *piVar24;
  int *piVar25;
  undefined4 *puVar26;
  CShade *pCVar27;
  int iVar28;
  uint uVar29;
  uint uVar30;
  Vector3 *pfVar29;
  int iVar31;
  byte (*data_ptr) [212];
  char *pcVar32;
  Vector3 *pVVar33;
  BoundingBox *pfVar35;
  Vector3 *pfVar34;
  float fVar34;
  char *pcVar35;
  ushort *puVar36;
  ushort *puVar37;
  char *pcVar38;
  BoundingBox *pBVar39;
  Vector3 *pfVar40;
  undefined4 *puVar40;
  byte *pbVar41;
  uint uStack_1ec;
  int iStack_1e4;
  BoundingBox fStack_1e0;
  Vector3 VStack_178;
  Vector3 VStack_16c;
  Vector3 VStack_160;
  Vector3 VStack_154;
  Vector3 VStack_148;
  Vector3 VStack_13c;
  float fStack_130;
  float fStack_12c;
  float fStack_128;
  float fStack_124;
  float fStack_120;
  float fStack_11c;
  float fStack_118;
  float fStack_114;
  float fStack_110;
  Vector3 fStack_10c;
  Vector3 fStack_f4;
  Vector3 fStack_e8;
  Vector3 fStack_d8;
  Vector3 VStack_78;
  Vector3 VStack_6c;
  Vector3 VStack_60;
  Vector3 VStack_54;
  Vector3 VStack_48;
  Vector3 VStack_3c;
  Vector3 VStack_30;
  Vector3 VStack_24;
  StackException exception2;
  StackException exception;
  
  exception.Offset = -1;
  exception.Handler = (undefined4 *)&LAB_1006320e;
  exception.ExceptionList = (undefined4 *)ExceptionList;
  ExceptionList = &exception;
  this->IComponent = (IComponent *)&PTR_pure_virtual_100655d0;
  CGameObject::ctor(&this->game_object,owner);
  this->ITerrain = (ITerrain *)&PTR_pure_virtual_100655a0;
  this->IMesh2 = (IMesh2 *)&PTR_pure_virtual_10065560;
  this->I0x1 = (I0x1 *)&PTR_pure_virtual_1006552c;
  this->IControl = (IControl *)&PTR_pure_virtual_100654d8;
  this->IUnknown_0x22 = (I0x22 *)&PTR_pure_virtual_100654c8;
  exception.Offset = 0;
  this->field104_0x1e8 = 0;
  this->field105_0x1ec = -2;
  paiVar8 = this->field115_0x214;
  iVar28 = 3;
  do {
    (*paiVar8)[0] = 0;
    (*paiVar8)[1] = -2;
    (*paiVar8)[2] = -1;
    (*paiVar8)[3] = -1;
    (*paiVar8)[4] = -1;
    paiVar8 = paiVar8 + 1;
    iVar28 = iVar28 + -1;
  } while (iVar28 != 0);
  puVar9 = (undefined4 *)operator_new(8000);
  (this->array_0x278).data = puVar9;
  (this->array_0x278).capacity = 1000;
  (this->array_0x278).count = 0;
  exception.Offset = CONCAT31(exception.Offset._1_3_,1);
  data_ptr = this->unk_obj_array_elemsize_212;
  uStack_1ec = 100;
  do {
    vector_initializer(data_ptr,0xc,8,move_to_eax_return);
    data_ptr = data_ptr + 1;
    uStack_1ec = uStack_1ec + -1;
  } while (uStack_1ec != 0);
  puVar9 = (undefined4 *)operator_new(400);
  (this->array_0x7a40).data = puVar9;
  (this->array_0x7a40).capacity = 100;
  (this->array_0x7a40).count = 0;
  exception.Offset = CONCAT31(exception.Offset._1_3_,2);
  puVar9 = (undefined4 *)operator_new(4000);
  (this->array_0x7c00).data = puVar9;
  (this->array_0x7c00).capacity = 1000;
  (this->array_0x7c00).count = 0;
  exception.Offset = CONCAT31(exception.Offset._1_3_,3);
  puVar9 = (undefined4 *)operator_new(400);
  (this->array_0x7c0c).data = puVar9;
  (this->array_0x7c0c).capacity = 100;
  (this->array_0x7c0c).count = 0;
  exception.Offset = CONCAT31(exception.Offset._1_3_,4);
  puVar9 = (undefined4 *)operator_new(400);
  (this->array_0x7c2c).data = puVar9;
  (this->array_0x7c2c).capacity = 100;
  (this->array_0x7c2c).count = 0;
  exception.Offset = CONCAT31(exception.Offset._1_3_,5);
  puVar9 = (undefined4 *)operator_new(0xa0);
  (this->array_0x7c84).data = puVar9;
  (this->array_0x7c84).capacity = 0x28;
  (this->array_0x7c84).count = 0;
  (this->m_CollisionContext).logicID = Landscape;
  (this->m_CollisionContext).GameObjectID = 0;
  (this->m_CollisionContext).field2_0x8 = 0;
  (this->m_CollisionContext).field3_0xc = 0;
  (this->m_CollisionContext).field4_0x10 = 0;
  (this->m_CollisionContext).field5_0x14 = 0;
  (this->m_CollisionContext).mSweptSphere.position.z = 0.0;
  (this->m_CollisionContext).mSweptSphere.position.y = 0.0;
  (this->m_CollisionContext).mSweptSphere.position.x = 0.0;
  (this->m_CollisionContext).mSweptSphere.targetPosition.z = 0.0;
  (this->m_CollisionContext).mSweptSphere.targetPosition.y = 0.0;
  (this->m_CollisionContext).mSweptSphere.targetPosition.x = 0.0;
  (this->m_CollisionContext).mSweptSphere.radius = -1.0;
  (this->m_CollisionContext).pSweptSphere2 = (SweptSphere *)0x0;
  (this->m_CollisionContext).ResolvedIMesh2 = (IMesh2 **)0x0;
  (this->m_CollisionContext).I0x25 = (void **)0x0;
  (this->m_CollisionContext).ResolvedICollManager = (ICollManager **)0x0;
  this->IComponent = &IComponent_of_CLandscape;
  (this->game_object).vptr = &IGameObject_of_CLandscape;
  this->ITerrain = &ITerrain_of_CLandscape;
  this->IMesh2 = &IMesh2_of_CLandscape;
  this->I0x1 = &I0x1_of_CLandscape;
  this->IControl = &IControl_1006536c;
  this->IUnknown_0x22 = &I0x22_1006535c;
  this->IComponent_owner = owner;
  uVar29 = 0xffffffff;
  exception.Offset = CONCAT31(exception.Offset._1_3_,6);
  pcVar32 = landscapeBaseName;
  do {
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar32 + 1;
  } while (cVar3 != '\0');
  pBVar2 = &this->bounding_box;
  fStack_1e0.TopBackLeft.y = (float)operator_new(~uVar29 + 4);
  uVar29 = 0xffffffff;
  pcVar32 = landscapeBaseName;
  do {
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar32 + 1;
  } while (cVar3 != '\0');
  fStack_1e0.TopBackLeft.x = (float)operator_new(~uVar29 + 5);
  uVar29 = 0xffffffff;
  pcVar32 = landscapeBaseName;
  do {
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar32 + 1;
  } while (cVar3 != '\0');
  mem = (char *)operator_new(~uVar29 + 5);
  uVar29 = 0xffffffff;
  pcVar32 = landscapeBaseName;
  do {
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar32 + 1;
  } while (cVar3 != '\0');
  mem_00 = (ushort *)operator_new(~uVar29 + 4);
  uVar29 = 0xffffffff;
  pcVar32 = landscapeBaseName;
  do {
    pcVar38 = pcVar32;
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    pcVar38 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar38;
  } while (cVar3 != '\0');
  uVar29 = ~uVar29;
  pcVar32 = pcVar38 + -uVar29;
  pcVar38 = (char *)fStack_1e0.TopBackLeft.y;
  for (uVar30 = uVar29 >> 2; uVar30 != 0; uVar30 = uVar30 - 1) {
    *(undefined4 *)pcVar38 = *(undefined4 *)pcVar32;
    pcVar32 = pcVar32 + 4;
    pcVar38 = pcVar38 + 4;
  }
  for (uVar29 = uVar29 & 3; uVar29 != 0; uVar29 = uVar29 - 1) {
    *pcVar38 = *pcVar32;
    pcVar32 = pcVar32 + 1;
    pcVar38 = pcVar38 + 1;
  }
  uVar29 = 0xffffffff;
  pcVar32 = s__msh_1006bfb4;
  do {
    pcVar38 = pcVar32;
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    pcVar38 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar38;
  } while (cVar3 != '\0');
  uVar29 = ~uVar29;
  iVar28 = -1;
  pcVar32 = (char *)fStack_1e0.TopBackLeft.y;
  do {
    pcVar35 = pcVar32;
    if (iVar28 == 0) break;
    iVar28 = iVar28 + -1;
    pcVar35 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar35;
  } while (cVar3 != '\0');
  pcVar32 = pcVar38 + -uVar29;
  pcVar38 = pcVar35 + -1;
  for (uVar30 = uVar29 >> 2; uVar30 != 0; uVar30 = uVar30 - 1) {
    *(undefined4 *)pcVar38 = *(undefined4 *)pcVar32;
    pcVar32 = pcVar32 + 4;
    pcVar38 = pcVar38 + 4;
  }
  for (uVar29 = uVar29 & 3; uVar29 != 0; uVar29 = uVar29 - 1) {
    *pcVar38 = *pcVar32;
    pcVar32 = pcVar32 + 1;
    pcVar38 = pcVar38 + 1;
  }
  uVar29 = 0xffffffff;
  pcVar32 = landscapeBaseName;
  do {
    pcVar38 = pcVar32;
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    pcVar38 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar38;
  } while (cVar3 != '\0');
  uVar29 = ~uVar29;
  pcVar32 = pcVar38 + -uVar29;
  pcVar38 = (char *)fStack_1e0.TopBackLeft.x;
  for (uVar30 = uVar29 >> 2; uVar30 != 0; uVar30 = uVar30 - 1) {
    *(undefined4 *)pcVar38 = *(undefined4 *)pcVar32;
    pcVar32 = pcVar32 + 4;
    pcVar38 = pcVar38 + 4;
  }
  for (uVar29 = uVar29 & 3; uVar29 != 0; uVar29 = uVar29 - 1) {
    *pcVar38 = *pcVar32;
    pcVar32 = pcVar32 + 1;
    pcVar38 = pcVar38 + 1;
  }
  uVar29 = 0xffffffff;
  pcVar32 = s_1_wea_1006bfac;
  do {
    pcVar38 = pcVar32;
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    pcVar38 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar38;
  } while (cVar3 != '\0');
  uVar29 = ~uVar29;
  iVar28 = -1;
  pcVar32 = (char *)fStack_1e0.TopBackLeft.x;
  do {
    pcVar35 = pcVar32;
    if (iVar28 == 0) break;
    iVar28 = iVar28 + -1;
    pcVar35 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar35;
  } while (cVar3 != '\0');
  pcVar32 = pcVar38 + -uVar29;
  pcVar38 = pcVar35 + -1;
  for (uVar30 = uVar29 >> 2; uVar30 != 0; uVar30 = uVar30 - 1) {
    *(undefined4 *)pcVar38 = *(undefined4 *)pcVar32;
    pcVar32 = pcVar32 + 4;
    pcVar38 = pcVar38 + 4;
  }
  for (uVar29 = uVar29 & 3; uVar29 != 0; uVar29 = uVar29 - 1) {
    *pcVar38 = *pcVar32;
    pcVar32 = pcVar32 + 1;
    pcVar38 = pcVar38 + 1;
  }
  uVar29 = 0xffffffff;
  pcVar32 = landscapeBaseName;
  do {
    pcVar38 = pcVar32;
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    pcVar38 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar38;
  } while (cVar3 != '\0');
  uVar29 = ~uVar29;
  pcVar32 = pcVar38 + -uVar29;
  pcVar38 = mem;
  for (uVar30 = uVar29 >> 2; uVar30 != 0; uVar30 = uVar30 - 1) {
    *(undefined4 *)pcVar38 = *(undefined4 *)pcVar32;
    pcVar32 = pcVar32 + 4;
    pcVar38 = pcVar38 + 4;
  }
  for (uVar29 = uVar29 & 3; uVar29 != 0; uVar29 = uVar29 - 1) {
    *pcVar38 = *pcVar32;
    pcVar32 = pcVar32 + 1;
    pcVar38 = pcVar38 + 1;
  }
  uVar29 = 0xffffffff;
  pcVar32 = s_2_wea_1006bfa4;
  do {
    pcVar38 = pcVar32;
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    pcVar38 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar38;
  } while (cVar3 != '\0');
  uVar29 = ~uVar29;
  iVar28 = -1;
  pcVar32 = mem;
  do {
    pcVar35 = pcVar32;
    if (iVar28 == 0) break;
    iVar28 = iVar28 + -1;
    pcVar35 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar35;
  } while (cVar3 != '\0');
  pcVar32 = pcVar38 + -uVar29;
  pcVar38 = pcVar35 + -1;
  for (uVar30 = uVar29 >> 2; uVar30 != 0; uVar30 = uVar30 - 1) {
    *(undefined4 *)pcVar38 = *(undefined4 *)pcVar32;
    pcVar32 = pcVar32 + 4;
    pcVar38 = pcVar38 + 4;
  }
  for (uVar29 = uVar29 & 3; uVar29 != 0; uVar29 = uVar29 - 1) {
    *pcVar38 = *pcVar32;
    pcVar32 = pcVar32 + 1;
    pcVar38 = pcVar38 + 1;
  }
  uVar29 = 0xffffffff;
  do {
    pcVar32 = landscapeBaseName;
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    pcVar32 = landscapeBaseName + 1;
    cVar3 = *landscapeBaseName;
    landscapeBaseName = pcVar32;
  } while (cVar3 != '\0');
  uVar29 = ~uVar29;
  pcVar32 = pcVar32 + -uVar29;
  puVar36 = mem_00;
  for (uVar30 = uVar29 >> 2; uVar30 != 0; uVar30 = uVar30 - 1) {
    *(undefined4 *)puVar36 = *(undefined4 *)pcVar32;
    pcVar32 = pcVar32 + 4;
    puVar36 = puVar36 + 2;
  }
  for (uVar29 = uVar29 & 3; uVar29 != 0; uVar29 = uVar29 - 1) {
    *(char *)puVar36 = *pcVar32;
    pcVar32 = pcVar32 + 1;
    puVar36 = (ushort *)((int)puVar36 + 1);
  }
  uVar29 = 0xffffffff;
  pcVar32 = s__map_1006bf9c;
  do {
    pcVar38 = pcVar32;
    if (uVar29 == 0) break;
    uVar29 = uVar29 - 1;
    pcVar38 = pcVar32 + 1;
    cVar3 = *pcVar32;
    pcVar32 = pcVar38;
  } while (cVar3 != '\0');
  uVar29 = ~uVar29;
  iVar28 = -1;
  puVar36 = mem_00;
  do {
    puVar37 = puVar36;
    if (iVar28 == 0) break;
    iVar28 = iVar28 + -1;
    puVar37 = (ushort *)((int)puVar36 + 1);
    uVar7 = *puVar36;
    puVar36 = puVar37;
  } while ((char)uVar7 != '\0');
  pcVar32 = pcVar38 + -uVar29;
  pcVar38 = (char *)((int)puVar37 + -1);
  for (uVar30 = uVar29 >> 2; uVar30 != 0; uVar30 = uVar30 - 1) {
    *(undefined4 *)pcVar38 = *(undefined4 *)pcVar32;
    pcVar32 = pcVar32 + 4;
    pcVar38 = pcVar38 + 4;
  }
  for (uVar29 = uVar29 & 3; uVar29 != 0; uVar29 = uVar29 - 1) {
    *pcVar38 = *pcVar32;
    pcVar32 = pcVar32 + 1;
    pcVar38 = pcVar38 + 1;
  }
  ppIVar10 = niGet3DRender();
  this->I3DRender__ = ppIVar10;
  if (ppIVar10 == (I3DRender **)0x0) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_3D_Render_was_not_initialized_1006bf7c);
  }
  ppIVar11 = niGet3DSound();
  this->I3DSound = ppIVar11;
  if (ppIVar11 == (I3DSound **)0x0) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_3D_Sound_was_not_initialized_1006bf40);
  }
  pCVar1 = &this->game_object;
  ppIVar12 = (ISoundPool **)
             (*(*this->I3DSound)->CreateSoundPool)
                       ((int *)this->I3DSound,0,0,-(uint)(this != (CLandscape *)0x0) & (uint)pCVar1)
  ;
  this->ISoundPool = ppIVar12;
  if (this->I3DRender__ != 0) {
    ppIVar13 = LoadMatManager(materialConfigName,(LPCSTR)fStack_1e0.TopBackLeft.x,
                              (IComponent **)(-(uint)(this != (CLandscape *)0x0) & (uint)pCVar1));
    this->IMatManager = ppIVar13;
    uVar14 = (*(code *)(*ppIVar13)->LoadWear)(ppIVar13,materialConfigName,mem);
    this->wear_ptr = uVar14;
  }
  ppIVar15 = CreateLightManager(0,(undefined4 *)0x0,
                                (IComponent **)(-(uint)(this != (CLandscape *)0x0) & (uint)pCVar1));
  this->ILightManager_owned = ppIVar15;
  ppIVar16 = CreateCollManager((IComponent **)(-(uint)(this != (CLandscape *)0x0) & (uint)pCVar1));
  BVar17 = (*(*ppIVar16)->QueryInterface)(ppIVar16,ICollManager,&this->ResolvedICollManager);
  if (BVar17 == 0) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_obtain_ICollManager_in_1006bf18);
  }
  ppIVar16 = CreateFxManager(0,0,(IComponent **)(-(uint)(this != (CLandscape *)0x0) & (uint)pCVar1))
  ;
  BVar17 = (*(*ppIVar16)->QueryInterface)(ppIVar16,IEffectManager,&this->ResolvedIEffectManager);
  if (BVar17 == 0) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_obtain_IEffectManager_i_1006beec);
  }
  this_00 = CreateSystemArealMap
                      (0,mem_00,(void *)(-(uint)(this != (CLandscape *)0x0) & (uint)pCVar1));
  BVar17 = (*(this_00->ISystemArealMap->IComponent).QueryInterface)
                     (this_00,ISystemArealMap,&this->ResolvedISystemArealMap);
  if (BVar17 == 0) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_obtain_ISystemArealMap_1006bec0);
  }
  ppIVar18 = niOpenResFile((LPCSTR)fStack_1e0.TopBackLeft.y);
  this->INResFile = ppIVar18;
  if (ppIVar18 == (INResFile **)0x0) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_open_library_1006bea8);
  }
  pnVar19 = (*(*this->INResFile)->get_metadata_ptr)(this->INResFile);
  this->_count_unk_obj_array_elemsize_212 = 1;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,3);
  if (iVar28 == -1) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_open_file_1006be94);
  }
  this->vertices_count = pnVar19[iVar28].element_count_or_version;
                    /* сначала читается тип файла, потом страйд */
  pVVar20 = (Vector3 *)(*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,1);
  this->_3_vertices_data_ptr = pVVar20;
  this->_3_vertices_stride = 0xc;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,4);
  if (iVar28 == -1) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_open_file_1006be94);
  }
  puVar9 = (*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,1);
  this->_4_data_ptr = puVar9;
  this->_4_stride = 4;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,5);
  if (iVar28 == -1) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_open_file_1006be94);
  }
  puVar9 = (*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,1);
  this->_5_data_ptr = puVar9;
  this->_5_stride = 4;
  this->_2_microtexture_mapping_data_ptr = 0;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,0x12);
  if (iVar28 == -1) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_find_microtexture_mapp_1006be68);
  }
  puVar9 = (*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,1);
  this->_2_microtexture_mapping_data_ptr = puVar9;
  this->_2_microtexture_mapping_stride = 4;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,0xe);
  if (iVar28 == -1) {
    this->_e_data_ptr = 0;
  }
  else {
    puVar9 = (*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,1);
    this->_e_data_ptr = puVar9;
    this->_e_stride = 4;
  }
  this->field138_0x2a4 = 0;
  this->field143_0x2ac = 0;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,0x15);
  if (iVar28 == -1) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_open_file_1006be94);
  }
  this->_5_element_count = pnVar19[iVar28].element_count_or_version;
  pMVar21 = (MSH_15_element *)
            (*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,1);
  this->_5_data_ptr = pMVar21;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,2);
  if (iVar28 == -1) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_open_file_1006be94);
  }
  pMVar22 = (MSH_02_file *)
            (*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,0);
  this->msh_02_data_ptr = pMVar22;
  pMVar23 = (MshMetaForLandscape *)operator_new(0x28);
  this->mshmeta_ptr = pMVar23;
  pMVar22 = this->msh_02_data_ptr;
  pBVar39 = &this->bounding_box_from_msh_02;
  for (iVar28 = 0x18; iVar28 != 0; iVar28 = iVar28 + -1) {
    (pBVar39->BottomFrontLeft).x = (pMVar22->header).bounding_box.BottomFrontLeft.x;
    pMVar22 = (MSH_02_file *)&(pMVar22->header).bounding_box.BottomFrontLeft.y;
    pBVar39 = (BoundingBox *)&(pBVar39->BottomFrontLeft).y;
  }
  this->mshmeta_ptr->min_x = (this->msh_02_data_ptr->header).bounding_box.BottomFrontLeft.x;
  this->mshmeta_ptr->min_y = (this->msh_02_data_ptr->header).bounding_box.BottomFrontLeft.y;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,1);
  if (iVar28 == -1) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_open_file_1006be94);
  }
  puVar9 = (*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,0);
  this->_1_data_ptr = puVar9;
  this->mshmeta_ptr->_1_element_count = pnVar19[iVar28].element_count_or_version;
  this->mshmeta_ptr->_1_magic1 = pnVar19[iVar28].magic1;
  pMVar23 = this->mshmeta_ptr;
  pMVar23->_1_element_count_magic1 = pMVar23->_1_element_count / pMVar23->_1_magic1;
  pMVar22 = this->msh_02_data_ptr;
  pVVar33 = &pMVar22->elements[0].local_minimum;
  pVVar20 = Vector3::ctor(&VStack_148,pVVar33->x,pMVar22->elements[0].local_minimum.y,
                          pMVar22->elements[0].local_minimum.z);
  fVar34 = pVVar20->x;
  fVar4 = pVVar20->y;
  Vector3::ctor(&VStack_13c,pMVar22->elements[0].local_maximum.x,
                pMVar22->elements[0].local_minimum.y,pMVar22->elements[0].local_minimum.z);
  Vector3::ctor(&VStack_154,pMVar22->elements[0].local_maximum.x,
                pMVar22->elements[0].local_maximum.y,pMVar22->elements[0].local_minimum.z);
  pVVar20 = Vector3::ctor(&VStack_16c,pMVar22->elements[0].local_maximum.x,
                          pMVar22->elements[0].local_maximum.y,pMVar22->elements[0].local_maximum.z)
  ;
  fVar5 = pVVar20->x;
  fVar6 = pVVar20->y;
  Vector3::ctor((Vector3 *)&fStack_1e0.TopFrontLeft.z,pVVar33->x,
                pMVar22->elements[0].local_maximum.y,pMVar22->elements[0].local_maximum.z);
  Vector3::ctor(&VStack_160,pVVar33->x,pMVar22->elements[0].local_minimum.y,
                pMVar22->elements[0].local_maximum.z);
  Vector3::ctor((Vector3 *)&fStack_1e0.TopBackLeft.z,pVVar33->x,pMVar22->elements[0].local_maximum.y
                ,pMVar22->elements[0].local_minimum.z);
  Vector3::ctor(&VStack_178,pMVar22->elements[0].local_maximum.x,
                pMVar22->elements[0].local_minimum.y,pMVar22->elements[0].local_maximum.z);
  this->mshmeta_ptr->width = fVar5 - fVar34;
  this->mshmeta_ptr->height = fVar6 - fVar4;
  this->mshmeta_ptr->inv_width = FLOAT_1006516c / this->mshmeta_ptr->width;
  this->mshmeta_ptr->inv_height = FLOAT_1006516c / this->mshmeta_ptr->height;
  iVar28 = (*(*this->INResFile)->find_index_of_type)(this->INResFile,0xb);
  if (iVar28 == -1) {
                    /* WARNING: Subroutine does not return */
    print_error_and_die(s_TERRAIN_DLL_1006b3f4,s_CLandscape__CLandscape___1006bf60,
                        s_Unable_to_open_file_1006be94);
  }
  puVar9 = (*(*this->INResFile)->get_item_data_ptr_by_index)(this->INResFile,iVar28,1);
  this->_b_data_ptr = puVar9;
  operator_delete((void *)fStack_1e0.TopBackLeft.y);
  operator_delete((void *)fStack_1e0.TopBackLeft.x);
  operator_delete(mem);
  operator_delete(mem_00);
  (this->m_CollisionContext).extraArg = (void **)0xbf800000;
  uVar29 = 0;
  if (this->_5_element_count != 0) {
    pMVar21 = this->_5_data_ptr;
    do {
      if ((pMVar21->flags & 0x20000) != 0) {
        (this->m_CollisionContext).extraArg =
             *(void ***)
              ((int)&this->_3_vertices_data_ptr->z +
              (uint)pMVar21->vertex1_idx * this->_3_vertices_stride);
        break;
      }
      uVar29 = uVar29 + 1;
      pMVar21 = pMVar21 + 1;
    } while (uVar29 < this->_5_element_count);
  }
  pfVar35 = &g_bounding_box;
  pBVar39 = pBVar2;
  for (iVar28 = 0x18; iVar28 != 0; iVar28 = iVar28 + -1) {
    (pBVar39->BottomFrontLeft).x = (pfVar35->BottomFrontLeft).x;
    pfVar35 = (BoundingBox *)&(pfVar35->BottomFrontLeft).y;
    pBVar39 = (BoundingBox *)&(pBVar39->BottomFrontLeft).y;
  }
  uStack_1ec = 0;
  if (this->_5_element_count != 0) {
    iStack_1e4 = 0;
    do {
      puVar20 = (MSH_15_element *)((int)&this->_5_data_ptr->flags + iStack_1e4);
      if ((puVar20->flags & 0x20000) != 0) {
        iVar28 = this->_3_vertices_stride;
        pfVar29 = (Vector3 *)
                  ((int)&this->_3_vertices_data_ptr->x + (uint)puVar20->vertex1_idx * iVar28);
        pfVar34 = (Vector3 *)
                  ((int)&this->_3_vertices_data_ptr->x + (uint)puVar20->vertex2_idx * iVar28);
        pfVar40 = (Vector3 *)
                  ((int)&this->_3_vertices_data_ptr->x + (uint)puVar20->vertex3_idx * iVar28);
        fStack_1e0.BottomFrontLeft.x = (pBVar2->BottomFrontLeft).x;
        fStack_1e0.BottomFrontLeft.y = (this->bounding_box).BottomFrontLeft.y;
        fStack_1e0.BottomFrontLeft.z = (this->bounding_box).BottomFrontLeft.z;
        fStack_1e0.BottomBackRight.x = (this->bounding_box).TopFrontLeft.x;
        fStack_1e0.BottomBackRight.y = (this->bounding_box).TopFrontLeft.y;
        fStack_1e0.BottomBackRight.z = (this->bounding_box).TopFrontLeft.z;
        if (pfVar29->x < fStack_1e0.BottomFrontLeft.x) {
          fStack_1e0.BottomFrontLeft.x = pfVar29->x;
        }
        if (pfVar29->y < fStack_1e0.BottomFrontLeft.y) {
          fStack_1e0.BottomFrontLeft.y = pfVar29->y;
        }
        if (pfVar29->z < fStack_1e0.BottomFrontLeft.z) {
          fStack_1e0.BottomFrontLeft.z = pfVar29->z;
        }
        if (fStack_1e0.BottomBackRight.x < pfVar29->x) {
          fStack_1e0.BottomBackRight.x = pfVar29->x;
        }
        if (fStack_1e0.BottomBackRight.y < pfVar29->y) {
          fStack_1e0.BottomBackRight.y = pfVar29->y;
        }
        if (fStack_1e0.BottomBackRight.z < pfVar29->z) {
          fStack_1e0.BottomBackRight.z = pfVar29->z;
        }
        fStack_10c.x = fStack_1e0.BottomBackRight.x;
        fStack_10c.y = fStack_1e0.BottomBackRight.y;
        fStack_10c.z = fStack_1e0.BottomFrontLeft.z;
        (pBVar2->BottomFrontLeft).x = fStack_1e0.BottomFrontLeft.x;
        (this->bounding_box).BottomFrontLeft.y = fStack_1e0.BottomFrontLeft.y;
        (this->bounding_box).BottomFrontLeft.z = fStack_1e0.BottomFrontLeft.z;
        (this->bounding_box).BottomFrontRight.x = fStack_1e0.BottomBackRight.x;
        (this->bounding_box).BottomFrontRight.y = fStack_1e0.BottomFrontLeft.y;
        (this->bounding_box).BottomFrontRight.z = fStack_1e0.BottomFrontLeft.z;
        (this->bounding_box).BottomBackRight.x = fStack_1e0.BottomBackRight.x;
        (this->bounding_box).BottomBackRight.y = fStack_1e0.BottomBackRight.y;
        fStack_124 = fStack_1e0.BottomFrontLeft.x;
        (this->bounding_box).BottomBackRight.z = fStack_1e0.BottomFrontLeft.z;
        (this->bounding_box).TopFrontLeft.x = fStack_1e0.BottomBackRight.x;
        fStack_120 = fStack_1e0.BottomBackRight.y;
        (this->bounding_box).TopFrontLeft.y = fStack_1e0.BottomBackRight.y;
        (this->bounding_box).TopFrontLeft.z = fStack_1e0.BottomBackRight.z;
        (this->bounding_box).TopBackRight.x = fStack_1e0.BottomFrontLeft.x;
        fStack_11c = fStack_1e0.BottomBackRight.z;
        (this->bounding_box).TopBackRight.y = fStack_1e0.BottomBackRight.y;
        (this->bounding_box).TopBackRight.z = fStack_1e0.BottomBackRight.z;
        fStack_118 = fStack_1e0.BottomFrontLeft.x;
        fStack_114 = fStack_1e0.BottomFrontLeft.y;
        (this->bounding_box).TopFrontRight.x = fStack_1e0.BottomFrontLeft.x;
        fStack_110 = fStack_1e0.BottomBackRight.z;
        (this->bounding_box).TopFrontRight.y = fStack_1e0.BottomFrontLeft.y;
        (this->bounding_box).TopFrontRight.z = fStack_1e0.BottomBackRight.z;
        pVVar20 = Vector3::ctor((Vector3 *)&exception2,fStack_1e0.BottomFrontLeft.x,
                                fStack_1e0.BottomBackRight.y,fStack_1e0.BottomFrontLeft.z);
        (this->bounding_box).BottomBackLeft.x = pVVar20->x;
        (this->bounding_box).BottomBackLeft.y = pVVar20->y;
        (this->bounding_box).BottomBackLeft.z = pVVar20->z;
        pVVar20 = Vector3::ctor(&VStack_78,fStack_1e0.BottomBackRight.x,fStack_1e0.BottomFrontLeft.y
                                ,fStack_1e0.BottomBackRight.z);
        (this->bounding_box).TopBackLeft.x = pVVar20->x;
        (this->bounding_box).TopBackLeft.y = pVVar20->y;
        (this->bounding_box).TopBackLeft.z = pVVar20->z;
        fStack_1e0.BottomFrontRight.x = (pBVar2->BottomFrontLeft).x;
        fStack_1e0.BottomFrontRight.y = (this->bounding_box).BottomFrontLeft.y;
        fStack_1e0.BottomFrontRight.z = (this->bounding_box).BottomFrontLeft.z;
        pVVar20 = &(this->bounding_box).TopFrontLeft;
        fStack_1e0.BottomBackLeft.x = pVVar20->x;
        fStack_1e0.BottomBackLeft.y = (this->bounding_box).TopFrontLeft.y;
        fStack_1e0.BottomBackLeft.z = (this->bounding_box).TopFrontLeft.z;
        if (pfVar34->x < fStack_1e0.BottomFrontRight.x) {
          fStack_1e0.BottomFrontRight.x = pfVar34->x;
        }
        if (pfVar34->y < fStack_1e0.BottomFrontRight.y) {
          fStack_1e0.BottomFrontRight.y = pfVar34->y;
        }
        if (pfVar34->z < fStack_1e0.BottomFrontRight.z) {
          fStack_1e0.BottomFrontRight.z = pfVar34->z;
        }
        if (fStack_1e0.BottomBackLeft.x < pfVar34->x) {
          fStack_1e0.BottomBackLeft.x = pfVar34->x;
        }
        if (fStack_1e0.BottomBackLeft.y < pfVar34->y) {
          fStack_1e0.BottomBackLeft.y = pfVar34->y;
        }
        if (fStack_1e0.BottomBackLeft.z < pfVar34->z) {
          fStack_1e0.BottomBackLeft.z = pfVar34->z;
        }
        fVar34 = fStack_1e0.BottomBackLeft.z;
        fStack_130 = fStack_1e0.BottomFrontRight.x;
        fStack_12c = fStack_1e0.BottomFrontRight.y;
        fStack_128 = fStack_1e0.BottomFrontRight.z;
        VStack_178.x = fStack_1e0.BottomBackLeft.x;
        VStack_178.y = fStack_1e0.BottomFrontRight.y;
        VStack_178.z = fStack_1e0.BottomFrontRight.z;
        fStack_1e0.TopBackLeft.z = fStack_1e0.BottomBackLeft.x;
        (pBVar2->BottomFrontLeft).x = fStack_1e0.BottomFrontRight.x;
        fStack_1e0.TopFrontLeft.x = fStack_1e0.BottomBackLeft.y;
        (this->bounding_box).BottomFrontLeft.y = fStack_1e0.BottomFrontRight.y;
        fStack_1e0.TopFrontLeft.y = fStack_1e0.BottomFrontRight.z;
        (this->bounding_box).BottomFrontLeft.z = fStack_1e0.BottomFrontRight.z;
        VStack_160.x = fStack_1e0.BottomBackLeft.x;
        (this->bounding_box).BottomFrontRight.x = fStack_1e0.BottomBackLeft.x;
        VStack_160.y = fStack_1e0.BottomBackLeft.y;
        (this->bounding_box).BottomFrontRight.y = fStack_1e0.BottomFrontRight.y;
        (this->bounding_box).BottomFrontRight.z = fStack_1e0.BottomFrontRight.z;
        VStack_160.z = fStack_1e0.BottomBackLeft.z;
        (this->bounding_box).BottomBackRight.x = fStack_1e0.BottomBackLeft.x;
        fStack_1e0.TopFrontLeft.z = fStack_1e0.BottomFrontRight.x;
        (this->bounding_box).BottomBackRight.y = fStack_1e0.BottomBackLeft.y;
        (this->bounding_box).BottomBackRight.z = fStack_1e0.BottomFrontRight.z;
        pVVar20->x = fStack_1e0.BottomBackLeft.x;
        (this->bounding_box).TopFrontLeft.y = fStack_1e0.BottomBackLeft.y;
        (this->bounding_box).TopFrontLeft.z = fStack_1e0.BottomBackLeft.z;
        (this->bounding_box).TopBackRight.x = fStack_1e0.BottomFrontRight.x;
        (this->bounding_box).TopBackRight.y = fStack_1e0.BottomBackLeft.y;
        (this->bounding_box).TopBackRight.z = fStack_1e0.BottomBackLeft.z;
        pVVar20 = Vector3::ctor(&VStack_3c,fStack_1e0.BottomFrontRight.x,
                                fStack_1e0.BottomFrontRight.y,fStack_1e0.BottomBackLeft.z);
        (this->bounding_box).TopFrontRight.x = pVVar20->x;
        (this->bounding_box).TopFrontRight.y = pVVar20->y;
        (this->bounding_box).TopFrontRight.z = pVVar20->z;
        pVVar20 = Vector3::ctor(&VStack_54,fStack_1e0.BottomFrontRight.x,fStack_1e0.BottomBackLeft.y
                                ,fStack_1e0.BottomFrontRight.z);
        (this->bounding_box).BottomBackLeft.x = pVVar20->x;
        (this->bounding_box).BottomBackLeft.y = pVVar20->y;
        (this->bounding_box).BottomBackLeft.z = pVVar20->z;
        pVVar20 = Vector3::ctor(&VStack_24,fStack_1e0.BottomBackLeft.x,fStack_1e0.BottomFrontRight.y
                                ,fVar34);
        (this->bounding_box).TopBackLeft.x = pVVar20->x;
        (this->bounding_box).TopBackLeft.y = pVVar20->y;
        (this->bounding_box).TopBackLeft.z = pVVar20->z;
        fStack_1e0.TopBackRight.x = (pBVar2->BottomFrontLeft).x;
        fStack_1e0.TopBackRight.y = (this->bounding_box).BottomFrontLeft.y;
        fStack_1e0.TopBackRight.z = (this->bounding_box).BottomFrontLeft.z;
        pVVar20 = &(this->bounding_box).TopFrontLeft;
        fStack_1e0.TopFrontRight.x = pVVar20->x;
        fStack_1e0.TopFrontRight.y = (this->bounding_box).TopFrontLeft.y;
        fVar34 = (this->bounding_box).TopFrontLeft.z;
        if (pfVar40->x < fStack_1e0.TopBackRight.x) {
          fStack_1e0.TopBackRight.x = pfVar40->x;
        }
        fVar4 = fStack_1e0.TopBackRight.x;
        if (pfVar40->y < fStack_1e0.TopBackRight.y) {
          fStack_1e0.TopBackRight.y = pfVar40->y;
        }
        if (pfVar40->z < fStack_1e0.TopBackRight.z) {
          fStack_1e0.TopBackRight.z = pfVar40->z;
        }
        if (fStack_1e0.TopFrontRight.x < pfVar40->x) {
          fStack_1e0.TopFrontRight.x = pfVar40->x;
        }
        if (fStack_1e0.TopFrontRight.y < pfVar40->y) {
          fStack_1e0.TopFrontRight.y = pfVar40->y;
        }
        if (fVar34 < pfVar40->z) {
          fVar34 = pfVar40->z;
        }
        VStack_16c.x = fStack_1e0.TopBackRight.x;
        VStack_16c.y = fStack_1e0.TopBackRight.y;
        VStack_16c.z = fStack_1e0.TopBackRight.z;
        VStack_154.x = fStack_1e0.TopFrontRight.x;
        VStack_154.y = fStack_1e0.TopBackRight.y;
        VStack_154.z = fStack_1e0.TopBackRight.z;
        (pBVar2->BottomFrontLeft).x = fStack_1e0.TopBackRight.x;
        VStack_13c.x = fStack_1e0.TopFrontRight.x;
        (this->bounding_box).BottomFrontLeft.y = fStack_1e0.TopBackRight.y;
        VStack_13c.y = fStack_1e0.TopFrontRight.y;
        (this->bounding_box).BottomFrontLeft.z = fStack_1e0.TopBackRight.z;
        VStack_13c.z = fStack_1e0.TopBackRight.z;
        (this->bounding_box).BottomFrontRight.x = fStack_1e0.TopFrontRight.x;
        VStack_148.x = fStack_1e0.TopFrontRight.x;
        (this->bounding_box).BottomFrontRight.y = fStack_1e0.TopBackRight.y;
        (this->bounding_box).BottomFrontRight.z = fStack_1e0.TopBackRight.z;
        VStack_148.y = fStack_1e0.TopFrontRight.y;
        (this->bounding_box).BottomBackRight.x = fStack_1e0.TopFrontRight.x;
        (this->bounding_box).BottomBackRight.y = fStack_1e0.TopFrontRight.y;
        (this->bounding_box).BottomBackRight.z = fStack_1e0.TopBackRight.z;
        pVVar20->x = fStack_1e0.TopFrontRight.x;
        (this->bounding_box).TopFrontLeft.y = fStack_1e0.TopFrontRight.y;
        (this->bounding_box).TopFrontLeft.z = fVar34;
        fStack_1e0.TopFrontRight.z = fVar34;
        VStack_148.z = fVar34;
        pVVar20 = Vector3::ctor(&VStack_6c,fStack_1e0.TopBackRight.x,fStack_1e0.TopFrontRight.y,
                                fVar34);
        (this->bounding_box).TopBackRight.x = pVVar20->x;
        (this->bounding_box).TopBackRight.y = pVVar20->y;
        (this->bounding_box).TopBackRight.z = pVVar20->z;
        pVVar20 = Vector3::ctor(&VStack_60,fVar4,fStack_1e0.TopBackRight.y,fVar34);
        (this->bounding_box).TopFrontRight.x = pVVar20->x;
        (this->bounding_box).TopFrontRight.y = pVVar20->y;
        (this->bounding_box).TopFrontRight.z = pVVar20->z;
        pVVar20 = Vector3::ctor(&VStack_48,fVar4,fStack_1e0.TopFrontRight.y,
                                fStack_1e0.TopBackRight.z);
        (this->bounding_box).BottomBackLeft.x = pVVar20->x;
        (this->bounding_box).BottomBackLeft.y = pVVar20->y;
        (this->bounding_box).BottomBackLeft.z = pVVar20->z;
        pVVar20 = Vector3::ctor(&VStack_30,fStack_1e0.TopFrontRight.x,fStack_1e0.TopBackRight.y,
                                fVar34);
        (this->bounding_box).TopBackLeft.x = pVVar20->x;
        (this->bounding_box).TopBackLeft.y = pVVar20->y;
        (this->bounding_box).TopBackLeft.z = pVVar20->z;
      }
      uStack_1ec = uStack_1ec + 1;
      iStack_1e4 = iStack_1e4 + 0x1c;
    } while (uStack_1ec < this->_5_element_count);
  }
  iVar28 = this->mshmeta_ptr->_1_element_count;
  piVar24 = (int *)operator_new(iVar28 * 0xc + 4);
  exception.Offset = CONCAT31(exception.Offset._1_3_,7);
  if (piVar24 == (int *)0x0) {
    piVar25 = (int *)0x0;
  }
  else {
    piVar25 = piVar24 + 1;
    *piVar24 = iVar28;
    vector_constructor_iterator(piVar25,0xc,iVar28,KindaArray::ctor_capacity5_elemsize8);
  }
  this->field9815_0x7b64 = piVar25;
  exception.Offset = CONCAT31(exception.Offset._1_3_,6);
  iVar28 = this->mshmeta_ptr->_1_element_count;
  piVar24 = (int *)operator_new(iVar28 * 0xc + 4);
  exception.Offset = CONCAT31(exception.Offset._1_3_,8);
  if (piVar24 == (int *)0x0) {
    piVar25 = (int *)0x0;
  }
  else {
    piVar25 = piVar24 + 1;
    *piVar24 = iVar28;
    vector_constructor_iterator(piVar25,0xc,iVar28,KindaArray::ctor_capacity100_elemsize4);
  }
  this->field9818_0x7b70 = piVar25;
  exception.Offset = CONCAT31(exception.Offset._1_3_,6);
  iVar28 = this->mshmeta_ptr->_1_element_count;
  piVar24 = (int *)operator_new(iVar28 * 0xc + 4);
  exception.Offset = CONCAT31(exception.Offset._1_3_,9);
  if (piVar24 == (int *)0x0) {
    piVar25 = (int *)0x0;
  }
  else {
    piVar25 = piVar24 + 1;
    *piVar24 = iVar28;
    vector_constructor_iterator(piVar25,0xc,iVar28,KindaArray::ctor_capacity5_elemsize4);
  }
  this->field9816_0x7b68 = piVar25;
  this->field62_0x194 = &this->field9784_0x7b3c;
  this->field95_0x1c4 = this->wear_ptr;
  this->field63_0x198 = &this->field9793_0x7b48;
  this->field22_0x160 = &this->field_0x7c54;
  this->field23_0x164 = &this->field_0x7c5c;
  this->field64_0x19c = &this->field_0x7b54;
  this->field96_0x1c8 = &this->field_0x7c6c;
  this->field24_0x168 = &this->field_0x7c64;
  this->field97_0x1cc = &this->field9885_0x7c74;
  this->field65_0x1a0 = -1;
  this->field70_0x1a8 = 0;
  this->field98_0x1d0 = &this->field9890_0x7c7c;
  this->field29_0x170 = &this->field_0x7c48;
  this->field101_0x1dc = -1;
  this->field103_0x1e4 = -1;
  this->field107_0x1f4 = 0;
  this->field104_0x1e8 = 0;
  this->field105_0x1ec = -2;
  this->field106_0x1f0 = 0;
  this->field108_0x1f8 = 0;
  this->field109_0x1fc = 0;
  this->field110_0x200 = 0;
  this->field111_0x204 = 0;
  uVar29 = this->_5_element_count;
  exception.Offset = CONCAT31(exception.Offset._1_3_,6);
  if ((this->array_0x278).data == (undefined4 *)0x0) {
    puVar9 = (undefined4 *)operator_new(8000);
    (this->array_0x278).data = puVar9;
  }
  iVar28 = uVar29 + (this->array_0x278).count;
  if ((this->array_0x278).capacity < iVar28) {
    puVar26 = (undefined4 *)operator_new(iVar28 * 8 + 800);
    puVar9 = (this->array_0x278).data;
    puVar40 = puVar26;
    for (uVar30 = (uint)((this->array_0x278).capacity << 3) >> 2; uVar30 != 0; uVar30 = uVar30 - 1)
    {
      *puVar40 = *puVar9;
      puVar9 = puVar9 + 1;
      puVar40 = puVar40 + 1;
    }
    for (iVar28 = 0; iVar28 != 0; iVar28 = iVar28 + -1) {
      *(undefined1 *)puVar40 = *(undefined1 *)puVar9;
      puVar9 = (undefined4 *)((int)puVar9 + 1);
      puVar40 = (undefined4 *)((int)puVar40 + 1);
    }
    operator_delete((this->array_0x278).data);
    iVar28 = (this->array_0x278).count + uVar29;
    (this->array_0x278).data = puVar26;
    (this->array_0x278).capacity = iVar28 + 100;
  }
  (this->array_0x278).count = iVar28;
  uVar29 = 0;
  if (this->_5_element_count != 0) {
    iVar28 = 0;
    do {
      iVar31 = (int)&this->_5_data_ptr->flags + iVar28;
      iVar28 = iVar28 + 0x1c;
      (this->array_0x278).data[uVar29 * 2] = iVar31;
      *(undefined1 *)((this->array_0x278).data + uVar29 * 2 + 1) = 0;
      uVar29 = uVar29 + 1;
    } while (uVar29 < this->_5_element_count);
  }
  pbVar41 = this->unk_obj_array_elemsize_212[99] + 0x60;
  for (iVar28 = 0x19; iVar28 != 0; iVar28 = iVar28 + -1) {
    pbVar41[0] = 0;
    pbVar41[1] = 0;
    pbVar41[2] = 0;
    pbVar41[3] = 0;
    pbVar41 = pbVar41 + 4;
  }
  this->field9896_0x7c90 = (undefined4 *)0x0;
  pCVar27 = GetShade();
  this->Shade = pCVar27;
  this->field9833_0x7c20 = 0;
  this->flags_of_mesh_part = 0x10;
  this->_CheckMaxBasementAngleStep = 2;
  ExceptionList = exception.ExceptionList;
  return this;
}



---

// Assembly:
1000eba0 PUSH -0x1
1000eba2 PUSH 0x1006320e
1000eba7 MOV EAX,FS:[0x0]
1000ebad PUSH EAX
1000ebae MOV dword ptr FS:[0x0],ESP
1000ebb5 SUB ESP,0x1e0
1000ebbb MOV EAX,dword ptr [ESP + 0x1fc]
1000ebc2 PUSH EBX
1000ebc3 PUSH EBP
1000ebc4 PUSH ESI
1000ebc5 PUSH EDI
1000ebc6 MOV EDI,dword ptr [ESP + 0x208]
1000ebcd MOV EBP,ECX
1000ebcf PUSH EAX
1000ebd0 PUSH EDI
1000ebd1 MOV dword ptr [ESP + 0x128],EBP
1000ebd8 LEA ECX,[EBP + 0x4]
1000ebdb MOV dword ptr [EBP],0x100655d0
1000ebe2 CALL 0x100565d0
1000ebe7 MOV dword ptr [EBP + 0x134],0x100655a0
1000ebf1 MOV dword ptr [EBP + 0x138],0x10065560
1000ebfb MOV dword ptr [EBP + 0x13c],0x1006552c
1000ec05 XOR ESI,ESI
1000ec07 MOV dword ptr [EBP + 0x140],0x100654d8
1000ec11 MOV EBX,0xfffffffe
1000ec16 MOV dword ptr [EBP + 0x144],0x100654c8
1000ec20 MOV dword ptr [ESP + 0x1f8],ESI
1000ec27 MOV dword ptr [EBP + 0x1e8],ESI
1000ec2d MOV dword ptr [EBP + 0x1ec],EBX
1000ec33 LEA EAX,[EBP + 0x214]
1000ec39 MOV ECX,0x3
1000ec3e OR EDX,0xffffffff
1000ec41 MOV dword ptr [EAX],ESI
1000ec43 MOV dword ptr [EAX + 0x4],EBX
1000ec46 MOV dword ptr [EAX + 0x8],EDX
1000ec49 MOV dword ptr [EAX + 0xc],EDX
1000ec4c MOV dword ptr [EAX + 0x10],EDX
1000ec4f ADD EAX,0x14
1000ec52 DEC ECX
1000ec53 JNZ 0x1000ec41
1000ec55 PUSH 0x1f40
1000ec5a CALL 0x100597e7
1000ec5f ADD ESP,0x4
1000ec62 MOV dword ptr [EBP + 0x278],EAX
1000ec68 MOV dword ptr [EBP + 0x280],0x3e8
1000ec72 MOV dword ptr [EBP + 0x27c],ESI
1000ec78 MOV EBX,0x64
1000ec7d MOV byte ptr [ESP + 0x1f8],0x1
1000ec85 LEA ESI,[EBP + 0x2fc]
1000ec8b MOV dword ptr [ESP + 0x10],EBX
1000ec8f PUSH 0x1000da50
1000ec94 PUSH 0x8
1000ec96 PUSH 0xc
1000ec98 PUSH ESI
1000ec99 CALL 0x10001330
1000ec9e MOV EAX,dword ptr [ESP + 0x10]
1000eca2 ADD ESI,0xd4
1000eca8 DEC EAX
1000eca9 MOV dword ptr [ESP + 0x10],EAX
1000ecad JNZ 0x1000ec8f
1000ecaf PUSH 0x190
1000ecb4 CALL 0x100597e7
1000ecb9 XOR ESI,ESI
1000ecbb MOV dword ptr [EBP + 0x7b74],EAX
1000ecc1 MOV dword ptr [EBP + 0x7b7c],EBX
1000ecc7 MOV dword ptr [EBP + 0x7b78],ESI
1000eccd PUSH 0xfa0
1000ecd2 MOV byte ptr [ESP + 0x200],0x2
1000ecda CALL 0x100597e7
1000ecdf MOV dword ptr [EBP + 0x7c00],EAX
1000ece5 MOV dword ptr [EBP + 0x7c08],0x3e8
1000ecef MOV dword ptr [EBP + 0x7c04],ESI
1000ecf5 PUSH 0x190
1000ecfa MOV byte ptr [ESP + 0x204],0x3
1000ed02 CALL 0x100597e7
1000ed07 MOV dword ptr [EBP + 0x7c0c],EAX
1000ed0d MOV dword ptr [EBP + 0x7c14],EBX
1000ed13 MOV dword ptr [EBP + 0x7c10],ESI
1000ed19 PUSH 0x190
1000ed1e MOV byte ptr [ESP + 0x208],0x4
1000ed26 CALL 0x100597e7
1000ed2b MOV dword ptr [EBP + 0x7c2c],EAX
1000ed31 MOV dword ptr [EBP + 0x7c34],EBX
1000ed37 MOV dword ptr [EBP + 0x7c30],ESI
1000ed3d PUSH 0xa0
1000ed42 MOV byte ptr [ESP + 0x20c],0x5
1000ed4a CALL 0x100597e7
1000ed4f MOV dword ptr [EBP + 0x7c84],EAX
1000ed55 MOV dword ptr [EBP + 0x7c8c],0x28
1000ed5f MOV dword ptr [EBP + 0x7c88],ESI
1000ed65 MOV dword ptr [EBP + 0x7c98],0x1
1000ed6f MOV dword ptr [EBP + 0x7c9c],ESI
1000ed75 MOV dword ptr [EBP + 0x7ca0],ESI
1000ed7b MOV dword ptr [EBP + 0x7ca4],ESI
1000ed81 MOV dword ptr [EBP + 0x7ca8],ESI
1000ed87 MOV dword ptr [EBP + 0x7cac],ESI
1000ed8d MOV dword ptr [EBP + 0x7cb8],ESI
1000ed93 MOV dword ptr [EBP + 0x7cb4],ESI
1000ed99 MOV dword ptr [EBP + 0x7cb0],ESI
1000ed9f MOV dword ptr [EBP + 0x7cc4],ESI
1000eda5 MOV dword ptr [EBP + 0x7cc0],ESI
1000edab MOV dword ptr [EBP + 0x7cbc],ESI
1000edb1 MOV dword ptr [EBP + 0x7cc8],0xbf800000
1000edbb MOV dword ptr [EBP + 0x7ccc],ESI
1000edc1 MOV dword ptr [EBP + 0x7cd0],ESI
1000edc7 MOV dword ptr [EBP + 0x7cd4],ESI
1000edcd MOV dword ptr [EBP + 0x7cd8],ESI
1000edd3 MOV ESI,dword ptr [ESP + 0x218]
1000edda MOV dword ptr [EBP],0x100654c0
1000ede1 MOV dword ptr [EBP + 0x4],0x10065464
1000ede8 MOV dword ptr [EBP + 0x134],0x10065434
1000edf2 MOV dword ptr [EBP + 0x138],0x100653f4
1000edfc MOV dword ptr [EBP + 0x13c],0x100653c0
1000ee06 MOV dword ptr [EBP + 0x140],0x1006536c
1000ee10 MOV dword ptr [EBP + 0x144],0x1006535c
1000ee1a MOV dword ptr [EBP + 0x274],EDI
1000ee20 MOV EDI,ESI
1000ee22 OR ECX,0xffffffff
1000ee25 XOR EAX,EAX
1000ee27 MOV byte ptr [ESP + 0x20c],0x6
1000ee2f SCASB.REPNE ES:EDI
1000ee31 NOT ECX
1000ee33 ADD ECX,0x4
1000ee36 LEA EBX,[EBP + 0x7ce0]
1000ee3c PUSH ECX
1000ee3d CALL 0x100597e7
1000ee42 MOV dword ptr [ESP + 0x80],EAX
1000ee49 MOV EDI,ESI
1000ee4b OR ECX,0xffffffff
1000ee4e XOR EAX,EAX
1000ee50 SCASB.REPNE ES:EDI
1000ee52 NOT ECX
1000ee54 ADD ECX,0x5
1000ee57 PUSH ECX
1000ee58 CALL 0x100597e7
1000ee5d MOV dword ptr [ESP + 0x80],EAX
1000ee64 MOV EDI,ESI
1000ee66 OR ECX,0xffffffff
1000ee69 XOR EAX,EAX
1000ee6b SCASB.REPNE ES:EDI
1000ee6d NOT ECX
1000ee6f ADD ECX,0x5
1000ee72 PUSH ECX
1000ee73 CALL 0x100597e7
1000ee78 MOV dword ptr [ESP + 0x38],EAX
1000ee7c MOV EDI,ESI
1000ee7e OR ECX,0xffffffff
1000ee81 XOR EAX,EAX
1000ee83 SCASB.REPNE ES:EDI
1000ee85 NOT ECX
1000ee87 ADD ECX,0x4
1000ee8a PUSH ECX
1000ee8b CALL 0x100597e7
1000ee90 MOV dword ptr [ESP + 0x34],EAX
1000ee94 OR ECX,0xffffffff
1000ee97 MOV EDI,ESI
1000ee99 XOR EAX,EAX
1000ee9b ADD ESP,0x24
1000ee9e MOV EDX,dword ptr [ESP + 0x68]
1000eea2 SCASB.REPNE ES:EDI
1000eea4 NOT ECX
1000eea6 SUB EDI,ECX
1000eea8 MOV EAX,ECX
1000eeaa MOV ESI,EDI
1000eeac SHR ECX,0x2
1000eeaf MOV EDI,EDX
1000eeb1 MOVSD.REP ES:EDI,ESI
1000eeb3 MOV ECX,EAX
1000eeb5 XOR EAX,EAX
1000eeb7 AND ECX,0x3
1000eeba MOVSB.REP ES:EDI,ESI
1000eebc MOV EDI,0x1006bfb4
1000eec1 OR ECX,0xffffffff
1000eec4 SCASB.REPNE ES:EDI
1000eec6 NOT ECX
1000eec8 SUB EDI,ECX
1000eeca MOV EAX,ECX
1000eecc MOV ESI,EDI
1000eece MOV EDI,EDX
1000eed0 MOV EDX,EAX
1000eed2 OR ECX,0xffffffff
1000eed5 XOR EAX,EAX
1000eed7 SCASB.REPNE ES:EDI
1000eed9 MOV ECX,EDX
1000eedb DEC EDI
1000eedc SHR ECX,0x2
1000eedf MOVSD.REP ES:EDI,ESI
1000eee1 MOV ECX,EDX
1000eee3 MOV EDX,dword ptr [ESP + 0x64]
1000eee7 AND ECX,0x3
1000eeea MOVSB.REP ES:EDI,ESI
1000eeec MOV EDI,dword ptr [ESP + 0x204]
1000eef3 OR ECX,0xffffffff
1000eef6 SCASB.REPNE ES:EDI
1000eef8 NOT ECX
1000eefa SUB EDI,ECX
1000eefc MOV EAX,ECX
1000eefe MOV ESI,EDI
1000ef00 MOV EDI,EDX
1000ef02 SHR ECX,0x2
1000ef05 MOVSD.REP ES:EDI,ESI
1000ef07 MOV ECX,EAX
1000ef09 XOR EAX,EAX
1000ef0b AND ECX,0x3
1000ef0e MOVSB.REP ES:EDI,ESI
1000ef10 MOV EDI,0x1006bfac
1000ef15 OR ECX,0xffffffff
1000ef18 SCASB.REPNE ES:EDI
1000ef1a NOT ECX
1000ef1c SUB EDI,ECX
1000ef1e MOV EAX,ECX
1000ef20 MOV ESI,EDI
1000ef22 OR ECX,0xffffffff
1000ef25 MOV dword ptr [ESP + 0x14],EAX
1000ef29 MOV EDI,EDX
1000ef2b XOR EAX,EAX
1000ef2d MOV EDX,dword ptr [ESP + 0x14]
1000ef31 SCASB.REPNE ES:EDI
1000ef33 MOV ECX,EDX
1000ef35 DEC EDI
1000ef36 SHR ECX,0x2
1000ef39 MOVSD.REP ES:EDI,ESI
1000ef3b MOV ECX,EDX
1000ef3d MOV EDX,dword ptr [ESP + 0x18]
1000ef41 AND ECX,0x3
1000ef44 MOVSB.REP ES:EDI,ESI
1000ef46 MOV EDI,dword ptr [ESP + 0x204]
1000ef4d OR ECX,0xffffffff
1000ef50 SCASB.REPNE ES:EDI
1000ef52 NOT ECX
1000ef54 SUB EDI,ECX
1000ef56 MOV EAX,ECX
1000ef58 MOV ESI,EDI
1000ef5a SHR ECX,0x2
1000ef5d MOV EDI,EDX
1000ef5f MOVSD.REP ES:EDI,ESI
1000ef61 MOV ECX,EAX
1000ef63 XOR EAX,EAX
1000ef65 AND ECX,0x3
1000ef68 MOVSB.REP ES:EDI,ESI
1000ef6a OR ECX,0xffffffff
1000ef6d MOV EDI,0x1006bfa4
1000ef72 SCASB.REPNE ES:EDI
1000ef74 NOT ECX
1000ef76 SUB EDI,ECX
1000ef78 MOV EAX,ECX
1000ef7a MOV ESI,EDI
1000ef7c OR ECX,0xffffffff
1000ef7f MOV EDI,EDX
1000ef81 MOV EDX,EAX
1000ef83 XOR EAX,EAX
1000ef85 SCASB.REPNE ES:EDI
1000ef87 MOV ECX,EDX
1000ef89 DEC EDI
1000ef8a SHR ECX,0x2
1000ef8d MOVSD.REP ES:EDI,ESI
1000ef8f MOV ECX,EDX
1000ef91 MOV EDX,dword ptr [ESP + 0x10]
1000ef95 AND ECX,0x3
1000ef98 MOVSB.REP ES:EDI,ESI
1000ef9a MOV EDI,dword ptr [ESP + 0x204]
1000efa1 OR ECX,0xffffffff
1000efa4 SCASB.REPNE ES:EDI
1000efa6 NOT ECX
1000efa8 SUB EDI,ECX
1000efaa MOV EAX,ECX
1000efac MOV ESI,EDI
1000efae MOV EDI,EDX
1000efb0 SHR ECX,0x2
1000efb3 MOVSD.REP ES:EDI,ESI
1000efb5 MOV ECX,EAX
1000efb7 XOR EAX,EAX
1000efb9 AND ECX,0x3
1000efbc MOVSB.REP ES:EDI,ESI
1000efbe MOV EDI,0x1006bf9c
1000efc3 OR ECX,0xffffffff
1000efc6 SCASB.REPNE ES:EDI
1000efc8 NOT ECX
1000efca SUB EDI,ECX
1000efcc MOV EAX,ECX
1000efce MOV ESI,EDI
1000efd0 MOV EDI,EDX
1000efd2 MOV EDX,EAX
1000efd4 OR ECX,0xffffffff
1000efd7 XOR EAX,EAX
1000efd9 SCASB.REPNE ES:EDI
1000efdb MOV ECX,EDX
1000efdd DEC EDI
1000efde SHR ECX,0x2
1000efe1 MOVSD.REP ES:EDI,ESI
1000efe3 MOV ECX,EDX
1000efe5 AND ECX,0x3
1000efe8 MOVSB.REP ES:EDI,ESI
1000efea CALL 0x10056568
1000efef MOV dword ptr [EBP + 0x7c1c],EAX
1000eff5 TEST EAX,EAX
1000eff7 JNZ 0x1000f00d
1000eff9 PUSH 0x1006bf7c
1000effe MOV EDX,0x1006bf60
1000f003 MOV ECX,0x1006b3f4
1000f008 CALL 0x10051b20
1000f00d CALL 0x10056562
1000f012 TEST EAX,EAX
1000f014 MOV dword ptr [EBP + 0x7bf8],EAX
1000f01a JNZ 0x1000f030
1000f01c PUSH 0x1006bf40
1000f021 MOV EDX,0x1006bf60
1000f026 MOV ECX,0x1006b3f4
1000f02b CALL 0x10051b20
1000f030 MOV ECX,dword ptr [EBP + 0x7bf8]
1000f036 MOV EAX,EBP
1000f038 NEG EAX
1000f03a MOV EDX,dword ptr [ECX]
1000f03c LEA ESI,[EBP + 0x4]
1000f03f SBB EAX,EAX
1000f041 AND EAX,ESI
1000f043 PUSH EAX
1000f044 PUSH 0x0
1000f046 PUSH 0x0
1000f048 PUSH ECX
1000f049 CALL dword ptr [EDX + 0x10]
1000f04c MOV dword ptr [EBP + 0x7bf4],EAX
1000f052 MOV EAX,dword ptr [EBP + 0x7c1c]
1000f058 TEST EAX,EAX
1000f05a JZ 0x1000f08f
1000f05c MOV EAX,EBP
1000f05e MOV EDI,dword ptr [ESP + 0x200]
1000f065 NEG EAX
1000f067 SBB EAX,EAX
1000f069 AND EAX,ESI
1000f06b PUSH EAX
1000f06c MOV EAX,dword ptr [ESP + 0x68]
1000f070 PUSH EAX
1000f071 PUSH EDI
1000f072 CALL 0x1005658c
1000f077 MOV EDX,dword ptr [ESP + 0x18]
1000f07b MOV dword ptr [EBP + 0x7be0],EAX
1000f081 MOV ECX,dword ptr [EAX]
1000f083 PUSH EDX
1000f084 PUSH EDI
1000f085 PUSH EAX
1000f086 CALL dword ptr [ECX + 0x18]
1000f089 MOV dword ptr [EBP + 0x7be4],EAX
1000f08f MOV EAX,EBP
1000f091 NEG EAX
1000f093 SBB EAX,EAX
1000f095 AND EAX,ESI
1000f097 PUSH EAX
1000f098 PUSH 0x0
1000f09a PUSH 0x0
1000f09c CALL 0x1004d7d0
1000f0a1 MOV dword ptr [EBP + 0x7bfc],EAX
1000f0a7 MOV EAX,EBP
1000f0a9 NEG EAX
1000f0ab SBB EAX,EAX
1000f0ad AND EAX,ESI
1000f0af PUSH EAX
1000f0b0 CALL 0x10057a5c
1000f0b5 MOV EDI,dword ptr [EAX]
1000f0b7 LEA ECX,[EBP + 0x7bf0]
1000f0bd PUSH ECX
1000f0be MOV EDX,0x203
1000f0c3 MOV ECX,EAX
1000f0c5 CALL dword ptr [EDI]
1000f0c7 TEST EAX,EAX
1000f0c9 JNZ 0x1000f0df
1000f0cb PUSH 0x1006bf18
1000f0d0 MOV EDX,0x1006bf60
1000f0d5 MOV ECX,0x1006b3f4
1000f0da CALL 0x10051b20
1000f0df MOV EAX,EBP
1000f0e1 NEG EAX
1000f0e3 SBB EAX,EAX
1000f0e5 AND EAX,ESI
1000f0e7 PUSH EAX
1000f0e8 PUSH 0x0
1000f0ea PUSH 0x0
1000f0ec CALL 0x10057a56
1000f0f1 MOV EDI,dword ptr [EAX]
1000f0f3 LEA EDX,[EBP + 0x7be8]
1000f0f9 PUSH EDX
1000f0fa MOV EDX,0x13
1000f0ff MOV ECX,EAX
1000f101 CALL dword ptr [EDI]
1000f103 TEST EAX,EAX
1000f105 JNZ 0x1000f11b
1000f107 PUSH 0x1006beec
1000f10c MOV EDX,0x1006bf60
1000f111 MOV ECX,0x1006b3f4
1000f116 CALL 0x10051b20
1000f11b MOV EAX,EBP
1000f11d NEG EAX
1000f11f SBB EAX,EAX
1000f121 AND EAX,ESI
1000f123 PUSH EAX
1000f124 MOV EAX,dword ptr [ESP + 0x14]
1000f128 PUSH EAX
1000f129 PUSH 0x0
1000f12b CALL 0x10057a50
1000f130 MOV ESI,dword ptr [EAX]
1000f132 LEA ECX,[EBP + 0x7bec]
1000f138 PUSH ECX
1000f139 MOV EDX,0x302
1000f13e MOV ECX,EAX
1000f140 CALL dword ptr [ESI]
1000f142 TEST EAX,EAX
1000f144 JNZ 0x1000f15a
1000f146 PUSH 0x1006bec0
1000f14b MOV EDX,0x1006bf60
1000f150 MOV ECX,0x1006b3f4
1000f155 CALL 0x10051b20
1000f15a MOV EDX,dword ptr [ESP + 0x68]
1000f15e PUSH EDX
1000f15f CALL 0x1005655c
1000f164 TEST EAX,EAX
1000f166 MOV dword ptr [EBP + 0x7c28],EAX
1000f16c JNZ 0x1000f182
1000f16e PUSH 0x1006bea8
1000f173 MOV EDX,0x1006bf60
1000f178 MOV ECX,0x1006b3f4
1000f17d CALL 0x10051b20
1000f182 MOV EAX,dword ptr [EBP + 0x7c28]
1000f188 PUSH EAX
1000f189 MOV ECX,dword ptr [EAX]
1000f18b CALL dword ptr [ECX + 0x14]
1000f18e MOV dword ptr [ESP + 0x14],EAX
1000f192 MOV dword ptr [EBP + 0x284],0x1
1000f19c MOV EAX,dword ptr [EBP + 0x7c28]
1000f1a2 PUSH 0x3
1000f1a4 PUSH EAX
1000f1a5 MOV EDX,dword ptr [EAX]
1000f1a7 CALL dword ptr [EDX + 0x20]
1000f1aa MOV ESI,EAX
1000f1ac CMP ESI,-0x1
1000f1af JNZ 0x1000f1c5
1000f1b1 PUSH 0x1006be94
1000f1b6 MOV EDX,0x1006bf60
1000f1bb MOV ECX,0x1006b3f4
1000f1c0 CALL 0x10051b20
1000f1c5 MOV ECX,dword ptr [ESP + 0x14]
1000f1c9 MOV EAX,ESI
1000f1cb SHL EAX,0x6
1000f1ce PUSH 0x1
1000f1d0 PUSH ESI
1000f1d1 MOV EDX,dword ptr [EAX + ECX*0x1 + 0x4]
1000f1d5 MOV dword ptr [EBP + 0x288],EDX
1000f1db MOV EAX,dword ptr [EBP + 0x7c28]
1000f1e1 PUSH EAX
1000f1e2 MOV ECX,dword ptr [EAX]
1000f1e4 CALL dword ptr [ECX + 0x18]
1000f1e7 MOV dword ptr [EBP + 0x294],EAX
1000f1ed MOV dword ptr [EBP + 0x298],0xc
1000f1f7 MOV EAX,dword ptr [EBP + 0x7c28]
1000f1fd MOV EDI,0x4
1000f202 PUSH EDI
1000f203 PUSH EAX
1000f204 MOV EDX,dword ptr [EAX]
1000f206 CALL dword ptr [EDX + 0x20]
1000f209 MOV ESI,EAX
1000f20b CMP ESI,-0x1
1000f20e JNZ 0x1000f224
1000f210 PUSH 0x1006be94
1000f215 MOV EDX,0x1006bf60
1000f21a MOV ECX,0x1006b3f4
1000f21f CALL 0x10051b20
1000f224 MOV EAX,dword ptr [EBP + 0x7c28]
1000f22a PUSH 0x1
1000f22c PUSH ESI
1000f22d PUSH EAX
1000f22e MOV ECX,dword ptr [EAX]
1000f230 CALL dword ptr [ECX + 0x18]
1000f233 MOV dword ptr [EBP + 0x29c],EAX
1000f239 MOV dword ptr [EBP + 0x2a0],EDI
1000f23f MOV EAX,dword ptr [EBP + 0x7c28]
1000f245 PUSH 0x5
1000f247 PUSH EAX
1000f248 MOV EDX,dword ptr [EAX]
1000f24a CALL dword ptr [EDX + 0x20]
1000f24d MOV ESI,EAX
1000f24f CMP ESI,-0x1
1000f252 JNZ 0x1000f268
1000f254 PUSH 0x1006be94
1000f259 MOV EDX,0x1006bf60
1000f25e MOV ECX,0x1006b3f4
1000f263 CALL 0x10051b20
1000f268 MOV EAX,dword ptr [EBP + 0x7c28]
1000f26e PUSH 0x1
1000f270 PUSH ESI
1000f271 PUSH EAX
1000f272 MOV ECX,dword ptr [EAX]
1000f274 CALL dword ptr [ECX + 0x18]
1000f277 MOV dword ptr [EBP + 0x2b4],EAX
1000f27d MOV dword ptr [EBP + 0x2b8],EDI
1000f283 MOV dword ptr [EBP + 0x2bc],0x0
1000f28d MOV EAX,dword ptr [EBP + 0x7c28]
1000f293 PUSH 0x12
1000f295 PUSH EAX
1000f296 MOV EDX,dword ptr [EAX]
1000f298 CALL dword ptr [EDX + 0x20]
1000f29b MOV ESI,EAX
1000f29d CMP ESI,-0x1
1000f2a0 JNZ 0x1000f2b6
1000f2a2 PUSH 0x1006be68
1000f2a7 MOV EDX,0x1006bf60
1000f2ac MOV ECX,0x1006b3f4
1000f2b1 CALL 0x10051b20
1000f2b6 MOV EAX,dword ptr [EBP + 0x7c28]
1000f2bc PUSH 0x1
1000f2be PUSH ESI
1000f2bf PUSH EAX
1000f2c0 MOV ECX,dword ptr [EAX]
1000f2c2 CALL dword ptr [ECX + 0x18]
1000f2c5 MOV dword ptr [EBP + 0x2bc],EAX
1000f2cb MOV dword ptr [EBP + 0x2c0],EDI
1000f2d1 MOV EAX,dword ptr [EBP + 0x7c28]
1000f2d7 PUSH 0xe
1000f2d9 PUSH EAX
1000f2da MOV EDX,dword ptr [EAX]
1000f2dc CALL dword ptr [EDX + 0x20]
1000f2df CMP EAX,-0x1
1000f2e2 JNZ 0x1000f2ee
1000f2e4 XOR EAX,EAX
1000f2e6 MOV dword ptr [EBP + 0x2ec],EAX
1000f2ec JMP 0x1000f30b
1000f2ee MOV ECX,dword ptr [EBP + 0x7c28]
1000f2f4 PUSH 0x1
1000f2f6 PUSH EAX
1000f2f7 PUSH ECX
1000f2f8 MOV EDX,dword ptr [ECX]
1000f2fa CALL dword ptr [EDX + 0x18]
1000f2fd MOV dword ptr [EBP + 0x2ec],EAX
1000f303 MOV dword ptr [EBP + 0x2f0],EDI
1000f309 XOR EAX,EAX
1000f30b MOV dword ptr [EBP + 0x2a4],EAX
1000f311 MOV dword ptr [EBP + 0x2ac],EAX
1000f317 MOV EAX,dword ptr [EBP + 0x7c28]
1000f31d PUSH 0x15
1000f31f PUSH EAX
1000f320 MOV ECX,dword ptr [EAX]
1000f322 CALL dword ptr [ECX + 0x20]
1000f325 MOV ESI,EAX
1000f327 CMP ESI,-0x1
1000f32a JNZ 0x1000f340
1000f32c PUSH 0x1006be94
1000f331 MOV EDX,0x1006bf60
1000f336 MOV ECX,0x1006b3f4
1000f33b CALL 0x10051b20
1000f340 MOV EAX,dword ptr [ESP + 0x14]
1000f344 MOV EDX,ESI
1000f346 SHL EDX,0x6
1000f349 PUSH 0x1
1000f34b PUSH ESI
1000f34c MOV ECX,dword ptr [EDX + EAX*0x1 + 0x4]
1000f350 MOV dword ptr [EBP + 0x28c],ECX
1000f356 MOV EAX,dword ptr [EBP + 0x7c28]
1000f35c PUSH EAX
1000f35d MOV EDX,dword ptr [EAX]
1000f35f CALL dword ptr [EDX + 0x18]
1000f362 MOV dword ptr [EBP + 0x2f4],EAX
1000f368 MOV EAX,dword ptr [EBP + 0x7c28]
1000f36e PUSH 0x2
1000f370 PUSH EAX
1000f371 MOV ECX,dword ptr [EAX]
1000f373 CALL dword ptr [ECX + 0x20]
1000f376 MOV ESI,EAX
1000f378 CMP ESI,-0x1
1000f37b JNZ 0x1000f391
1000f37d PUSH 0x1006be94
1000f382 MOV EDX,0x1006bf60
1000f387 MOV ECX,0x1006b3f4
1000f38c CALL 0x10051b20
1000f391 MOV EAX,dword ptr [EBP + 0x7c28]
1000f397 PUSH 0x0
1000f399 PUSH ESI
1000f39a PUSH EAX
1000f39b MOV EDX,dword ptr [EAX]
1000f39d CALL dword ptr [EDX + 0x18]
1000f3a0 PUSH 0x28
1000f3a2 MOV dword ptr [EBP + 0x14c],EAX
1000f3a8 CALL 0x100597e7
1000f3ad MOV dword ptr [EBP + 0x7b60],EAX
1000f3b3 MOV ESI,dword ptr [EBP + 0x14c]
1000f3b9 MOV ECX,0x18
1000f3be LEA EDI,[EBP + 0x7b80]
1000f3c4 MOVSD.REP ES:EDI,ESI
1000f3c6 MOV EAX,dword ptr [EBP + 0x14c]
1000f3cc MOV ECX,dword ptr [EBP + 0x7b60]
1000f3d2 ADD ESP,0x4
1000f3d5 MOV EDX,dword ptr [EAX]
1000f3d7 MOV dword ptr [ECX + 0x4],EDX
1000f3da MOV EAX,dword ptr [EBP + 0x14c]
1000f3e0 MOV ECX,dword ptr [EBP + 0x7b60]
1000f3e6 PUSH 0x1
1000f3e8 MOV EDX,dword ptr [EAX + 0x4]
1000f3eb MOV dword ptr [ECX + 0x8],EDX
1000f3ee MOV EAX,dword ptr [EBP + 0x7c28]
1000f3f4 PUSH EAX
1000f3f5 MOV ECX,dword ptr [EAX]
1000f3f7 CALL dword ptr [ECX + 0x20]
1000f3fa MOV ESI,EAX
1000f3fc CMP ESI,-0x1
1000f3ff JNZ 0x1000f415
1000f401 PUSH 0x1006be94
1000f406 MOV EDX,0x1006bf60
1000f40b MOV ECX,0x1006b3f4
1000f410 CALL 0x10051b20
1000f415 MOV EAX,dword ptr [EBP + 0x7c28]
1000f41b PUSH 0x0
1000f41d PUSH ESI
1000f41e PUSH EAX
1000f41f MOV EDX,dword ptr [EAX]
1000f421 CALL dword ptr [EDX + 0x18]
1000f424 MOV dword ptr [EBP + 0x148],EAX
1000f42a MOV EAX,dword ptr [ESP + 0x14]
1000f42e MOV ECX,dword ptr [EBP + 0x7b60]
1000f434 SHL ESI,0x6
1000f437 ADD EAX,ESI
1000f439 MOV EDX,dword ptr [EAX + 0x4]
1000f43c MOV dword ptr [ECX + 0x24],EDX
1000f43f MOV ECX,dword ptr [EBP + 0x7b60]
1000f445 MOV EDX,dword ptr [EAX + 0x8]
1000f448 MOV dword ptr [ECX + 0x1c],EDX
1000f44b MOV ECX,dword ptr [EBP + 0x7b60]
1000f451 MOV EAX,dword ptr [ECX + 0x24]
1000f454 CDQ
1000f455 IDIV dword ptr [ECX + 0x1c]
1000f458 MOV dword ptr [ECX + 0x20],EAX
1000f45b MOV ESI,dword ptr [EBP + 0x14c]
1000f461 ADD ESI,0x94
1000f467 MOV EAX,dword ptr [ESI + 0x8]
1000f46a MOV ECX,dword ptr [ESI + 0x4]
1000f46d MOV EDX,dword ptr [ESI]
1000f46f PUSH EAX
1000f470 PUSH ECX
1000f471 PUSH EDX
1000f472 LEA ECX,[ESP + 0xc0]
1000f479 CALL 0x1000e230
1000f47e MOV ECX,dword ptr [EAX]
1000f480 MOV dword ptr [ESP + 0x124],ECX
1000f487 MOV ECX,dword ptr [ESI + 0x8]
1000f48a MOV EDX,dword ptr [EAX + 0x4]
1000f48d PUSH ECX
1000f48e MOV dword ptr [ESP + 0x12c],EDX
1000f495 MOV EDX,dword ptr [ESI + 0x4]
1000f498 MOV EAX,dword ptr [EAX + 0x8]
1000f49b PUSH EDX
1000f49c MOV dword ptr [ESP + 0x134],EAX
1000f4a3 MOV EAX,dword ptr [ESI + 0xc]
1000f4a6 PUSH EAX
1000f4a7 LEA ECX,[ESP + 0xcc]
1000f4ae CALL 0x1000e230
1000f4b3 MOV ECX,dword ptr [ESI + 0x8]
1000f4b6 MOV EDX,dword ptr [ESI + 0x10]
1000f4b9 MOV EAX,dword ptr [ESI + 0xc]
1000f4bc PUSH ECX
1000f4bd PUSH EDX
1000f4be PUSH EAX
1000f4bf LEA ECX,[ESP + 0xb4]
1000f4c6 CALL 0x1000e230
1000f4cb MOV ECX,dword ptr [ESI + 0x14]
1000f4ce MOV EDX,dword ptr [ESI + 0x10]
1000f4d1 MOV EAX,dword ptr [ESI + 0xc]
1000f4d4 PUSH ECX
1000f4d5 PUSH EDX
1000f4d6 PUSH EAX
1000f4d7 LEA ECX,[ESP + 0x9c]
1000f4de CALL 0x1000e230
1000f4e3 MOV ECX,dword ptr [EAX]
1000f4e5 MOV dword ptr [ESP + 0x178],ECX
1000f4ec MOV ECX,dword ptr [ESI + 0x14]
1000f4ef MOV EDX,dword ptr [EAX + 0x4]
1000f4f2 PUSH ECX
1000f4f3 MOV dword ptr [ESP + 0x180],EDX
1000f4fa MOV EDX,dword ptr [ESI + 0x10]
1000f4fd MOV EAX,dword ptr [EAX + 0x8]
1000f500 PUSH EDX
1000f501 MOV dword ptr [ESP + 0x188],EAX
1000f508 MOV EAX,dword ptr [ESI]
1000f50a PUSH EAX
1000f50b LEA ECX,[ESP + 0x84]
1000f512 CALL 0x1000e230
1000f517 MOV ECX,dword ptr [ESI + 0x14]
1000f51a MOV EDX,dword ptr [ESI + 0x4]
1000f51d MOV EAX,dword ptr [ESI]
1000f51f PUSH ECX
1000f520 PUSH EDX
1000f521 PUSH EAX
1000f522 LEA ECX,[ESP + 0xa8]
1000f529 CALL 0x1000e230
1000f52e MOV ECX,dword ptr [ESI + 0x8]
1000f531 MOV EDX,dword ptr [ESI + 0x10]
1000f534 MOV EAX,dword ptr [ESI]
1000f536 PUSH ECX
1000f537 PUSH EDX
1000f538 PUSH EAX
1000f539 LEA ECX,[ESP + 0x78]
1000f53d CALL 0x1000e230
1000f542 MOV ECX,dword ptr [ESI + 0x14]
1000f545 MOV EDX,dword ptr [ESI + 0x4]
1000f548 MOV EAX,dword ptr [ESI + 0xc]
1000f54b PUSH ECX
1000f54c PUSH EDX
1000f54d PUSH EAX
1000f54e LEA ECX,[ESP + 0x90]
1000f555 CALL 0x1000e230
1000f55a FLD float ptr [ESP + 0x178]
1000f561 FSUB float ptr [ESP + 0x124]
1000f568 MOV ECX,dword ptr [EBP + 0x7b60]
1000f56e PUSH 0xb
1000f570 FSTP float ptr [ECX + 0xc]
1000f573 FLD float ptr [ESP + 0x180]
1000f57a FSUB float ptr [ESP + 0x12c]
1000f581 MOV EDX,dword ptr [EBP + 0x7b60]
1000f587 FSTP float ptr [EDX + 0x10]
1000f58a MOV EAX,dword ptr [EBP + 0x7b60]
1000f590 FLD float ptr [0x1006516c]
1000f596 FDIV float ptr [EAX + 0xc]
1000f599 FSTP float ptr [EAX + 0x14]
1000f59c MOV EAX,dword ptr [EBP + 0x7b60]
1000f5a2 FLD float ptr [0x1006516c]
1000f5a8 FDIV float ptr [EAX + 0x10]
1000f5ab FSTP float ptr [EAX + 0x18]
1000f5ae MOV EAX,dword ptr [EBP + 0x7c28]
1000f5b4 PUSH EAX
1000f5b5 MOV ECX,dword ptr [EAX]
1000f5b7 CALL dword ptr [ECX + 0x20]
1000f5ba MOV ESI,EAX
1000f5bc CMP ESI,-0x1
1000f5bf JNZ 0x1000f5d5
1000f5c1 PUSH 0x1006be94
1000f5c6 MOV EDX,0x1006bf60
1000f5cb MOV ECX,0x1006b3f4
1000f5d0 CALL 0x10051b20
1000f5d5 MOV EAX,dword ptr [EBP + 0x7c28]
1000f5db PUSH 0x1
1000f5dd PUSH ESI
1000f5de PUSH EAX
1000f5df MOV EDX,dword ptr [EAX]
1000f5e1 CALL dword ptr [EDX + 0x18]
1000f5e4 MOV dword ptr [EBP + 0x150],EAX
1000f5ea MOV EAX,dword ptr [ESP + 0x68]
1000f5ee PUSH EAX
1000f5ef CALL 0x10059600
1000f5f4 MOV ECX,dword ptr [ESP + 0x68]
1000f5f8 PUSH ECX
1000f5f9 CALL 0x10059600
1000f5fe MOV EDX,dword ptr [ESP + 0x20]
1000f602 PUSH EDX
1000f603 CALL 0x10059600
1000f608 MOV EAX,dword ptr [ESP + 0x1c]
1000f60c PUSH EAX
1000f60d CALL 0x10059600
1000f612 MOV dword ptr [EBP + 0x7cdc],0xbf800000
1000f61c MOV EAX,dword ptr [EBP + 0x28c]
1000f622 ADD ESP,0x10
1000f625 XOR ECX,ECX
1000f627 TEST EAX,EAX
1000f629 JBE 0x1000f661
1000f62b MOV EDX,dword ptr [EBP + 0x2f4]
1000f631 TEST dword ptr [EDX],0x20000
1000f637 JNZ 0x1000f643
1000f639 INC ECX
1000f63a ADD EDX,0x1c
1000f63d CMP ECX,EAX
1000f63f JC 0x1000f631
1000f641 JMP 0x1000f661
1000f643 XOR EAX,EAX
1000f645 MOV ECX,dword ptr [EBP + 0x294]
1000f64b MOV AX,word ptr [EDX + 0x8]
1000f64f IMUL EAX,dword ptr [EBP + 0x298]
1000f656 ADD EAX,ECX
1000f658 MOV ECX,dword ptr [EAX + 0x8]
1000f65b MOV dword ptr [EBP + 0x7cdc],ECX
1000f661 MOV ECX,0x18
1000f666 MOV ESI,0x10070950
1000f66b MOV EDI,EBX
1000f66d XOR EAX,EAX
1000f66f MOVSD.REP ES:EDI,ESI
1000f671 MOV ECX,dword ptr [EBP + 0x28c]
1000f677 MOV dword ptr [ESP + 0x10],EAX
1000f67b CMP ECX,EAX
1000f67d JBE 0x1000fe21
1000f683 MOV dword ptr [ESP + 0x18],EAX
1000f687 ADD EAX,dword ptr [EBP + 0x2f4]
1000f68d TEST dword ptr [EAX],0x20000
1000f693 JZ 0x1000fdff
1000f699 MOV EDX,dword ptr [EBP + 0x298]
1000f69f XOR ECX,ECX
1000f6a1 MOV CX,word ptr [EAX + 0x8]
1000f6a5 XOR ESI,ESI
1000f6a7 MOV SI,word ptr [EAX + 0xa]
1000f6ab IMUL ECX,EDX
1000f6ae IMUL ESI,EDX
1000f6b1 MOV EDI,dword ptr [EBP + 0x294]
1000f6b7 ADD ECX,EDI
1000f6b9 ADD ESI,EDI
1000f6bb XOR EDI,EDI
1000f6bd MOV DI,word ptr [EAX + 0xc]
1000f6c1 MOV EAX,dword ptr [EBP + 0x294]
1000f6c7 IMUL EDI,EDX
1000f6ca FLD float ptr [ECX]
1000f6cc MOV EDX,EBX
1000f6ce ADD EDI,EAX
1000f6d0 MOV EAX,dword ptr [EDX]
1000f6d2 MOV dword ptr [ESP + 0x1c],EAX
1000f6d6 MOV EAX,dword ptr [EDX + 0x4]
1000f6d9 MOV dword ptr [ESP + 0x20],EAX
1000f6dd LEA EAX,[EBX + 0x54]
1000f6e0 MOV EDX,dword ptr [EDX + 0x8]
1000f6e3 FCOMP float ptr [ESP + 0x1c]
1000f6e7 MOV dword ptr [ESP + 0x24],EDX
1000f6eb MOV EDX,dword ptr [EAX]
1000f6ed MOV dword ptr [ESP + 0x34],EDX
1000f6f1 MOV EDX,dword ptr [EAX + 0x4]
1000f6f4 MOV dword ptr [ESP + 0x38],EDX
1000f6f8 MOV EAX,dword ptr [EAX + 0x8]
1000f6fb MOV dword ptr [ESP + 0x3c],EAX
1000f6ff FNSTSW AX
1000f701 TEST AH,0x1
1000f704 JZ 0x1000f70c
1000f706 MOV EDX,dword ptr [ECX]
1000f708 MOV dword ptr [ESP + 0x1c],EDX
1000f70c FLD float ptr [ECX + 0x4]
1000f70f FCOMP float ptr [ESP + 0x20]
1000f713 FNSTSW AX
1000f715 TEST AH,0x1
1000f718 JZ 0x1000f721
1000f71a MOV EAX,dword ptr [ECX + 0x4]
1000f71d MOV dword ptr [ESP + 0x20],EAX
1000f721 FLD float ptr [ECX + 0x8]
1000f724 FCOMP float ptr [ESP + 0x24]
1000f728 FNSTSW AX
1000f72a TEST AH,0x1
1000f72d JZ 0x1000f736
1000f72f MOV EDX,dword ptr [ECX + 0x8]
1000f732 MOV dword ptr [ESP + 0x24],EDX
1000f736 FLD float ptr [ECX]
1000f738 FCOMP float ptr [ESP + 0x34]
1000f73c FNSTSW AX
1000f73e TEST AH,0x41
1000f741 JNZ 0x1000f749
1000f743 MOV EAX,dword ptr [ECX]
1000f745 MOV dword ptr [ESP + 0x34],EAX
1000f749 FLD float ptr [ECX + 0x4]
1000f74c FCOMP float ptr [ESP + 0x38]
1000f750 FNSTSW AX
1000f752 TEST AH,0x41
1000f755 JNZ 0x1000f75e
1000f757 MOV EDX,dword ptr [ECX + 0x4]
1000f75a MOV dword ptr [ESP + 0x38],EDX
1000f75e FLD float ptr [ECX + 0x8]
1000f761 FCOMP float ptr [ESP + 0x3c]
1000f765 FNSTSW AX
1000f767 TEST AH,0x41
1000f76a JNZ 0x1000f773
1000f76c MOV EAX,dword ptr [ECX + 0x8]
1000f76f MOV dword ptr [ESP + 0x3c],EAX
1000f773 FLD float ptr [ESP + 0x1c]
1000f777 FSTP float ptr [ESP + 0x114]
1000f77e FLD float ptr [ESP + 0x20]
1000f782 FSTP float ptr [ESP + 0x118]
1000f789 FLD float ptr [ESP + 0x24]
1000f78d FSTP float ptr [ESP + 0x11c]
1000f794 FLD float ptr [ESP + 0x34]
1000f798 FSTP float ptr [ESP + 0x108]
1000f79f FLD float ptr [ESP + 0x20]
1000f7a3 FSTP float ptr [ESP + 0x10c]
1000f7aa FLD float ptr [ESP + 0x24]
1000f7ae FSTP float ptr [ESP + 0x110]
1000f7b5 FLD float ptr [ESP + 0x34]
1000f7b9 MOV EDX,dword ptr [ESP + 0x114]
1000f7c0 MOV EAX,dword ptr [ESP + 0x118]
1000f7c7 FSTP float ptr [ESP + 0xf0]
1000f7ce FLD float ptr [ESP + 0x38]
1000f7d2 FSTP float ptr [ESP + 0xf4]
1000f7d9 FLD float ptr [ESP + 0x24]
1000f7dd MOV ECX,EBX
1000f7df FSTP float ptr [ESP + 0xf8]
1000f7e6 FLD float ptr [ESP + 0x34]
1000f7ea MOV dword ptr [ECX],EDX
1000f7ec MOV EDX,dword ptr [ESP + 0x11c]
1000f7f3 FSTP float ptr [ESP + 0xfc]
1000f7fa FLD float ptr [ESP + 0x38]
1000f7fe MOV dword ptr [ECX + 0x4],EAX
1000f801 LEA EAX,[EBX + 0xc]
1000f804 FSTP float ptr [ESP + 0x100]
1000f80b MOV dword ptr [ECX + 0x8],EDX
1000f80e MOV ECX,dword ptr [ESP + 0x108]
1000f815 MOV EDX,dword ptr [ESP + 0x10c]
1000f81c MOV dword ptr [EAX],ECX
1000f81e MOV ECX,dword ptr [ESP + 0x110]
1000f825 FLD float ptr [ESP + 0x3c]
1000f829 MOV dword ptr [EAX + 0x4],EDX
1000f82c LEA EDX,[EBX + 0x18]
1000f82f FSTP float ptr [ESP + 0x104]
1000f836 FLD float ptr [ESP + 0x1c]
1000f83a MOV dword ptr [EAX + 0x8],ECX
1000f83d MOV EAX,dword ptr [ESP + 0xf0]
1000f844 MOV ECX,dword ptr [ESP + 0xf4]
1000f84b MOV dword ptr [EDX],EAX
1000f84d MOV EAX,dword ptr [ESP + 0xf8]
1000f854 MOV dword ptr [EDX + 0x4],ECX
1000f857 MOV ECX,dword ptr [ESP + 0xfc]
1000f85e FSTP float ptr [ESP + 0xd8]
1000f865 FLD float ptr [ESP + 0x38]
1000f869 MOV dword ptr [EDX + 0x8],EAX
1000f86c MOV EDX,dword ptr [ESP + 0x100]
1000f873 MOV EAX,dword ptr [ESP + 0x104]
1000f87a MOV dword ptr [EBX + 0x54],ECX
1000f87d FSTP float ptr [ESP + 0xdc]
1000f884 FLD float ptr [ESP + 0x3c]
1000f888 MOV dword ptr [EBX + 0x58],EDX
1000f88b MOV EDX,dword ptr [ESP + 0xd8]
1000f892 LEA ECX,[EBX + 0x30]
1000f895 MOV dword ptr [EBX + 0x5c],EAX
1000f898 MOV EAX,dword ptr [ESP + 0xdc]
1000f89f MOV dword ptr [ECX],EDX
1000f8a1 FSTP float ptr [ESP + 0xe0]
1000f8a8 MOV EDX,dword ptr [ESP + 0xe0]
1000f8af MOV dword ptr [ECX + 0x4],EAX
1000f8b2 FLD float ptr [ESP + 0x1c]
1000f8b6 MOV dword ptr [ECX + 0x8],EDX
1000f8b9 FSTP float ptr [ESP + 0xe4]
1000f8c0 FLD float ptr [ESP + 0x20]
1000f8c4 MOV ECX,dword ptr [ESP + 0xe4]
1000f8cb LEA EAX,[EBX + 0x3c]
1000f8ce FSTP float ptr [ESP + 0xe8]
1000f8d5 FLD float ptr [ESP + 0x3c]
1000f8d9 MOV EDX,dword ptr [ESP + 0xe8]
1000f8e0 MOV dword ptr [EAX],ECX
1000f8e2 FSTP float ptr [ESP + 0xec]
1000f8e9 MOV ECX,dword ptr [ESP + 0xec]
1000f8f0 MOV dword ptr [EAX + 0x4],EDX
1000f8f3 MOV EDX,dword ptr [ESP + 0x24]
1000f8f7 MOV dword ptr [EAX + 0x8],ECX
1000f8fa MOV EAX,dword ptr [ESP + 0x38]
1000f8fe MOV ECX,dword ptr [ESP + 0x1c]
1000f902 PUSH EDX
1000f903 PUSH EAX
1000f904 PUSH ECX
1000f905 LEA ECX,[ESP + 0x1f0]
1000f90c CALL 0x1000e230
1000f911 MOV ECX,dword ptr [EAX]
1000f913 LEA EDX,[EBX + 0x24]
1000f916 MOV dword ptr [EBX + 0x24],ECX
1000f919 MOV ECX,dword ptr [EAX + 0x4]
1000f91c MOV dword ptr [EDX + 0x4],ECX
1000f91f MOV EAX,dword ptr [EAX + 0x8]
1000f922 MOV ECX,dword ptr [ESP + 0x3c]
1000f926 MOV dword ptr [EDX + 0x8],EAX
1000f929 MOV EDX,dword ptr [ESP + 0x20]
1000f92d MOV EAX,dword ptr [ESP + 0x34]
1000f931 PUSH ECX
1000f932 PUSH EDX
1000f933 PUSH EAX
1000f934 LEA ECX,[ESP + 0x190]
1000f93b CALL 0x1000e230
1000f940 MOV EDX,dword ptr [EAX]
1000f942 LEA ECX,[EBX + 0x48]
1000f945 MOV dword ptr [EBX + 0x48],EDX
1000f948 MOV EDX,dword ptr [EAX + 0x4]
1000f94b MOV dword ptr [ECX + 0x4],EDX
1000f94e MOV EAX,dword ptr [EAX + 0x8]
1000f951 MOV dword ptr [ECX + 0x8],EAX
1000f954 MOV ECX,EBX
1000f956 FLD float ptr [ESI]
1000f958 MOV EDX,dword ptr [ECX]
1000f95a MOV dword ptr [ESP + 0x28],EDX
1000f95e MOV EAX,dword ptr [ECX + 0x4]
1000f961 FCOMP float ptr [ESP + 0x28]
1000f965 MOV ECX,dword ptr [ECX + 0x8]
1000f968 MOV dword ptr [ESP + 0x2c],EAX
1000f96c MOV dword ptr [ESP + 0x30],ECX
1000f970 LEA ECX,[EBX + 0x54]
1000f973 MOV EDX,ECX
1000f975 MOV EAX,dword ptr [EDX]
1000f977 MOV dword ptr [ESP + 0x40],EAX
1000f97b MOV EAX,dword ptr [EDX + 0x4]
1000f97e MOV dword ptr [ESP + 0x44],EAX
1000f982 MOV EDX,dword ptr [EDX + 0x8]
1000f985 FNSTSW AX
1000f987 MOV dword ptr [ESP + 0x48],EDX
1000f98b TEST AH,0x1
1000f98e JZ 0x1000f996
1000f990 MOV EAX,dword ptr [ESI]
1000f992 MOV dword ptr [ESP + 0x28],EAX
1000f996 FLD float ptr [ESI + 0x4]
1000f999 FCOMP float ptr [ESP + 0x2c]
1000f99d FNSTSW AX
1000f99f TEST AH,0x1
1000f9a2 JZ 0x1000f9ab
1000f9a4 MOV EDX,dword ptr [ESI + 0x4]
1000f9a7 MOV dword ptr [ESP + 0x2c],EDX
1000f9ab FLD float ptr [ESI + 0x8]
1000f9ae FCOMP float ptr [ESP + 0x30]
1000f9b2 FNSTSW AX
1000f9b4 TEST AH,0x1
1000f9b7 JZ 0x1000f9c0
1000f9b9 MOV EAX,dword ptr [ESI + 0x8]
1000f9bc MOV dword ptr [ESP + 0x30],EAX
1000f9c0 FLD float ptr [ESI]
1000f9c2 FCOMP float ptr [ESP + 0x40]
1000f9c6 FNSTSW AX
1000f9c8 TEST AH,0x41
1000f9cb JNZ 0x1000f9d3
1000f9cd MOV EDX,dword ptr [ESI]
1000f9cf MOV dword ptr [ESP + 0x40],EDX
1000f9d3 FLD float ptr [ESI + 0x4]
1000f9d6 FCOMP float ptr [ESP + 0x44]
1000f9da FNSTSW AX
1000f9dc TEST AH,0x41
1000f9df JNZ 0x1000f9e8
1000f9e1 MOV EAX,dword ptr [ESI + 0x4]
1000f9e4 MOV dword ptr [ESP + 0x44],EAX
1000f9e8 FLD float ptr [ESI + 0x8]
1000f9eb FCOMP float ptr [ESP + 0x48]
1000f9ef FNSTSW AX
1000f9f1 TEST AH,0x41
1000f9f4 JNZ 0x1000f9fd
1000f9f6 MOV EDX,dword ptr [ESI + 0x8]
1000f9f9 MOV dword ptr [ESP + 0x48],EDX
1000f9fd FLD float ptr [ESP + 0x28]
1000fa01 FSTP float ptr [ESP + 0xcc]
1000fa08 FLD float ptr [ESP + 0x2c]
1000fa0c FSTP float ptr [ESP + 0xd0]
1000fa13 FLD float ptr [ESP + 0x30]
1000fa17 FSTP float ptr [ESP + 0xd4]
1000fa1e FLD float ptr [ESP + 0x40]
1000fa22 FSTP float ptr [ESP + 0x84]
1000fa29 FLD float ptr [ESP + 0x2c]
1000fa2d FSTP float ptr [ESP + 0x88]
1000fa34 FLD float ptr [ESP + 0x30]
1000fa38 MOV EDX,dword ptr [ESP + 0xcc]
1000fa3f MOV EAX,EBX
1000fa41 FSTP float ptr [ESP + 0x8c]
1000fa48 FLD float ptr [ESP + 0x40]
1000fa4c FSTP float ptr [ESP + 0x6c]
1000fa50 FLD float ptr [ESP + 0x44]
1000fa54 MOV dword ptr [EAX],EDX
1000fa56 MOV EDX,dword ptr [ESP + 0xd0]
1000fa5d FSTP float ptr [ESP + 0x70]
1000fa61 FLD float ptr [ESP + 0x30]
1000fa65 MOV dword ptr [EAX + 0x4],EDX
1000fa68 MOV EDX,dword ptr [ESP + 0xd4]
1000fa6f FSTP float ptr [ESP + 0x74]
1000fa73 FLD float ptr [ESP + 0x40]
1000fa77 MOV dword ptr [EAX + 0x8],EDX
1000fa7a MOV EDX,dword ptr [ESP + 0x84]
1000fa81 LEA EAX,[EBX + 0xc]
1000fa84 MOV ESI,dword ptr [ESP + 0x48]
1000fa88 FSTP float ptr [ESP + 0x9c]
1000fa8f FLD float ptr [ESP + 0x44]
1000fa93 MOV dword ptr [EAX],EDX
1000fa95 MOV EDX,dword ptr [ESP + 0x88]
1000fa9c FSTP float ptr [ESP + 0xa0]
1000faa3 MOV dword ptr [EAX + 0x4],EDX
1000faa6 MOV EDX,dword ptr [ESP + 0x8c]
1000faad FLD float ptr [ESP + 0x48]
1000fab1 MOV dword ptr [EAX + 0x8],EDX
1000fab4 MOV EDX,dword ptr [ESP + 0x6c]
1000fab8 LEA EAX,[EBX + 0x18]
1000fabb PUSH ESI
1000fabc FSTP float ptr [ESP + 0xa8]
1000fac3 FLD float ptr [ESP + 0x2c]
1000fac7 MOV dword ptr [EAX],EDX
1000fac9 MOV EDX,dword ptr [ESP + 0x74]
1000facd FSTP float ptr [ESP + 0x7c]
1000fad1 MOV dword ptr [EAX + 0x4],EDX
1000fad4 MOV EDX,dword ptr [ESP + 0x78]
1000fad8 FLD float ptr [ESP + 0x48]
1000fadc MOV dword ptr [EAX + 0x8],EDX
1000fadf MOV EAX,dword ptr [ESP + 0xa0]
1000fae6 MOV EDX,dword ptr [ESP + 0xa4]
1000faed MOV dword ptr [ECX],EAX
1000faef MOV EAX,dword ptr [ESP + 0xa8]
1000faf6 MOV dword ptr [ECX + 0x4],EDX
1000faf9 MOV EDX,dword ptr [ESP + 0x7c]
1000fafd MOV dword ptr [ECX + 0x8],EAX
1000fb00 FSTP float ptr [ESP + 0x80]
1000fb07 FLD float ptr [ESP + 0x4c]
1000fb0b MOV EAX,dword ptr [ESP + 0x80]
1000fb12 LEA ECX,[EBX + 0x30]
1000fb15 FSTP float ptr [ESP + 0x84]
1000fb1c MOV dword ptr [ECX],EDX
1000fb1e MOV EDX,dword ptr [ESP + 0x84]
1000fb25 MOV dword ptr [ECX + 0x4],EAX
1000fb28 MOV dword ptr [ECX + 0x8],EDX
1000fb2b MOV EAX,dword ptr [ESP + 0x30]
1000fb2f MOV ECX,dword ptr [ESP + 0x2c]
1000fb33 PUSH EAX
1000fb34 PUSH ECX
1000fb35 LEA ECX,[ESP + 0x1cc]
1000fb3c CALL 0x1000e230
1000fb41 MOV ECX,dword ptr [EAX]
1000fb43 LEA EDX,[EBX + 0x3c]
1000fb46 MOV dword ptr [EBX + 0x3c],ECX
1000fb49 MOV ECX,dword ptr [EAX + 0x4]
1000fb4c MOV dword ptr [EDX + 0x4],ECX
1000fb4f MOV EAX,dword ptr [EAX + 0x8]
1000fb52 MOV ECX,dword ptr [ESP + 0x30]
1000fb56 MOV dword ptr [EDX + 0x8],EAX
1000fb59 MOV EDX,dword ptr [ESP + 0x44]
1000fb5d MOV EAX,dword ptr [ESP + 0x28]
1000fb61 PUSH ECX
1000fb62 PUSH EDX
1000fb63 PUSH EAX
1000fb64 LEA ECX,[ESP + 0x1b4]
1000fb6b CALL 0x1000e230
1000fb70 MOV EDX,dword ptr [EAX]
1000fb72 LEA ECX,[EBX + 0x24]
1000fb75 PUSH ESI
1000fb76 MOV dword ptr [ECX],EDX
1000fb78 MOV EDX,dword ptr [EAX + 0x4]
1000fb7b MOV dword ptr [ECX + 0x4],EDX
1000fb7e MOV EAX,dword ptr [EAX + 0x8]
1000fb81 MOV EDX,dword ptr [ESP + 0x44]
1000fb85 MOV dword ptr [ECX + 0x8],EAX
1000fb88 MOV ECX,dword ptr [ESP + 0x30]
1000fb8c PUSH ECX
1000fb8d PUSH EDX
1000fb8e LEA ECX,[ESP + 0x1e4]
1000fb95 CALL 0x1000e230
1000fb9a MOV EDX,dword ptr [EAX]
1000fb9c LEA ECX,[EBX + 0x48]
1000fb9f MOV dword ptr [EBX + 0x48],EDX
1000fba2 MOV EDX,dword ptr [EAX + 0x4]
1000fba5 MOV dword ptr [ECX + 0x4],EDX
1000fba8 MOV EAX,dword ptr [EAX + 0x8]
1000fbab MOV dword ptr [ECX + 0x8],EAX
1000fbae MOV ECX,EBX
1000fbb0 FLD float ptr [EDI]
1000fbb2 MOV EDX,dword ptr [ECX]
1000fbb4 MOV dword ptr [ESP + 0x4c],EDX
1000fbb8 MOV EAX,dword ptr [ECX + 0x4]
1000fbbb FCOMP float ptr [ESP + 0x4c]
1000fbbf MOV ECX,dword ptr [ECX + 0x8]
1000fbc2 MOV dword ptr [ESP + 0x50],EAX
1000fbc6 MOV dword ptr [ESP + 0x54],ECX
1000fbca LEA ECX,[EBX + 0x54]
1000fbcd MOV EDX,ECX
1000fbcf MOV EAX,dword ptr [EDX]
1000fbd1 MOV dword ptr [ESP + 0x58],EAX
1000fbd5 MOV EAX,dword ptr [EDX + 0x4]
1000fbd8 MOV dword ptr [ESP + 0x5c],EAX
1000fbdc MOV ESI,dword ptr [EDX + 0x8]
1000fbdf FNSTSW AX
1000fbe1 MOV dword ptr [ESP + 0x60],ESI
1000fbe5 TEST AH,0x1
1000fbe8 JZ 0x1000fbf0
1000fbea MOV EDX,dword ptr [EDI]
1000fbec MOV dword ptr [ESP + 0x4c],EDX
1000fbf0 FLD float ptr [EDI + 0x4]
1000fbf3 FCOMP float ptr [ESP + 0x50]
1000fbf7 FNSTSW AX
1000fbf9 TEST AH,0x1
1000fbfc JZ 0x1000fc05
1000fbfe MOV EAX,dword ptr [EDI + 0x4]
1000fc01 MOV dword ptr [ESP + 0x50],EAX
1000fc05 FLD float ptr [EDI + 0x8]
1000fc08 FCOMP float ptr [ESP + 0x54]
1000fc0c FNSTSW AX
1000fc0e TEST AH,0x1
1000fc11 JZ 0x1000fc1a
1000fc13 MOV EDX,dword ptr [EDI + 0x8]
1000fc16 MOV dword ptr [ESP + 0x54],EDX
1000fc1a FLD float ptr [EDI]
1000fc1c FCOMP float ptr [ESP + 0x58]
1000fc20 FNSTSW AX
1000fc22 TEST AH,0x41
1000fc25 JNZ 0x1000fc2d
1000fc27 MOV EAX,dword ptr [EDI]
1000fc29 MOV dword ptr [ESP + 0x58],EAX
1000fc2d FLD float ptr [EDI + 0x4]
1000fc30 FCOMP float ptr [ESP + 0x5c]
1000fc34 FNSTSW AX
1000fc36 TEST AH,0x41
1000fc39 JNZ 0x1000fc42
1000fc3b MOV EDX,dword ptr [EDI + 0x4]
1000fc3e MOV dword ptr [ESP + 0x5c],EDX
1000fc42 FLD float ptr [EDI + 0x8]
1000fc45 FCOMP float ptr [ESP + 0x60]
1000fc49 FNSTSW AX
1000fc4b TEST AH,0x41
1000fc4e JNZ 0x1000fc59
1000fc50 MOV EAX,dword ptr [EDI + 0x8]
1000fc53 MOV dword ptr [ESP + 0x60],EAX
1000fc57 MOV ESI,EAX
1000fc59 FLD float ptr [ESP + 0x4c]
1000fc5d FSTP float ptr [ESP + 0x90]
1000fc64 FLD float ptr [ESP + 0x50]
1000fc68 FSTP float ptr [ESP + 0x94]
1000fc6f FLD float ptr [ESP + 0x54]
1000fc73 FSTP float ptr [ESP + 0x98]
1000fc7a FLD float ptr [ESP + 0x58]
1000fc7e FSTP float ptr [ESP + 0xa8]
1000fc85 FLD float ptr [ESP + 0x50]
1000fc89 MOV EAX,dword ptr [ESP + 0x90]
1000fc90 MOV EDX,EBX
1000fc92 FSTP float ptr [ESP + 0xac]
1000fc99 FLD float ptr [ESP + 0x54]
1000fc9d FSTP float ptr [ESP + 0xb0]
1000fca4 FLD float ptr [ESP + 0x58]
1000fca8 MOV dword ptr [EDX],EAX
1000fcaa MOV EAX,dword ptr [ESP + 0x94]
1000fcb1 FSTP float ptr [ESP + 0xc0]
1000fcb8 FLD float ptr [ESP + 0x5c]
1000fcbc MOV dword ptr [EDX + 0x4],EAX
1000fcbf MOV EAX,dword ptr [ESP + 0x98]
1000fcc6 FSTP float ptr [ESP + 0xc4]
1000fccd FLD float ptr [ESP + 0x54]
1000fcd1 MOV dword ptr [EDX + 0x8],EAX
1000fcd4 MOV EAX,dword ptr [ESP + 0xa8]
1000fcdb LEA EDX,[EBX + 0xc]
1000fcde MOV EDI,dword ptr [ESP + 0x4c]
1000fce2 FSTP float ptr [ESP + 0xc8]
1000fce9 FLD float ptr [ESP + 0x58]
1000fced MOV dword ptr [EDX],EAX
1000fcef MOV EAX,dword ptr [ESP + 0xac]
1000fcf6 FSTP float ptr [ESP + 0xb4]
1000fcfd MOV dword ptr [EDX + 0x4],EAX
1000fd00 MOV EAX,dword ptr [ESP + 0xb0]
1000fd07 FLD float ptr [ESP + 0x5c]
1000fd0b MOV dword ptr [EDX + 0x8],EAX
1000fd0e MOV EAX,dword ptr [ESP + 0xc0]
1000fd15 LEA EDX,[EBX + 0x18]
1000fd18 PUSH ESI
1000fd19 FSTP float ptr [ESP + 0xbc]
1000fd20 FLD float ptr [ESP + 0x64]
1000fd24 MOV dword ptr [EDX],EAX
1000fd26 MOV EAX,dword ptr [ESP + 0xc8]
1000fd2d FSTP float ptr [ESP + 0xc0]
1000fd34 MOV dword ptr [EDX + 0x4],EAX
1000fd37 MOV EAX,dword ptr [ESP + 0xcc]
1000fd3e MOV dword ptr [EDX + 0x8],EAX
1000fd41 MOV EDX,dword ptr [ESP + 0xb8]
1000fd48 MOV EAX,dword ptr [ESP + 0xbc]
1000fd4f MOV dword ptr [ECX],EDX
1000fd51 MOV EDX,dword ptr [ESP + 0xc0]
1000fd58 MOV dword ptr [ECX + 0x4],EAX
1000fd5b MOV EAX,dword ptr [ESP + 0x60]
1000fd5f MOV dword ptr [ECX + 0x8],EDX
1000fd62 PUSH EAX
1000fd63 PUSH EDI
1000fd64 LEA ECX,[ESP + 0x19c]
1000fd6b CALL 0x1000e230
1000fd70 MOV EDX,dword ptr [EAX]
1000fd72 LEA ECX,[EBX + 0x30]
1000fd75 PUSH ESI
1000fd76 MOV dword ptr [ECX],EDX
1000fd78 MOV EDX,dword ptr [EAX + 0x4]
1000fd7b MOV dword ptr [ECX + 0x4],EDX
1000fd7e MOV EAX,dword ptr [EAX + 0x8]
1000fd81 MOV dword ptr [ECX + 0x8],EAX
1000fd84 MOV ECX,dword ptr [ESP + 0x54]
1000fd88 PUSH ECX
1000fd89 PUSH EDI
1000fd8a LEA ECX,[ESP + 0x1a8]
1000fd91 CALL 0x1000e230
1000fd96 MOV ECX,dword ptr [EAX]
1000fd98 LEA EDX,[EBX + 0x3c]
1000fd9b MOV dword ptr [EBX + 0x3c],ECX
1000fd9e MOV ECX,dword ptr [EAX + 0x4]
1000fda1 MOV dword ptr [EDX + 0x4],ECX
1000fda4 MOV EAX,dword ptr [EAX + 0x8]
1000fda7 MOV ECX,dword ptr [ESP + 0x54]
1000fdab MOV dword ptr [EDX + 0x8],EAX
1000fdae MOV EDX,dword ptr [ESP + 0x5c]
1000fdb2 PUSH ECX
1000fdb3 PUSH EDX
1000fdb4 PUSH EDI
1000fdb5 LEA ECX,[ESP + 0x1c0]
1000fdbc CALL 0x1000e230
1000fdc1 MOV EDX,dword ptr [EAX]
1000fdc3 LEA ECX,[EBX + 0x24]
1000fdc6 PUSH ESI
1000fdc7 MOV dword ptr [ECX],EDX
1000fdc9 MOV EDX,dword ptr [EAX + 0x4]
1000fdcc MOV dword ptr [ECX + 0x4],EDX
1000fdcf MOV EAX,dword ptr [EAX + 0x8]
1000fdd2 MOV EDX,dword ptr [ESP + 0x5c]
1000fdd6 MOV dword ptr [ECX + 0x8],EAX
1000fdd9 MOV ECX,dword ptr [ESP + 0x54]
1000fddd PUSH ECX
1000fdde PUSH EDX
1000fddf LEA ECX,[ESP + 0x1d8]
1000fde6 CALL 0x1000e230
1000fdeb MOV EDX,dword ptr [EAX]
1000fded LEA ECX,[EBX + 0x48]
1000fdf0 MOV dword ptr [EBX + 0x48],EDX
1000fdf3 MOV EDX,dword ptr [EAX + 0x4]
1000fdf6 MOV dword ptr [ECX + 0x4],EDX
1000fdf9 MOV EAX,dword ptr [EAX + 0x8]
1000fdfc MOV dword ptr [ECX + 0x8],EAX
1000fdff MOV ECX,dword ptr [ESP + 0x10]
1000fe03 MOV EAX,dword ptr [ESP + 0x18]
1000fe07 MOV EDX,dword ptr [EBP + 0x28c]
1000fe0d INC ECX
1000fe0e ADD EAX,0x1c
1000fe11 CMP ECX,EDX
1000fe13 MOV dword ptr [ESP + 0x10],ECX
1000fe17 MOV dword ptr [ESP + 0x18],EAX
1000fe1b JC 0x1000f687
1000fe21 MOV ECX,dword ptr [EBP + 0x7b60]
1000fe27 MOV ESI,dword ptr [ECX + 0x24]
1000fe2a LEA EDX,[ESI + ESI*0x2]
1000fe2d LEA EAX,[EDX*0x4 + 0x4]
1000fe34 PUSH EAX
1000fe35 CALL 0x100597e7
1000fe3a ADD ESP,0x4
1000fe3d MOV dword ptr [ESP + 0x10],EAX
1000fe41 XOR EBX,EBX
1000fe43 MOV byte ptr [ESP + 0x1f8],0x7
1000fe4b CMP EAX,EBX
1000fe4d JZ 0x1000fe69
1000fe4f PUSH 0x10044900
1000fe54 PUSH 0x10015d00
1000fe59 LEA EDI,[EAX + 0x4]
1000fe5c PUSH ESI
1000fe5d PUSH 0xc
1000fe5f PUSH EDI
1000fe60 MOV dword ptr [EAX],ESI
1000fe62 CALL 0x10059d5f
1000fe67 JMP 0x1000fe6b
1000fe69 XOR EDI,EDI
1000fe6b MOV ECX,dword ptr [EBP + 0x7b60]
1000fe71 MOV dword ptr [EBP + 0x7b64],EDI
1000fe77 MOV byte ptr [ESP + 0x1f8],0x6
1000fe7f MOV ESI,dword ptr [ECX + 0x24]
1000fe82 LEA EDX,[ESI + ESI*0x2]
1000fe85 LEA EAX,[EDX*0x4 + 0x4]
1000fe8c PUSH EAX
1000fe8d CALL 0x100597e7
1000fe92 ADD ESP,0x4
1000fe95 MOV dword ptr [ESP + 0x10],EAX
1000fe99 CMP EAX,EBX
1000fe9b MOV byte ptr [ESP + 0x1f8],0x8
1000fea3 JZ 0x1000febf
1000fea5 PUSH 0x10044900
1000feaa PUSH 0x10015c50
1000feaf LEA EDI,[EAX + 0x4]
1000feb2 PUSH ESI
1000feb3 PUSH 0xc
1000feb5 PUSH EDI
1000feb6 MOV dword ptr [EAX],ESI
1000feb8 CALL 0x10059d5f
1000febd JMP 0x1000fec1
1000febf XOR EDI,EDI
1000fec1 MOV ECX,dword ptr [EBP + 0x7b60]
1000fec7 MOV dword ptr [EBP + 0x7b70],EDI
1000fecd MOV byte ptr [ESP + 0x1f8],0x6
1000fed5 MOV ESI,dword ptr [ECX + 0x24]
1000fed8 LEA EDX,[ESI + ESI*0x2]
1000fedb LEA EAX,[EDX*0x4 + 0x4]
1000fee2 PUSH EAX
1000fee3 CALL 0x100597e7
1000fee8 ADD ESP,0x4
1000feeb MOV dword ptr [ESP + 0x10],EAX
1000feef CMP EAX,EBX
1000fef1 MOV byte ptr [ESP + 0x1f8],0x9
1000fef9 JZ 0x1000ff17
1000fefb PUSH 0x10044900
1000ff00 PUSH 0x10015d30
1000ff05 LEA EDI,[EAX + 0x4]
1000ff08 PUSH ESI
1000ff09 PUSH 0xc
1000ff0b PUSH EDI
1000ff0c MOV dword ptr [EAX],ESI
1000ff0e CALL 0x10059d5f
1000ff13 MOV EAX,EDI
1000ff15 JMP 0x1000ff19
1000ff17 XOR EAX,EAX
1000ff19 MOV dword ptr [EBP + 0x7b68],EAX
1000ff1f LEA EAX,[EBP + 0x7b3c]
1000ff25 MOV EDX,dword ptr [EBP + 0x7be4]
1000ff2b MOV dword ptr [EBP + 0x194],EAX
1000ff31 LEA EAX,[EBP + 0x7b48]
1000ff37 MOV dword ptr [EBP + 0x1c4],EDX
1000ff3d MOV dword ptr [EBP + 0x198],EAX
1000ff43 LEA EAX,[EBP + 0x7c54]
1000ff49 MOV dword ptr [EBP + 0x160],EAX
1000ff4f LEA EAX,[EBP + 0x7c5c]
1000ff55 MOV dword ptr [EBP + 0x164],EAX
1000ff5b LEA EDX,[EBP + 0x7b54]
1000ff61 LEA EAX,[EBP + 0x7c6c]
1000ff67 MOV dword ptr [EBP + 0x19c],EDX
1000ff6d MOV dword ptr [EBP + 0x1c8],EAX
1000ff73 LEA EDX,[EBP + 0x7c64]
1000ff79 LEA EAX,[EBP + 0x7c74]
1000ff7f MOV dword ptr [EBP + 0x168],EDX
1000ff85 MOV dword ptr [EBP + 0x1cc],EAX
1000ff8b OR ECX,0xffffffff
1000ff8e LEA EDX,[EBP + 0x7c7c]
1000ff94 LEA EAX,[EBP + 0x7c48]
1000ff9a MOV dword ptr [EBP + 0x1a0],ECX
1000ffa0 MOV dword ptr [EBP + 0x1a8],EBX
1000ffa6 MOV dword ptr [EBP + 0x1d0],EDX
1000ffac MOV dword ptr [EBP + 0x170],EAX
1000ffb2 MOV dword ptr [EBP + 0x1dc],ECX
1000ffb8 MOV dword ptr [EBP + 0x1e4],ECX
1000ffbe MOV dword ptr [EBP + 0x1f4],EBX
1000ffc4 MOV dword ptr [EBP + 0x1e8],EBX
1000ffca MOV dword ptr [EBP + 0x1ec],0xfffffffe
1000ffd4 MOV dword ptr [EBP + 0x1f0],EBX
1000ffda MOV dword ptr [EBP + 0x1f8],EBX
1000ffe0 MOV dword ptr [EBP + 0x1fc],EBX
1000ffe6 MOV dword ptr [EBP + 0x200],EBX
1000ffec MOV dword ptr [EBP + 0x204],EBX
1000fff2 MOV EAX,dword ptr [EBP + 0x278]
1000fff8 MOV ESI,dword ptr [EBP + 0x28c]
1000fffe CMP EAX,EBX
10010000 MOV byte ptr [ESP + 0x1f8],0x6
10010008 MOV dword ptr [ESP + 0x18],ESI
1001000c JNZ 0x10010021
1001000e PUSH 0x1f40
10010013 CALL 0x100597e7
10010018 ADD ESP,0x4
1001001b MOV dword ptr [EBP + 0x278],EAX
10010021 MOV ECX,dword ptr [EBP + 0x27c]
10010027 LEA EAX,[ESI + ECX*0x1]
1001002a MOV ECX,dword ptr [EBP + 0x280]
10010030 CMP EAX,ECX
10010032 JLE 0x10010092
10010034 LEA EDX,[EAX*0x8 + 0x320]
1001003b PUSH EDX
1001003c CALL 0x100597e7
10010041 MOV ECX,dword ptr [EBP + 0x280]
10010047 MOV ESI,dword ptr [EBP + 0x278]
1001004d SHL ECX,0x3
10010050 MOV dword ptr [ESP + 0x14],EAX
10010054 MOV EDI,EAX
10010056 MOV EAX,ECX
10010058 SHR ECX,0x2
1001005b MOVSD.REP ES:EDI,ESI
1001005d MOV ECX,EAX
1001005f AND ECX,0x3
10010062 MOVSB.REP ES:EDI,ESI
10010064 MOV ECX,dword ptr [EBP + 0x278]
1001006a PUSH ECX
1001006b CALL 0x10059600
10010070 MOV EAX,dword ptr [EBP + 0x27c]
10010076 MOV ECX,dword ptr [ESP + 0x20]
1001007a MOV EDX,dword ptr [ESP + 0x18]
1001007e ADD ESP,0x8
10010081 ADD EAX,ECX
10010083 MOV dword ptr [EBP + 0x278],EDX
10010089 LEA EDX,[EAX + 0x64]
1001008c MOV dword ptr [EBP + 0x280],EDX
10010092 MOV dword ptr [EBP + 0x27c],EAX
10010098 MOV ECX,dword ptr [EBP + 0x28c]
1001009e XOR EAX,EAX
100100a0 CMP ECX,EBX
100100a2 JBE 0x100100d2
100100a4 XOR ECX,ECX
100100a6 MOV ESI,dword ptr [EBP + 0x2f4]
100100ac MOV EDX,ECX
100100ae ADD EDX,ESI
100100b0 MOV ESI,dword ptr [EBP + 0x278]
100100b6 ADD ECX,0x1c
100100b9 MOV dword ptr [ESI + EAX*0x8],EDX
100100bc MOV EDX,dword ptr [EBP + 0x278]
100100c2 MOV byte ptr [EDX + EAX*0x8 + 0x4],0x0
100100c7 MOV EDX,dword ptr [EBP + 0x28c]
100100cd INC EAX
100100ce CMP EAX,EDX
100100d0 JC 0x100100a6
100100d2 LEA EDI,[EBP + 0x5558]
100100d8 MOV ECX,0x19
100100dd XOR EAX,EAX
100100df STOSD.REP ES:EDI
100100e1 MOV dword ptr [EBP + 0x7c90],EBX
100100e7 CALL 0x100282c0
100100ec MOV ECX,dword ptr [ESP + 0x1f0]
100100f3 MOV dword ptr [EBP + 0x7c18],EAX
100100f9 POP EDI
100100fa MOV dword ptr [EBP + 0x7c20],EBX
10010100 MOV dword ptr [EBP + 0x7c24],0x10
1001010a MOV dword ptr [EBP + 0x7c94],0x2
10010114 MOV EAX,EBP
10010116 POP ESI
10010117 POP EBP
10010118 POP EBX
10010119 MOV dword ptr FS:[0x0],ECX
10010120 ADD ESP,0x1ec
10010126 RET 0x10