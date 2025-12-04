
/* returns index into g_MaterialGlobalDescriptors  */

int __fastcall LoadMaterialByName(char *itemname)

{
  char *pcVar1;
  undefined1 *puVar2;
  byte *pbVar3;
  uint uVar4;
  undefined4 *puVar5;
  int iVar6;
  int iVar7;
  MaterialGlobalDescriptor_ptr_4_undefined4 piVar8;
  nres_metadata_item *pnVar8;
  int iVar9;
  MaterialStageWorldDllInternal *pMVar10;
  int iVar11;
  ITexture **ppIVar12;
  MaterialAnimationKey *pMVar13;
  MaterialGlobalDescriptor *pMVar14;
  float unaff_EBX;
  MaterialStageExternal *pfVar16;
  ushort unaff_DI;
  MaterialAnimation *pMVar15;
  uint uVar16;
  int iStack_90;
  uint uStack_8c;
  char acStack_88 [8];
  int iStack_80;
  char acStack_58 [88];
  
  iVar6 = (*(*g_material_resfile)->get_index_in_file_by_itemname)(g_material_resfile,itemname);
  if (iVar6 < 0) {
    sprintf(acStack_58,s_Material_%s_not_found._100246f8,itemname);
                    /* WARNING: Subroutine does not return */
    write_error_to_file_and_msgbox(acStack_58,(void *)0x0);
  }
  iVar7 = 0;
  if (0 < g_MaterialGlobalDescriptor_count) {
    pMVar14 = g_MaterialGlobalDescriptors;
    do {
      if (pMVar14->index_in_file == iVar6) {
        if (-1 < iVar7) {
          g_MaterialGlobalDescriptors[iVar7].RefCount =
               g_MaterialGlobalDescriptors[iVar7].RefCount + 1;
          return iVar7;
        }
        break;
      }
      iVar7 += 1;
      pMVar14 = pMVar14 + 1;
    } while (iVar7 < g_MaterialGlobalDescriptor_count);
  }
  iVar7 = 0;
  if (0 < g_MaterialGlobalDescriptor_count) {
    piVar8 = &g_MaterialGlobalDescriptors[0].RefCount;
    do {
      if (ADJ(piVar8)->RefCount == 0) break;
      iVar7 += 1;
      piVar8 = piVar8 + 0x5c;
    } while (iVar7 < g_MaterialGlobalDescriptor_count);
  }
  if (iVar7 == g_MaterialGlobalDescriptor_count) {
    g_MaterialGlobalDescriptor_count += 1;
  }
  iStack_80 = iVar7;
  g_currentMaterialFileData_ptr =
       (*(*g_material_resfile)->get_item_data_ptr_by_index)(g_material_resfile,iVar6,1);
  pnVar8 = (*(*g_material_resfile)->get_metadata_ptr)(g_material_resfile);
  uVar16 = 0;
  uVar4 = pnVar8[iVar6].magic1;
  if ((pnVar8[iVar6].element_count_or_version & 1) != 0) {
    uVar16 = 0x200000;
  }
  g_MaterialGlobalDescriptors[iVar7].extra_meta.field0_0x0 = 0;
  g_MaterialGlobalDescriptors[iVar7].extra_meta.field2_0x8 = 0;
  g_MaterialGlobalDescriptors[iVar7].extra_meta.field1_0x4 =
       pnVar8[iVar6].element_count_or_version >> 2 & 0xf;
  iVar9 = g_bumpmapping_enabled_value;
  if (((pnVar8[iVar6].element_count_or_version & 2) != 0) &&
     ((g_MaterialGlobalDescriptors[iVar7].extra_meta.field0_0x0 = 1, iVar9 == 0 ||
      (g_supports_texture_mode_6 == 0)))) {
    uVar16 |= 0x80000;
  }
  if ((pnVar8[iVar6].element_count_or_version & 0x40) != 0) {
    g_MaterialGlobalDescriptors[iVar7].extra_meta.field2_0x8 = 1;
  }
  iVar9 = 0;
  g_MaterialGlobalDescriptors[iVar7].RefCount = g_MaterialGlobalDescriptors[iVar7].RefCount + 1;
  puVar5 = g_currentMaterialFileData_ptr;
  g_MaterialGlobalDescriptors[iVar7].index_in_file = iVar6;
  iVar6 = 0;
  do {
    pcVar1 = (char *)(iVar9 + (int)puVar5);
    iVar9 += 1;
    acStack_88[iVar6 + -0x1c] = *pcVar1;
    iVar6 += 1;
  } while (iVar6 < 2);
  g_MaterialGlobalDescriptors[iVar7].stageCount = (uint)unaff_DI;
  iVar6 = 0;
  do {
    iVar11 = iVar9;
    iVar9 = iVar11 + 1;
    acStack_88[iVar6 + -0x1c] = *(char *)(iVar11 + (int)puVar5);
    iVar6 += 1;
  } while (iVar6 < 2);
  DAT_10128674 = iVar9;
  g_MaterialGlobalDescriptors[iVar7].animCount = (uint)unaff_DI;
  if (0x13 < unaff_DI) {
                    /* WARNING: Subroutine does not return */
    write_error_to_file_and_msgbox(s_Too_many_animations_for_material_100246cc,(void *)0x0);
  }
  g_MaterialGlobalDescriptors[iVar7].field8_0x15c = 1.0;
  g_MaterialGlobalDescriptors[iVar7].field9_0x160 = 0;
  if (uVar4 < 2) {
    g_MaterialGlobalDescriptors[iVar7].field6_0x154 = 0xff;
    g_MaterialGlobalDescriptors[iVar7].field7_0x158 = 0xff;
  }
  else {
    g_MaterialGlobalDescriptors[iVar7].field6_0x154 = (uint)*(byte *)(iVar9 + (int)puVar5);
    iVar6 = iVar11 + 3;
    DAT_10128674 = iVar6;
    g_MaterialGlobalDescriptors[iVar7].field7_0x158 = (uint)*(byte *)(iVar11 + 2 + (int)puVar5);
    if (2 < uVar4) {
      iVar9 = 0;
      do {
        puVar2 = (undefined1 *)(iVar6 + (int)puVar5);
        iVar6 += 1;
        (&stack0xffffff68)[iVar9] = *puVar2;
        iVar9 += 1;
      } while (iVar9 < 4);
      DAT_10128674 = iVar6;
      g_MaterialGlobalDescriptors[iVar7].field8_0x15c = unaff_EBX;
      if (3 < uVar4) {
        iVar9 = 0;
        do {
          puVar2 = (undefined1 *)(iVar6 + (int)puVar5);
          iVar6 += 1;
          (&stack0xffffff68)[iVar9] = *puVar2;
          iVar9 += 1;
        } while (iVar9 < 4);
        DAT_10128674 = iVar6;
        g_MaterialGlobalDescriptors[iVar7].field9_0x160 = unaff_EBX;
      }
    }
  }
  pMVar10 = (MaterialStageWorldDllInternal *)
            _malloc(g_MaterialGlobalDescriptors[iVar7].stageCount * 0x4c);
  g_MaterialGlobalDescriptors[iVar7].stages = pMVar10;
  iVar6 = 0;
  if (0 < g_MaterialGlobalDescriptors[iVar7].stageCount) {
    iVar9 = 0;
    do {
      pfVar16 = (MaterialStageExternal *)
                ((int)&((g_MaterialGlobalDescriptors[iVar7].stages)->diffuse).R + iVar9);
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->ambient).R = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->ambient).G = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->ambient).B = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->ambient).A = (float)*pbVar3 * 0.01;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->diffuse).R = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->diffuse).G = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->diffuse).B = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->diffuse).A = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->specular).R = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->specular).G = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->specular).B = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->specular).A = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->emissive).R = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->emissive).G = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->emissive).B = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      (pfVar16->emissive).A = (float)*pbVar3 * 1/255f;
      pbVar3 = (byte *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 += 1;
      pfVar16->Power = (float)(uint)*pbVar3;
      pcVar1 = (char *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
      DAT_10128674 = DAT_10128674 + 1;
      pfVar16->current_m_LL_ITexture = (ITexture **)(int)*pcVar1;
      iVar11 = 0;
      do {
        pcVar1 = (char *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
        DAT_10128674 += 1;
        acStack_88[iVar11] = *pcVar1;
        iVar11 += 1;
      } while (iVar11 < 0x10);
      if (acStack_88[0] == '\0') {
        pfVar16->m_LL_ITexture = (ITexture **)0xffffffff;
        pfVar16->current_m_LL_ITexture = (ITexture **)0xffffffff;
      }
      else {
        ppIVar12 = (ITexture **)LoadAndCacheTexture(acStack_88,uVar16);
        pfVar16->m_LL_ITexture = ppIVar12;
      }
      iVar6 += 1;
      iVar9 += 0x4c;
    } while (iVar6 < g_MaterialGlobalDescriptors[iVar7].stageCount);
  }
  iVar6 = 0;
  if (0 < g_MaterialGlobalDescriptors[iVar7].animCount) {
    pMVar15 = g_MaterialGlobalDescriptors[iVar7].animations;
    do {
      puVar5 = g_currentMaterialFileData_ptr;
      iVar9 = 0;
      do {
        pcVar1 = (char *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
        DAT_10128674 += 1;
        acStack_88[iVar9 + -4] = *pcVar1;
        iVar9 += 1;
      } while (iVar9 < 4);
      pMVar15->FieldSelector = (int)uStack_8c >> 3;
      pMVar15->loop_mode = uStack_8c & 7;
      iVar9 = 0;
      do {
        pcVar1 = (char *)(DAT_10128674 + (int)puVar5);
        DAT_10128674 += 1;
        acStack_88[iVar9 + -0x1c] = *pcVar1;
        iVar9 += 1;
      } while (iVar9 < 2);
      pMVar15->keyCount = (uint)unaff_DI;
      pMVar13 = (MaterialAnimationKey *)_malloc((uint)unaff_DI << 3);
      pMVar15->keys = pMVar13;
      iVar9 = 0;
      if (0 < (int)pMVar15->keyCount) {
        do {
          iVar11 = 0;
          do {
            pcVar1 = (char *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
            DAT_10128674 += 1;
            acStack_88[iVar11 + -0x1c] = *pcVar1;
            iVar11 += 1;
          } while (iVar11 < 2);
          pMVar15->keys[iVar9].stage_index = (uint)unaff_DI;
          iVar11 = 0;
          do {
            pcVar1 = (char *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
            DAT_10128674 += 1;
            acStack_88[iVar11 + -0x1c] = *pcVar1;
            iVar11 += 1;
          } while (iVar11 < 2);
          pMVar15->keys[iVar9].duration_ms = unaff_DI;
          iVar11 = 0;
          do {
            pcVar1 = (char *)(DAT_10128674 + (int)g_currentMaterialFileData_ptr);
            DAT_10128674 += 1;
            acStack_88[iVar11 + -0x1c] = *pcVar1;
            iVar11 += 1;
          } while (iVar11 < 2);
          pMVar15->keys[iVar9].field2_0x6 = unaff_DI;
          iVar9 += 1;
        } while (iVar9 < (int)pMVar15->keyCount);
      }
      iVar6 += 1;
      pMVar15 = pMVar15 + 1;
    } while (iVar6 < g_MaterialGlobalDescriptors[iVar7].animCount);
  }
  return iStack_90;
}

