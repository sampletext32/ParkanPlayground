// Decompiled C Code:

/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */
/* WARNING: Enum "DRAW_PRIMITIVE_FLAGS": Some values do not have unique names */
/* WARNING: Exceeded maximum restarts with more pending */

void IMesh2_of_CLandscape::Render
               (IMesh2_of_CLandscape this,CBufferingCamera *camera,uint param_3,int **param_4)

{
  MshMetaForLandscape *pMVar1;
  uint uVar2;
  ICPrimBuffer *usePass2;
  bool bVar3;
  CShade *pCVar4;
  int iVar5;
  CameraFrustum *this_00;
  CShade *pCVar6;
  ENewLogicID iVar8;
  int iVar7;
  int *piVar8;
  uint *puVar9;
  undefined4 *puVar10;
  float *pfVar11;
  CShade **ppCVar12;
  undefined4 extraout_ECX;
  IMesh2_of_CLandscape this_01;
  int iVar13;
  Vector3 *pVVar14;
  int iVar15;
  float *pfVar16;
  Plane *pPVar17;
  float10 fVar18;
  RECT *viewportOverride;
  char cVar19;
  ClipPlanes_WeirdAllignment **outPlanes;
  Vector3 **outVertices;
  undefined4 uVar20;
  MSH_02_element *pMVar21;
  CPrimBuffer *pCVar22;
  CPrimitiveContents *primitiveContents;
  CPrimBuffer **ppCVar23;
  CShade *pCVar24;
  CPrimBuffer *pCStack_6f4;
  CShade *pCStack_6f0;
  CShade *pCStack_6ec;
  CShade *pCStack_6e8;
  CPrimBuffer *pCStack_6e4;
  undefined1 *puStack_6e0;
  int iStack_6dc;
  Vector3 *pVStack_6d8;
  IShade *pIStack_6d4;
  IBufferingCamera **piStack_6d0;
  IGameObject ***ppIStack_6cc;
  CShade *pCStack_6c8;
  IGameObject **ppIStack_6c4;
  CShade *pCStack_6c0;
  int iStack_6bc;
  CShade *pCStack_6b8;
  ClipPlanes_WeirdAllignment *pCStack_6b4;
  GetFrustumLayerConfig get_frustum_layer_config;
  CPrimBuffer *pCStack_6a8;
  undefined1 uStack_6a4;
  int iStack_6a0;
  int iStack_69c;
  int iStack_698;
  int iStack_694;
  float afStack_690 [5];
  undefined4 uStack_67c;
  undefined4 uStack_670;
  undefined4 uStack_664;
  undefined4 uStack_658;
  undefined4 uStack_64c;
  undefined4 uStack_640;
  undefined4 uStack_634;
  float afStack_630 [24];
  undefined1 auStack_5d0 [96];
  CameraFrustum camera_frustum;
  undefined4 uStack_4;
  
  if (ADJ(this)->field9833_0x7c20 == 0) {
    ADJ(this)->field9833_0x7c20 = 1;
    ppIStack_6c4 = (IGameObject **)(param_3 & 0x40);
    if ((ppIStack_6c4 == (IGameObject **)0x0) && (iVar13 = 0, 0 < (ADJ(this)->array_0x7a40).count))
    {
      do {
        iVar5 = (*(code *)**(undefined4 **)(ADJ(this)->array_0x7a40).data[iVar13])();
        if (iVar5 != 0) {
          (**(code **)(*(int *)pIStack_6d4 + 0x2c))();
        }
        iVar13 = iVar13 + 1;
      } while (iVar13 < (ADJ(this)->array_0x7a40).count);
    }
    (ADJ(this)->array_0x7c84).count = 0;
    ppIStack_6cc = (IGameObject ***)(uint)((param_3 & 9) == 0);
    pIStack_6d4 = ADJ(this)->Shade->vptr;
    iVar13 = (*pIStack_6d4->CollectVisiblePrimitivesAndGetCount)
                       (ADJ(this)->Shade,
                        (IComponent **)(-(uint)(this != (IMesh2_of_CLandscape)0x138) & (uint)this),
                        (uint)camera,(ICamera ***)0x0,&ADJ(this)->meshes,&ADJ(this)->game_objects);
    ADJ(this)->current_visible_primitives_count = iVar13;
    (*ADJ(this)->Shade->vptr->StartMeshRender)
              (ADJ(this)->Shade,camera,
               (IMesh2 **)(-(uint)(this != (IMesh2_of_CLandscape)0x138) & (uint)this),
               ADJ(this)->IMatManager,0);
    (*(camera->vptr->IComponent).QueryInterface)(camera,IBufferingCamera,&piStack_6d0);
    outVertices = &pVStack_6d8;
    outPlanes = &pCStack_6b4;
    primitiveContents = (CPrimitiveContents *)&DAT_100708d0;
    get_frustum_layer_config.usePass2 = 0;
    get_frustum_layer_config.index = 0;
    viewportOverride = (RECT *)0x0;
    this_00 = (CameraFrustum *)
              (*(*piStack_6d0)->GetFrustum)
                        (piStack_6d0,(BOOL)&camera_frustum,(int)&get_frustum_layer_config);
    CameraFrustum::method_58ed0(this_00,viewportOverride,outPlanes,outVertices,primitiveContents);
    iVar13 = 5;
    puStack_6e0 = &DAT_7f7fffff;
    pCStack_6e4 = (CPrimBuffer *)&DAT_7f7fffff;
    pCVar24 = (CShade *)0x800000;
    pCStack_6e8 = (CShade *)0x800000;
    pCVar6 = (CShade *)ppIStack_6cc;
    pVVar14 = pVStack_6d8;
    do {
      if (pVVar14->x < (float)pCStack_6e4) {
        pCStack_6e4 = (CPrimBuffer *)pVVar14->x;
        fVar18 = floor((double)((pVVar14->x - (float)ADJ(this)->mshmeta_ptr->min_x) *
                               ADJ(this)->mshmeta_ptr->inv_width));
        pCVar6 = (CShade *)round((double)fVar18);
      }
      if (pVVar14->y < (float)puStack_6e0) {
        puStack_6e0 = (undefined1 *)pVVar14->y;
        fVar18 = floor((double)((pVVar14->y - (float)ADJ(this)->mshmeta_ptr->min_y) *
                               ADJ(this)->mshmeta_ptr->inv_height));
        pCStack_6ec = (CShade *)round((double)fVar18);
      }
      if ((float)pCStack_6e8 < pVVar14->x) {
        pCStack_6e8 = (CShade *)pVVar14->x;
        fVar18 = floor((double)((pVVar14->x - (float)ADJ(this)->mshmeta_ptr->min_x) *
                               ADJ(this)->mshmeta_ptr->inv_width));
        pCStack_6f4 = (CPrimBuffer *)round((double)fVar18);
      }
      if ((float)pCVar24 < pVVar14->y) {
        pCVar24 = (CShade *)pVVar14->y;
        fVar18 = floor((double)((pVVar14->y - (float)ADJ(this)->mshmeta_ptr->min_y) *
                               ADJ(this)->mshmeta_ptr->inv_height));
        pCStack_6f0 = (CShade *)round((double)fVar18);
      }
      pVVar14 = pVVar14 + 1;
      iVar13 = iVar13 + -1;
    } while (iVar13 != 0);
    if ((int)pCVar6 < 0) {
      pCVar6 = (CShade *)0x0;
    }
    pMVar1 = ADJ(this)->mshmeta_ptr;
    iVar13 = pMVar1->_1_magic1;
    if (iVar13 <= (int)pCVar6) {
      pCVar6 = (CShade *)(iVar13 + -1);
    }
    if ((int)pCStack_6ec < 0) {
      pCStack_6ec = (CShade *)0x0;
    }
    uVar2 = pMVar1->_1_element_count_magic1;
    if ((int)uVar2 <= (int)pCStack_6ec) {
      pCStack_6ec = (CShade *)(uVar2 - 1);
    }
    if ((int)pCStack_6f4 < 0) {
      pCStack_6f4 = (CPrimBuffer *)0x0;
    }
    if (iVar13 <= (int)pCStack_6f4) {
      pCStack_6f4 = (CPrimBuffer *)(iVar13 + -1);
    }
    if ((int)pCStack_6f0 < 0) {
      pCStack_6f0 = (CShade *)0x0;
    }
    if ((int)uVar2 <= (int)pCStack_6f0) {
      pCStack_6f0 = (CShade *)(uVar2 - 1);
    }
    iVar5 = ((int)pCStack_6f0 + (1 - (int)pCStack_6ec)) * ((int)pCStack_6f4 + (1 - (int)pCVar6));
    fVar18 = floor((double)((pVStack_6d8->x - (float)pMVar1->min_x) * pMVar1->inv_width));
    iVar13 = round((double)fVar18);
    iStack_6a0 = iVar13;
    fVar18 = floor((double)((pVStack_6d8->y - (float)ADJ(this)->mshmeta_ptr->min_y) *
                           ADJ(this)->mshmeta_ptr->inv_height));
    iStack_69c = round((double)fVar18);
    iStack_698 = ADJ(this)->mshmeta_ptr->_1_element_count_magic1 * iVar13 + iStack_69c;
    if ((int)pCStack_6f4 - iVar13 < iVar13 - (int)pCVar6) {
      iStack_6bc = -1;
      pCVar6 = (CShade *)pCStack_6f4;
    }
    else {
      iStack_6bc = 1;
    }
    if ((int)pCStack_6f0 - iStack_69c < iStack_69c - (int)pCStack_6ec) {
      puStack_6e0 = (undefined1 *)0xffffffff;
      pCStack_6e8 = pCStack_6ec;
      pCStack_6ec = pCStack_6f0;
    }
    else {
      puStack_6e0 = (undefined1 *)0x1;
      pCStack_6e8 = pCStack_6f0;
    }
    pCStack_6f0 = pCStack_6ec;
    pCStack_6c0 = pCStack_6ec;
    if ((DAT_100708cc & 1) == 0) {
      DAT_100708cc = DAT_100708cc | 1;
      pCStack_6ec = pCVar6;
      FUN_100156f0((int *)&DAT_100708c0);
      atexit(&LAB_10011fd0);
      pCVar6 = pCStack_6ec;
    }
    pCStack_6ec = pCVar6;
    if (DAT_100708c4 != 0) {
      FUN_10015d60((undefined4 *)DAT_100708c0->IShadowProcessor);
      DAT_100708c0->IShadeStatsBuilder = (IShadeStatsBuilder *)DAT_100708c0;
      DAT_100708c0->IShadowProcessor = (IShadowProcessor *)0x0;
      DAT_100708c0->activeICamera = (ICamera **)DAT_100708c0;
      DAT_100708c4 = 0;
    }
    iVar13 = (*((ADJ(this)->game_object).vptr)->QueryChildren)
                       (&ADJ(this)->game_object,&ppIStack_6c4);
    iVar15 = 0;
    if (0 < iVar13) {
      do {
        iVar8 = (**(code **)((ppIStack_6c4[iVar15]->IComponent).QueryInterface + 0x2c))();
        if ((iVar8 == BULL_Agent) &&
           (iVar7 = (**(code **)(ppIStack_6c4[iVar15]->IComponent).QueryInterface)(), iVar7 != 0)) {
          piVar8 = (int *)FUN_10038580(&DAT_100708c0,(int *)&pCStack_6b8,(uint *)&pCStack_6f4);
          pCVar6 = (CShade *)*piVar8;
          if ((pCVar6 == DAT_100708c0) || (pCStack_6f4 < pCVar6->PrimBufferOfBufferingCamera)) {
            puVar9 = FUN_10015900(&pCStack_6a8,&pCStack_6f4,&stack0xfffff906);
            uVar20 = extraout_ECX;
            FUN_10037e80(&stack0xfffff8ec,(undefined4 *)&stack0xfffff908);
            puVar10 = (undefined4 *)FUN_100158b0(&DAT_100708c0,&pCStack_6c8,uVar20,puVar9);
            pCVar6 = (CShade *)*puVar10;
          }
          *(undefined1 *)&(pCVar6->m_CameraFrustum).cameraPosMatrixPtr = 0;
        }
        iVar15 = iVar15 + 1;
      } while (iVar15 < iVar13);
    }
    if (0 < iVar5) {
      pCStack_6c8 = (CShade *)
                    ((int)(pCStack_6e8->m_CameraFrustum).pad_004 + (int)puStack_6e0 + -0x18);
      iStack_6dc = iVar5;
      do {
        iVar5 = (int)(pCStack_6f0->m_CameraFrustum).pad_004 +
                ADJ(this)->mshmeta_ptr->_1_element_count_magic1 * (int)pCStack_6ec + -0x18;
        pVVar14 = &ADJ(this)->msh_02_data_ptr->elements
                   [*(ushort *)(ADJ(this)->_1_data_ptr + 8 + iVar5 * 0x26)].local_minimum;
        pfVar11 = (float *)FUN_10015580(auStack_5d0,&pVVar14->x,&pVVar14[1].x);
        pfVar16 = afStack_630;
        for (iVar13 = 0x18; iVar13 != 0; iVar13 = iVar13 + -1) {
          *pfVar16 = *pfVar11;
          pfVar11 = pfVar11 + 1;
          pfVar16 = pfVar16 + 1;
        }
        pfVar11 = afStack_630;
        pfVar16 = afStack_690;
        for (iVar13 = 0x18; iVar13 != 0; iVar13 = iVar13 + -1) {
          *pfVar16 = *pfVar11;
          pfVar11 = pfVar11 + 1;
          pfVar16 = pfVar16 + 1;
        }
        uStack_64c = 0x43fa0000;
        uStack_658 = 0x43fa0000;
        uStack_640 = 0x43fa0000;
        uStack_634 = 0x43fa0000;
        afStack_690[2] = -100.0;
        uStack_664 = 0xc2c80000;
        uStack_67c = 0xc2c80000;
        uStack_670 = 0xc2c80000;
        pCStack_6f4 = (CPrimBuffer *)
                      (uint)(8 < ((int)pCStack_6f0 - iStack_69c) * ((int)pCStack_6f0 - iStack_69c) +
                                 ((int)pCStack_6ec - iStack_6a0) * ((int)pCStack_6ec - iStack_6a0));
        fVar18 = (float10)(*(*piStack_6d0)->GetFOV_into_ST0)();
        if (fVar18 < (float10)FLOAT_100651fc) {
          pCStack_6f4 = (CPrimBuffer *)0x0;
        }
        pCStack_6f0 = (CShade *)
                      ((int)(pCStack_6f0->m_CameraFrustum).pad_004 + (int)puStack_6e0 + -0x18);
        if (pCStack_6f0 == pCStack_6c8) {
          pCStack_6f0 = pCStack_6c0;
          pCStack_6ec = (CShade *)((int)&pCStack_6ec->vptr + iStack_6bc);
        }
        if (iStack_698 == iVar5) {
          bVar3 = false;
        }
        else {
          pCVar6 = (CShade *)0x0;
          pPVar17 = pCStack_6b4->p_m_currentFrustumPlanes;
          do {
            bVar3 = true;
            pfVar11 = afStack_690 + 1;
            iVar13 = 8;
            do {
              if (_DAT_10065158 <=
                  *pfVar11 * pPVar17->nY + pfVar11[1] * pPVar17->nZ + pfVar11[-1] * pPVar17->nX +
                  pPVar17->d) {
                bVar3 = false;
              }
              pfVar11 = pfVar11 + 3;
              iVar13 = iVar13 + -1;
            } while (iVar13 != 0);
            if (bVar3) goto LAB_10011de5;
            pPVar17 = pPVar17 + 1;
            pCVar6 = (CShade *)((int)&pCVar6->vptr + 1);
          } while ((int)pCVar6 < 6);
        }
        if (pIStack_6d4 != (IShade *)0x0) {
          pfVar11 = afStack_690;
          cVar19 = (char)((uint)ADJ(this)->field9840_0x7c44 >> 0x10);
          (*(code *)ADJ(this)->Shade->vptr->method19)
                    (ADJ(this)->Shade,ADJ(this)->current_visible_primitives_count,ADJ(this)->meshes,
                     ADJ(this)->game_objects);
          if ((ADJ(this)->field9816_0x7b68[iVar5 * 3 + 1] != 0) ||
             (*(short *)(ADJ(this)->_1_data_ptr + 8 + ((int)pfVar11 + iVar5 * 0x13) * 2) == 0xff)) {
            pfVar11 = (float *)0x0;
          }
          pMVar21 = ADJ(this)->msh_02_data_ptr->elements +
                    *(ushort *)(ADJ(this)->_1_data_ptr + 8 + ((int)pfVar11 + iVar5 * 0x13) * 2);
          (*ADJ(this)->Shade->vptr->SetIsJointClippedByFrustum)(ADJ(this)->Shade,cVar19 == '\0');
          (*(code *)ADJ(this)->Shade->vptr->method3)
                    (ADJ(this)->Shade,0,pMVar21->count_in_07,
                     ADJ(this)->_b_data_ptr + (uint)(ushort)pMVar21->start_index_in_07 * 4,0);
          if (pfVar11 == (float *)0x0) {
            piVar8 = ADJ(this)->field9816_0x7b68 + iVar5 * 3;
            pCVar6 = (CShade *)0x0;
            if (0 < piVar8[1]) {
              do {
                iVar13 = *(int *)(*piVar8 + (int)pCVar6 * 4);
                if (ADJ(this)->unk_obj_array_elemsize_212[99][iVar13 + 0x60] == 0) {
                  ADJ(this)->unk_obj_array_elemsize_212[99][iVar13 + 0x60] = 1;
                  (*ADJ(this)->Shade->vptr->SetIsJointClippedByFrustum)(ADJ(this)->Shade,true);
                  (*(code *)ADJ(this)->Shade->vptr->method19)
                            (ADJ(this)->Shade,ADJ(this)->current_visible_primitives_count,
                             ADJ(this)->meshes,ADJ(this)->game_objects,ADJ(this)->field9840_0x7c44);
                  (*(code *)ADJ(this)->Shade->vptr->method3)
                            (ADJ(this)->Shade,0,
                             *(undefined4 *)
                              (ADJ(this)->unk_obj_array_elemsize_212[iVar13 + -1] + 0x68),
                             *(undefined4 *)
                              (ADJ(this)->unk_obj_array_elemsize_212[iVar13 + -1] + 0xd0),0);
                }
                pCVar6 = (CShade *)((int)&pCVar6->vptr + 1);
                piVar8 = ADJ(this)->field9816_0x7b68 + iVar5 * 3;
              } while ((int)pCVar6 < piVar8[1]);
            }
          }
        }
LAB_10011de5:
        puVar10 = ADJ(this)->field9818_0x7b70;
        iVar13 = 0;
        if (0 < (int)puVar10[iVar5 * 3 + 1]) {
          do {
            iVar15 = (**(code **)**(undefined4 **)(puVar10[iVar5 * 3] + iVar13 * 4))();
            if (iVar15 != 0) {
              pCVar24 = (CShade *)DAT_100708c0->IShadowProcessor;
              pCVar6 = DAT_100708c0;
              while (pCVar4 = pCVar24, pCVar4 != (CShade *)0x0) {
                if (pCVar4->PrimBufferOfBufferingCamera < pCStack_6e4) {
                  pCVar24 = (CShade *)pCVar4->activeICamera;
                }
                else {
                  pCVar24 = (CShade *)pCVar4->IShadeStatsBuilder;
                  pCVar6 = pCVar4;
                }
              }
              if ((pCVar6 == DAT_100708c0) || (pCStack_6e4 < pCVar6->PrimBufferOfBufferingCamera)) {
                pCStack_6b8 = DAT_100708c0;
                ppCVar12 = &pCStack_6b8;
              }
              else {
                ppCVar12 = (CShade **)&stack0xfffff908;
              }
              pCVar6 = *ppCVar12;
              if (DAT_100708c0 == pCVar6) {
                piVar8 = (int *)FUN_10038580(&DAT_100708c0,&iStack_694,(uint *)&pCStack_6e4);
                pCStack_6e8 = (CShade *)*piVar8;
                if ((pCStack_6e8 == DAT_100708c0) ||
                   (pCStack_6e4 < pCStack_6e8->PrimBufferOfBufferingCamera)) {
                  ppCVar23 = &pCStack_6a8;
                  pCStack_6a8 = pCStack_6e4;
                  uStack_6a4 = 0;
                  pCVar22 = pCStack_6e4;
                  FUN_10037e80(&stack0xfffff8ec,&pCStack_6e8);
                  puVar10 = (undefined4 *)
                            FUN_100158b0(&DAT_100708c0,&get_frustum_layer_config.usePass2,pCVar22,
                                         (uint *)ppCVar23);
                  pCStack_6e8 = (CShade *)*puVar10;
                }
                *(bool *)&(pCStack_6e8->m_CameraFrustum).cameraPosMatrixPtr = !bVar3;
              }
              else if (*(char *)&(pCVar6->m_CameraFrustum).cameraPosMatrixPtr == '\0') {
                *(bool *)&(pCVar6->m_CameraFrustum).cameraPosMatrixPtr = !bVar3;
              }
            }
            puVar10 = ADJ(this)->field9818_0x7b70;
            iVar13 = iVar13 + 1;
          } while (iVar13 < (int)puVar10[iVar5 * 3 + 1]);
        }
        iStack_6dc = iStack_6dc + -1;
      } while (iStack_6dc != 0);
    }
    (*(code *)ADJ(this)->Shade->vptr->EndMeshRender)();
    if ((piStack_6d0 == (IBufferingCamera **)0x0) &&
       (pCVar6 = (CShade *)DAT_100708c0->IShadeStatsBuilder, pCVar6 != DAT_100708c0)) {
      do {
        usePass2 = pCVar6->PrimBufferOfBufferingCamera->vptr;
        this_01 = this;
        if (*(char *)&(pCVar6->m_CameraFrustum).cameraPosMatrixPtr == '\0') {
          this_01 = (IMesh2_of_CLandscape)((uint)this | 1);
        }
        (*usePass2[2].GetLayer)(this_01,(BOOL)usePass2,(int)pCVar6->PrimBufferOfBufferingCamera);
        pCVar24 = (CShade *)pCVar6->activeICamera;
        if (pCVar24 == (CShade *)0x0) {
          pCVar24 = (CShade *)pCVar6->IShadowProcessor;
          if (pCVar6 == (CShade *)pCVar24->activeICamera) {
            do {
              pCVar6 = pCVar24;
              pCVar24 = (CShade *)pCVar6->IShadowProcessor;
            } while (pCVar6 == (CShade *)pCVar24->activeICamera);
          }
          if ((CShade *)pCVar6->activeICamera != pCVar24) {
            pCVar6 = pCVar24;
          }
        }
        else {
          for (pCVar4 = (CShade *)pCVar24->IShadeStatsBuilder; pCVar6 = pCVar24,
              pCVar4 != (CShade *)0x0; pCVar4 = (CShade *)pCVar4->IShadeStatsBuilder) {
            pCVar24 = pCVar4;
          }
        }
      } while (pCVar6 != DAT_100708c0);
    }
    (*(code *)ADJ(this)->Shade->vptr->method7)();
    (*(code *)(*ADJ(this)->ResolvedIEffectManager)->Render__)
              (ADJ(this)->ResolvedIEffectManager,uStack_4,0xfffffffe);
  }
  return;
}



---

// Assembly:
100115a0 SUB ESP,0x6f4
100115a6 PUSH EBX
100115a7 PUSH EBP
100115a8 MOV EBP,dword ptr [ESP + 0x700]
100115af PUSH ESI
100115b0 PUSH EDI
100115b1 MOV EAX,dword ptr [EBP + 0x7ae8]
100115b7 TEST EAX,EAX
100115b9 JNZ 0x10011fc1
100115bf MOV EBX,dword ptr [ESP + 0x710]
100115c6 MOV EDI,dword ptr [ESP + 0x714]
100115cd MOV EAX,EBX
100115cf MOV dword ptr [EBP + 0x7ae8],0x1
100115d9 AND EAX,0x40
100115dc MOV dword ptr [ESP + 0x40],EAX
100115e0 JNZ 0x10011628
100115e2 MOV EAX,dword ptr [EBP + 0x7a40]
100115e8 XOR ESI,ESI
100115ea TEST EAX,EAX
100115ec JLE 0x10011628
100115ee MOV EAX,dword ptr [EBP + 0x7a3c]
100115f4 LEA EDX,[ESP + 0x30]
100115f8 PUSH EDX
100115f9 MOV EDX,0x18
100115fe MOV ECX,dword ptr [EAX + ESI*0x4]
10011601 MOV EAX,dword ptr [ECX]
10011603 CALL dword ptr [EAX]
10011605 TEST EAX,EAX
10011607 JZ 0x1001161d
10011609 MOV EAX,dword ptr [ESP + 0x30]
1001160d MOV EDX,dword ptr [ESP + 0x70c]
10011614 PUSH EDI
10011615 PUSH EBX
10011616 MOV ECX,dword ptr [EAX]
10011618 PUSH EDX
10011619 PUSH EAX
1001161a CALL dword ptr [ECX + 0x2c]
1001161d MOV EAX,dword ptr [EBP + 0x7a40]
10011623 INC ESI
10011624 CMP ESI,EAX
10011626 JL 0x100115ee
10011628 XOR EAX,EAX
1001162a MOV dword ptr [ESP + 0x38],0x1
10011632 TEST BL,0x9
10011635 MOV dword ptr [EBP + 0x7b50],EAX
1001163b JZ 0x10011641
1001163d MOV dword ptr [ESP + 0x38],EAX
10011641 MOV ECX,dword ptr [EBP + 0x7ae0]
10011647 LEA ESI,[EBP + 0xfffffec8]
1001164d MOV EAX,ESI
1001164f LEA EDX,[EBP + 0x7b0c]
10011655 MOV EBX,dword ptr [ECX]
10011657 LEA EDI,[EBP + 0x7b08]
1001165d NEG EAX
1001165f PUSH EDX
10011660 LEA EDX,[EBP + 0x7b04]
10011666 SBB EAX,EAX
10011668 MOV dword ptr [ESP + 0x34],EBX
1001166c PUSH EDI
1001166d MOV EDI,dword ptr [ESP + 0x714]
10011674 XOR EBX,EBX
10011676 PUSH EDX
10011677 AND EAX,EBP
10011679 PUSH EBX
1001167a PUSH EDI
1001167b PUSH EAX
1001167c MOV EAX,dword ptr [ESP + 0x48]
10011680 PUSH ECX
10011681 CALL dword ptr [EAX + 0x58]
10011684 NEG ESI
10011686 MOV dword ptr [EBP + 0x7b00],EAX
1001168c MOV EDX,dword ptr [EBP + 0x7aa8]
10011692 MOV EAX,dword ptr [EBP + 0x7ae0]
10011698 PUSH EBX
10011699 SBB ESI,ESI
1001169b PUSH EBX
1001169c MOV ECX,dword ptr [EAX]
1001169e AND ESI,EBP
100116a0 PUSH EDX
100116a1 PUSH ESI
100116a2 PUSH EDI
100116a3 PUSH EAX
100116a4 CALL dword ptr [ECX + 0x14]
100116a7 MOV EAX,dword ptr [EDI]
100116a9 LEA ECX,[ESP + 0x3c]
100116ad PUSH ECX
100116ae MOV EDX,0x12
100116b3 MOV ECX,EDI
100116b5 CALL dword ptr [EAX]
100116b7 LEA EDX,[ESP + 0x34]
100116bb PUSH 0x100708d0
100116c0 MOV ECX,dword ptr [ESP + 0x40]
100116c4 LEA EAX,[ESP + 0x5c]
100116c8 PUSH EDX
100116c9 PUSH EAX
100116ca LEA EDX,[ESP + 0x68]
100116ce MOV dword ptr [ESP + 0x68],EBX
100116d2 MOV dword ptr [ESP + 0x6c],EBX
100116d6 MOV EAX,dword ptr [ECX]
100116d8 PUSH EBX
100116d9 PUSH EDX
100116da LEA EDX,[ESP + 0x1b0]
100116e1 CALL dword ptr [EAX + 0x4c]
100116e4 MOV ECX,EAX
100116e6 CALL 0x10058ed0
100116eb MOV ESI,dword ptr [ESP + 0x34]
100116ef MOV EDI,0x5
100116f4 MOV EBX,dword ptr [ESP + 0x40]
100116f8 MOV dword ptr [ESP + 0x2c],0x7f7fffff
10011700 MOV dword ptr [ESP + 0x28],0x7f7fffff
10011708 MOV dword ptr [ESP + 0x14],0x800000
10011710 MOV dword ptr [ESP + 0x24],0x800000
10011718 FLD float ptr [ESI]
1001171a FCOMP float ptr [ESP + 0x28]
1001171e FNSTSW AX
10011720 TEST AH,0x1
10011723 JZ 0x1001174e
10011725 MOV EAX,dword ptr [ESI]
10011727 SUB ESP,0x8
1001172a FLD float ptr [ESI]
1001172c MOV dword ptr [ESP + 0x30],EAX
10011730 MOV EAX,dword ptr [EBP + 0x7a28]
10011736 FSUB float ptr [EAX + 0x4]
10011739 FMUL float ptr [EAX + 0x14]
1001173c FSTP double ptr [ESP]
1001173f CALL 0x10059c87
10011744 ADD ESP,0x8
10011747 CALL 0x10001000
1001174c MOV EBX,EAX
1001174e FLD float ptr [ESI + 0x4]
10011751 FCOMP float ptr [ESP + 0x2c]
10011755 FNSTSW AX
10011757 TEST AH,0x1
1001175a JZ 0x10011789
1001175c MOV EAX,dword ptr [EBP + 0x7a28]
10011762 MOV ECX,dword ptr [ESI + 0x4]
10011765 FLD float ptr [ESI + 0x4]
10011768 FSUB float ptr [EAX + 0x8]
1001176b SUB ESP,0x8
1001176e MOV dword ptr [ESP + 0x34],ECX
10011772 FMUL float ptr [EAX + 0x18]
10011775 FSTP double ptr [ESP]
10011778 CALL 0x10059c87
1001177d ADD ESP,0x8
10011780 CALL 0x10001000
10011785 MOV dword ptr [ESP + 0x20],EAX
10011789 FLD float ptr [ESI]
1001178b FCOMP float ptr [ESP + 0x24]
1001178f FNSTSW AX
10011791 TEST AH,0x41
10011794 JNZ 0x100117c1
10011796 MOV EAX,dword ptr [EBP + 0x7a28]
1001179c MOV EDX,dword ptr [ESI]
1001179e FLD float ptr [ESI]
100117a0 FSUB float ptr [EAX + 0x4]
100117a3 SUB ESP,0x8
100117a6 MOV dword ptr [ESP + 0x2c],EDX
100117aa FMUL float ptr [EAX + 0x14]
100117ad FSTP double ptr [ESP]
100117b0 CALL 0x10059c87
100117b5 ADD ESP,0x8
100117b8 CALL 0x10001000
100117bd MOV dword ptr [ESP + 0x18],EAX
100117c1 FLD float ptr [ESI + 0x4]
100117c4 FCOMP float ptr [ESP + 0x14]
100117c8 FNSTSW AX
100117ca TEST AH,0x41
100117cd JNZ 0x100117fc
100117cf MOV EAX,dword ptr [ESI + 0x4]
100117d2 SUB ESP,0x8
100117d5 FLD float ptr [ESI + 0x4]
100117d8 MOV dword ptr [ESP + 0x1c],EAX
100117dc MOV EAX,dword ptr [EBP + 0x7a28]
100117e2 FSUB float ptr [EAX + 0x8]
100117e5 FMUL float ptr [EAX + 0x18]
100117e8 FSTP double ptr [ESP]
100117eb CALL 0x10059c87
100117f0 ADD ESP,0x8
100117f3 CALL 0x10001000
100117f8 MOV dword ptr [ESP + 0x1c],EAX
100117fc ADD ESI,0xc
100117ff DEC EDI
10011800 JNZ 0x10011718
10011806 TEST EBX,EBX
10011808 JGE 0x1001180c
1001180a XOR EBX,EBX
1001180c MOV EAX,dword ptr [EBP + 0x7a28]
10011812 MOV ECX,dword ptr [EAX + 0x1c]
10011815 CMP EBX,ECX
10011817 JL 0x1001181c
10011819 LEA EBX,[ECX + -0x1]
1001181c MOV EDX,dword ptr [ESP + 0x20]
10011820 TEST EDX,EDX
10011822 JGE 0x1001182c
10011824 MOV dword ptr [ESP + 0x20],0x0
1001182c MOV EDX,dword ptr [EAX + 0x20]
1001182f MOV ESI,dword ptr [ESP + 0x20]
10011833 CMP ESI,EDX
10011835 JL 0x1001183e
10011837 LEA ESI,[EDX + -0x1]
1001183a MOV dword ptr [ESP + 0x20],ESI
1001183e MOV ESI,dword ptr [ESP + 0x18]
10011842 TEST ESI,ESI
10011844 JGE 0x1001184c
10011846 XOR ESI,ESI
10011848 MOV dword ptr [ESP + 0x18],ESI
1001184c CMP ESI,ECX
1001184e JL 0x10011855
10011850 DEC ECX
10011851 MOV dword ptr [ESP + 0x18],ECX
10011855 MOV ECX,dword ptr [ESP + 0x1c]
10011859 TEST ECX,ECX
1001185b JGE 0x10011863
1001185d XOR ECX,ECX
1001185f MOV dword ptr [ESP + 0x1c],ECX
10011863 CMP ECX,EDX
10011865 JL 0x1001186c
10011867 DEC EDX
10011868 MOV dword ptr [ESP + 0x1c],EDX
1001186c MOV EDI,dword ptr [ESP + 0x1c]
10011870 MOV EDX,dword ptr [ESP + 0x20]
10011874 SUB EDI,EDX
10011876 MOV EDX,dword ptr [ESP + 0x34]
1001187a MOV ECX,dword ptr [ESP + 0x18]
1001187e SUB ESP,0x8
10011881 FLD float ptr [EDX]
10011883 FSUB float ptr [EAX + 0x4]
10011886 SUB ECX,EBX
10011888 INC EDI
10011889 INC ECX
1001188a IMUL EDI,ECX
1001188d FMUL float ptr [EAX + 0x14]
10011890 FSTP double ptr [ESP]
10011893 CALL 0x10059c87
10011898 CALL 0x10001000
1001189d MOV ECX,dword ptr [ESP + 0x3c]
100118a1 MOV ESI,EAX
100118a3 MOV EAX,dword ptr [EBP + 0x7a28]
100118a9 MOV dword ptr [ESP + 0x74],ESI
100118ad FLD float ptr [ECX + 0x4]
100118b0 FSUB float ptr [EAX + 0x8]
100118b3 FMUL float ptr [EAX + 0x18]
100118b6 FSTP double ptr [ESP]
100118b9 CALL 0x10059c87
100118be ADD ESP,0x8
100118c1 CALL 0x10001000
100118c6 MOV EDX,dword ptr [EBP + 0x7a28]
100118cc MOV dword ptr [ESP + 0x70],EAX
100118d0 MOV ECX,dword ptr [EDX + 0x20]
100118d3 IMUL ECX,ESI
100118d6 ADD ECX,EAX
100118d8 MOV dword ptr [ESP + 0x74],ECX
100118dc MOV ECX,dword ptr [ESP + 0x18]
100118e0 SUB ECX,ESI
100118e2 SUB ESI,EBX
100118e4 CMP ESI,ECX
100118e6 JG 0x100118f4
100118e8 MOV dword ptr [ESP + 0x50],0x1
100118f0 MOV ESI,EBX
100118f2 JMP 0x10011900
100118f4 MOV ESI,dword ptr [ESP + 0x18]
100118f8 MOV dword ptr [ESP + 0x50],0xffffffff
10011900 MOV ECX,dword ptr [ESP + 0x1c]
10011904 MOV EDX,dword ptr [ESP + 0x20]
10011908 MOV EBX,ECX
1001190a SUB EBX,EAX
1001190c SUB EAX,EDX
1001190e CMP EAX,EBX
10011910 JG 0x10011926
10011912 MOV EAX,EDX
10011914 MOV dword ptr [ESP + 0x2c],0x1
1001191c MOV dword ptr [ESP + 0x4c],EAX
10011920 MOV dword ptr [ESP + 0x24],ECX
10011924 JMP 0x10011938
10011926 MOV EAX,ECX
10011928 MOV dword ptr [ESP + 0x2c],0xffffffff
10011930 MOV dword ptr [ESP + 0x4c],EAX
10011934 MOV dword ptr [ESP + 0x24],EDX
10011938 MOV dword ptr [ESP + 0x1c],EAX
1001193c MOV AL,[0x100708cc]
10011941 TEST AL,0x1
10011943 MOV dword ptr [ESP + 0x20],ESI
10011947 JNZ 0x1001196b
10011949 MOV DL,AL
1001194b MOV ECX,0x100708c0
10011950 OR DL,0x1
10011953 MOV byte ptr [0x100708cc],DL
10011959 CALL 0x100156f0
1001195e PUSH 0x10011fd0
10011963 CALL 0x100596c8
10011968 ADD ESP,0x4
1001196b MOV EAX,[0x100708c4]
10011970 XOR ESI,ESI
10011972 CMP EAX,ESI
10011974 JZ 0x100119a9
10011976 MOV EDX,dword ptr [0x100708c0]
1001197c MOV ECX,0x100708c0
10011981 MOV EAX,dword ptr [EDX + 0x4]
10011984 PUSH EAX
10011985 CALL 0x10015d60
1001198a MOV EAX,[0x100708c0]
1001198f MOV dword ptr [EAX + 0x8],EAX
10011992 MOV ECX,dword ptr [0x100708c0]
10011998 MOV dword ptr [ECX + 0x4],ESI
1001199b MOV EAX,[0x100708c0]
100119a0 MOV dword ptr [EAX + 0xc],EAX
100119a3 MOV dword ptr [0x100708c4],ESI
100119a9 MOV EDX,dword ptr [EBP + 0xfffffecc]
100119af LEA EAX,[EBP + 0xfffffecc]
100119b5 LEA ECX,[ESP + 0x48]
100119b9 PUSH ECX
100119ba PUSH EAX
100119bb CALL dword ptr [EDX + 0x18]
100119be MOV EBX,EAX
100119c0 XOR ESI,ESI
100119c2 TEST EBX,EBX
100119c4 JLE 0x10011a6f
100119ca MOV EDX,dword ptr [ESP + 0x48]
100119ce MOV ECX,dword ptr [EDX + ESI*0x4]
100119d1 MOV EAX,dword ptr [ECX]
100119d3 CALL dword ptr [EAX + 0x2c]
100119d6 CMP EAX,0x9
100119d9 JNZ 0x10011a66
100119df MOV ECX,dword ptr [ESP + 0x48]
100119e3 LEA EDX,[ESP + 0x18]
100119e7 PUSH EDX
100119e8 MOV EDX,0x18
100119ed MOV ECX,dword ptr [ECX + ESI*0x4]
100119f0 MOV EAX,dword ptr [ECX]
100119f2 CALL dword ptr [EAX]
100119f4 TEST EAX,EAX
100119f6 JZ 0x10011a66
100119f8 LEA EAX,[ESP + 0x18]
100119fc LEA ECX,[ESP + 0x54]
10011a00 PUSH EAX
10011a01 PUSH ECX
10011a02 MOV ECX,0x100708c0
10011a07 CALL 0x10038580
10011a0c MOV EAX,dword ptr [EAX]
10011a0e MOV ECX,dword ptr [0x100708c0]
10011a14 CMP EAX,ECX
10011a16 MOV dword ptr [ESP + 0x14],EAX
10011a1a JZ 0x10011a27
10011a1c MOV EDX,dword ptr [ESP + 0x18]
10011a20 MOV ECX,dword ptr [EAX + 0x10]
10011a23 CMP EDX,ECX
10011a25 JNC 0x10011a62
10011a27 LEA EAX,[ESP + 0x12]
10011a2b LEA ECX,[ESP + 0x18]
10011a2f PUSH EAX
10011a30 PUSH ECX
10011a31 LEA ECX,[ESP + 0x6c]
10011a35 MOV byte ptr [ESP + 0x1a],0x0
10011a3a CALL 0x10015900
10011a3f PUSH EAX
10011a40 PUSH ECX
10011a41 LEA EDX,[ESP + 0x1c]
10011a45 MOV ECX,ESP
10011a47 PUSH EDX
10011a48 CALL 0x10037e80
10011a4d LEA EAX,[ESP + 0x4c]
10011a51 MOV ECX,0x100708c0
10011a56 PUSH EAX
10011a57 CALL 0x100158b0
10011a5c MOV EAX,dword ptr [EAX]
10011a5e MOV dword ptr [ESP + 0x14],EAX
10011a62 MOV byte ptr [EAX + 0x14],0x0
10011a66 INC ESI
10011a67 CMP ESI,EBX
10011a69 JL 0x100119ca
10011a6f TEST EDI,EDI
10011a71 JLE 0x10011f07
10011a77 MOV ECX,dword ptr [ESP + 0x24]
10011a7b MOV EDX,dword ptr [ESP + 0x2c]
10011a7f ADD ECX,EDX
10011a81 MOV dword ptr [ESP + 0x30],EDI
10011a85 MOV dword ptr [ESP + 0x44],ECX
10011a89 MOV EAX,dword ptr [EBP + 0x7a28]
10011a8f MOV EDX,dword ptr [ESP + 0x1c]
10011a93 MOV EBX,dword ptr [EAX + 0x20]
10011a96 XOR EAX,EAX
10011a98 IMUL EBX,dword ptr [ESP + 0x20]
10011a9d ADD EBX,EDX
10011a9f LEA ECX,[EBX + EBX*0x8]
10011aa2 LEA EDX,[EBX + ECX*0x2]
10011aa5 MOV ECX,dword ptr [EBP + 0x10]
10011aa8 MOV AX,word ptr [ECX + EDX*0x2 + 0x8]
10011aad MOV EDX,EAX
10011aaf SHL EDX,0x4
10011ab2 ADD EDX,EAX
10011ab4 MOV EAX,dword ptr [EBP + 0x14]
10011ab7 LEA EAX,[EAX + EDX*0x4 + 0x94]
10011abe LEA ECX,[EAX + 0xc]
10011ac1 PUSH ECX
10011ac2 PUSH EAX
10011ac3 LEA ECX,[ESP + 0x144]
10011aca CALL 0x10015580
10011acf MOV ECX,0x18
10011ad4 MOV ESI,EAX
10011ad6 LEA EDI,[ESP + 0xdc]
10011add MOV EAX,dword ptr [ESP + 0x20]
10011ae1 MOVSD.REP ES:EDI,ESI
10011ae3 MOV ECX,0x18
10011ae8 LEA ESI,[ESP + 0xdc]
10011aef LEA EDI,[ESP + 0x7c]
10011af3 MOVSD.REP ES:EDI,ESI
10011af5 MOV ECX,dword ptr [ESP + 0x6c]
10011af9 MOV EDI,dword ptr [ESP + 0x70]
10011afd SUB EAX,ECX
10011aff MOV ECX,dword ptr [ESP + 0x1c]
10011b03 SUB ECX,EDI
10011b05 MOV dword ptr [ESP + 0xc0],0x43fa0000
10011b10 MOV EDX,ECX
10011b12 MOV dword ptr [ESP + 0xb4],0x43fa0000
10011b1d IMUL EDX,ECX
10011b20 MOV ECX,EAX
10011b22 MOV dword ptr [ESP + 0xcc],0x43fa0000
10011b2d IMUL ECX,EAX
10011b30 ADD EDX,ECX
10011b32 MOV ECX,dword ptr [ESP + 0x3c]
10011b36 XOR EAX,EAX
10011b38 CMP EDX,0x9
10011b3b MOV dword ptr [ESP + 0xd8],0x43fa0000
10011b46 MOV dword ptr [ESP + 0x84],0xc2c80000
10011b51 MOV dword ptr [ESP + 0xa8],0xc2c80000
10011b5c MOV dword ptr [ESP + 0x90],0xc2c80000
10011b67 MOV dword ptr [ESP + 0x9c],0xc2c80000
10011b72 MOV EDX,dword ptr [ECX]
10011b74 SETGE AL
10011b77 MOV dword ptr [ESP + 0x18],EAX
10011b7b CALL dword ptr [EDX + 0x44]
10011b7e FCOMP float ptr [0x100651fc]
10011b84 FNSTSW AX
10011b86 TEST AH,0x1
10011b89 JZ 0x10011b93
10011b8b MOV dword ptr [ESP + 0x18],0x0
10011b93 MOV EAX,dword ptr [ESP + 0x1c]
10011b97 MOV EDX,dword ptr [ESP + 0x2c]
10011b9b MOV ECX,dword ptr [ESP + 0x44]
10011b9f ADD EAX,EDX
10011ba1 CMP EAX,ECX
10011ba3 MOV dword ptr [ESP + 0x1c],EAX
10011ba7 JNZ 0x10011bbf
10011ba9 MOV EAX,dword ptr [ESP + 0x4c]
10011bad MOV ECX,dword ptr [ESP + 0x50]
10011bb1 MOV dword ptr [ESP + 0x1c],EAX
10011bb5 MOV EAX,dword ptr [ESP + 0x20]
10011bb9 ADD EAX,ECX
10011bbb MOV dword ptr [ESP + 0x20],EAX
10011bbf CMP dword ptr [ESP + 0x74],EBX
10011bc3 JZ 0x10011c40
10011bc5 MOV EDX,dword ptr [ESP + 0x58]
10011bc9 MOV byte ptr [ESP + 0x12],0x1
10011bce MOV dword ptr [ESP + 0x14],0x0
10011bd6 MOV EDI,dword ptr [EDX + 0x1f0]
10011bdc MOV DL,0x1
10011bde LEA ECX,[ESP + 0x80]
10011be5 MOV byte ptr [ESP + 0x13],DL
10011be9 MOV ESI,0x8
10011bee FLD float ptr [ECX + -0x4]
10011bf1 FMUL float ptr [EDI]
10011bf3 FLD float ptr [ECX + 0x4]
10011bf6 FMUL float ptr [EDI + 0x8]
10011bf9 FADDP
10011bfb FLD float ptr [ECX]
10011bfd FMUL float ptr [EDI + 0x4]
10011c00 FADDP
10011c02 FADD float ptr [EDI + 0xc]
10011c05 FCOMP float ptr [0x10065158]
10011c0b FNSTSW AX
10011c0d TEST AH,0x1
10011c10 JNZ 0x10011c16
10011c12 XOR DL,DL
10011c14 JMP 0x10011c1b
10011c16 MOV byte ptr [ESP + 0x12],0x0
10011c1b ADD ECX,0xc
10011c1e DEC ESI
10011c1f JNZ 0x10011bee
10011c21 TEST DL,DL
10011c23 MOV byte ptr [ESP + 0x13],DL
10011c27 JNZ 0x10011de5
10011c2d MOV EAX,dword ptr [ESP + 0x14]
10011c31 ADD EDI,0x10
10011c34 INC EAX
10011c35 CMP EAX,0x6
10011c38 MOV dword ptr [ESP + 0x14],EAX
10011c3c JL 0x10011bdc
10011c3e JMP 0x10011c4a
10011c40 MOV byte ptr [ESP + 0x13],0x0
10011c45 MOV byte ptr [ESP + 0x12],0x0
10011c4a MOV EAX,dword ptr [ESP + 0x38]
10011c4e TEST EAX,EAX
10011c50 JZ 0x10011de5
10011c56 LEA EDX,[ESP + 0x7c]
10011c5a MOV EAX,dword ptr [EBP + 0x7ae0]
10011c60 PUSH EDX
10011c61 MOV EDX,dword ptr [EBP + 0x7b0c]
10011c67 PUSH 0x0
10011c69 PUSH EDX
10011c6a MOV EDX,dword ptr [EBP + 0x7b08]
10011c70 MOV ECX,dword ptr [EAX]
10011c72 PUSH EDX
10011c73 MOV EDX,dword ptr [EBP + 0x7b04]
10011c79 PUSH EDX
10011c7a MOV EDX,dword ptr [EBP + 0x7b00]
10011c80 PUSH EDX
10011c81 PUSH EAX
10011c82 CALL dword ptr [ECX + 0x50]
10011c85 MOV EAX,dword ptr [EBP + 0x7a30]
10011c8b LEA EDI,[EBX + EBX*0x2]
10011c8e MOV ESI,dword ptr [ESP + 0x18]
10011c92 SHL EDI,0x2
10011c95 MOV ECX,dword ptr [EDI + EAX*0x1 + 0x4]
10011c99 TEST ECX,ECX
10011c9b JNZ 0x10011cb3
10011c9d MOV EDX,ESI
10011c9f LEA ECX,[EBX + EBX*0x8]
10011ca2 ADD EDX,EBX
10011ca4 LEA EAX,[EDX + ECX*0x2]
10011ca7 MOV ECX,dword ptr [EBP + 0x10]
10011caa CMP word ptr [ECX + EAX*0x2 + 0x8],0xff
10011cb1 JNZ 0x10011cb5
10011cb3 XOR ESI,ESI
10011cb5 LEA EDX,[EBX + EBX*0x8]
10011cb8 LEA EAX,[ESI + EBX*0x1]
10011cbb LEA ECX,[EAX + EDX*0x2]
10011cbe MOV EDX,dword ptr [EBP + 0x10]
10011cc1 XOR EAX,EAX
10011cc3 MOV AX,word ptr [EDX + ECX*0x2 + 0x8]
10011cc8 MOV EDX,dword ptr [EBP + 0x14]
10011ccb MOV ECX,EAX
10011ccd SHL ECX,0x4
10011cd0 ADD ECX,EAX
10011cd2 LEA EAX,[EDX + ECX*0x4 + 0x8c]
10011cd9 MOV DL,byte ptr [ESP + 0x12]
10011cdd MOV dword ptr [ESP + 0x14],EAX
10011ce1 MOV EAX,dword ptr [EBP + 0x7ae0]
10011ce7 TEST DL,DL
10011ce9 MOV ECX,dword ptr [EAX]
10011ceb SETZ DL
10011cee PUSH EDX
10011cef PUSH EAX
10011cf0 CALL dword ptr [ECX + 0x3c]
10011cf3 MOV EAX,dword ptr [EBP + 0x7ae0]
10011cf9 XOR EDX,EDX
10011cfb PUSH 0x0
10011cfd MOV ECX,dword ptr [EAX]
10011cff MOV EAX,dword ptr [ESP + 0x18]
10011d03 MOV DX,word ptr [EAX]
10011d06 MOV EAX,dword ptr [EBP + 0x18]
10011d09 LEA EDX,[EAX + EDX*0x4]
10011d0c XOR EAX,EAX
10011d0e PUSH EDX
10011d0f MOV EDX,dword ptr [ESP + 0x1c]
10011d13 MOV AX,word ptr [EDX + 0x2]
10011d17 PUSH EAX
10011d18 MOV EAX,dword ptr [EBP + 0x7ae0]
10011d1e PUSH 0x0
10011d20 PUSH EAX
10011d21 CALL dword ptr [ECX + 0x10]
10011d24 TEST ESI,ESI
10011d26 JNZ 0x10011de5
10011d2c MOV EAX,dword ptr [EBP + 0x7a30]
10011d32 XOR ECX,ECX
10011d34 ADD EAX,EDI
10011d36 MOV dword ptr [ESP + 0x14],ECX
10011d3a MOV EDX,dword ptr [EAX + 0x4]
10011d3d TEST EDX,EDX
10011d3f JLE 0x10011de5
10011d45 MOV EDX,dword ptr [EAX]
10011d47 MOV ESI,dword ptr [EDX + ECX*0x4]
10011d4a MOV AL,byte ptr [ESI + EBP*0x1 + 0x5420]
10011d51 TEST AL,AL
10011d53 JNZ 0x10011dcf
10011d55 MOV byte ptr [ESI + EBP*0x1 + 0x5420],0x1
10011d5d MOV EAX,dword ptr [EBP + 0x7ae0]
10011d63 PUSH 0x1
10011d65 PUSH EAX
10011d66 MOV ECX,dword ptr [EAX]
10011d68 CALL dword ptr [ECX + 0x3c]
10011d6b LEA ECX,[ESI + ESI*0x2]
10011d6e MOV EAX,dword ptr [EBP + 0x7ae0]
10011d74 LEA EDX,[ECX + ECX*0x8]
10011d77 MOV ECX,dword ptr [EAX]
10011d79 SHL EDX,0x1
10011d7b SUB EDX,ESI
10011d7d LEA ESI,[EBP + EDX*0x4 + 0xfffffec8]
10011d84 LEA EDX,[ESI + 0x2fc]
10011d8a PUSH EDX
10011d8b MOV EDX,dword ptr [EBP + 0x7b0c]
10011d91 PUSH 0x0
10011d93 PUSH EDX
10011d94 MOV EDX,dword ptr [EBP + 0x7b08]
10011d9a PUSH EDX
10011d9b MOV EDX,dword ptr [EBP + 0x7b04]
10011da1 PUSH EDX
10011da2 MOV EDX,dword ptr [EBP + 0x7b00]
10011da8 PUSH EDX
10011da9 PUSH EAX
10011daa CALL dword ptr [ECX + 0x50]
10011dad MOV EDX,dword ptr [ESI + 0x2f8]
10011db3 MOV EAX,dword ptr [EBP + 0x7ae0]
10011db9 PUSH 0x0
10011dbb PUSH EDX
10011dbc MOV EDX,dword ptr [ESI + 0x290]
10011dc2 MOV ECX,dword ptr [EAX]
10011dc4 PUSH EDX
10011dc5 PUSH 0x0
10011dc7 PUSH EAX
10011dc8 CALL dword ptr [ECX + 0x10]
10011dcb MOV ECX,dword ptr [ESP + 0x14]
10011dcf MOV EAX,dword ptr [EBP + 0x7a30]
10011dd5 INC ECX
10011dd6 ADD EAX,EDI
10011dd8 MOV dword ptr [ESP + 0x14],ECX
10011ddc CMP ECX,dword ptr [EAX + 0x4]
10011ddf JL 0x10011d45
10011de5 MOV ECX,dword ptr [EBP + 0x7a38]
10011deb LEA EBX,[EBX + EBX*0x2]
10011dee SHL EBX,0x2
10011df1 XOR EDI,EDI
10011df3 LEA EAX,[EBX + ECX*0x1]
10011df6 MOV ECX,dword ptr [EBX + ECX*0x1 + 0x4]
10011dfa TEST ECX,ECX
10011dfc JLE 0x10011ef8
10011e02 MOV EDX,dword ptr [EAX]
10011e04 MOV ECX,dword ptr [EDX + EDI*0x4]
10011e07 LEA EDX,[ESP + 0x28]
10011e0b PUSH EDX
10011e0c MOV EDX,0x18
10011e11 MOV EAX,dword ptr [ECX]
10011e13 CALL dword ptr [EAX]
10011e15 TEST EAX,EAX
10011e17 JZ 0x10011ee2
10011e1d MOV EDX,dword ptr [0x100708c0]
10011e23 MOV ESI,dword ptr [ESP + 0x28]
10011e27 MOV ECX,EDX
10011e29 MOV EAX,dword ptr [EDX + 0x4]
10011e2c TEST EAX,EAX
10011e2e JZ 0x10011e43
10011e30 CMP dword ptr [EAX + 0x10],ESI
10011e33 JC 0x10011e3c
10011e35 MOV ECX,EAX
10011e37 MOV EAX,dword ptr [EAX + 0x8]
10011e3a JMP 0x10011e3f
10011e3c MOV EAX,dword ptr [EAX + 0xc]
10011e3f TEST EAX,EAX
10011e41 JNZ 0x10011e30
10011e43 CMP ECX,EDX
10011e45 MOV dword ptr [ESP + 0x14],ECX
10011e49 JZ 0x10011e56
10011e4b CMP ESI,dword ptr [ECX + 0x10]
10011e4e JC 0x10011e56
10011e50 LEA EAX,[ESP + 0x14]
10011e54 JMP 0x10011e5e
10011e56 MOV dword ptr [ESP + 0x54],EDX
10011e5a LEA EAX,[ESP + 0x54]
10011e5e MOV EAX,dword ptr [EAX]
10011e60 CMP EDX,EAX
10011e62 JNZ 0x10011ecf
10011e64 LEA EAX,[ESP + 0x28]
10011e68 LEA ECX,[ESP + 0x78]
10011e6c PUSH EAX
10011e6d PUSH ECX
10011e6e MOV ECX,0x100708c0
10011e73 CALL 0x10038580
10011e78 MOV EAX,dword ptr [EAX]
10011e7a MOV ECX,dword ptr [0x100708c0]
10011e80 CMP EAX,ECX
10011e82 MOV ECX,dword ptr [ESP + 0x28]
10011e86 MOV dword ptr [ESP + 0x24],EAX
10011e8a JZ 0x10011e91
10011e8c CMP ECX,dword ptr [EAX + 0x10]
10011e8f JNC 0x10011ec1
10011e91 LEA EDX,[ESP + 0x64]
10011e95 MOV dword ptr [ESP + 0x64],ECX
10011e99 PUSH EDX
10011e9a PUSH ECX
10011e9b LEA EAX,[ESP + 0x2c]
10011e9f MOV ECX,ESP
10011ea1 PUSH EAX
10011ea2 MOV byte ptr [ESP + 0x74],0x0
10011ea7 CALL 0x10037e80
10011eac LEA ECX,[ESP + 0x64]
10011eb0 PUSH ECX
10011eb1 MOV ECX,0x100708c0
10011eb6 CALL 0x100158b0
10011ebb MOV EAX,dword ptr [EAX]
10011ebd MOV dword ptr [ESP + 0x24],EAX
10011ec1 MOV CL,byte ptr [ESP + 0x13]
10011ec5 TEST CL,CL
10011ec7 SETZ DL
10011eca MOV byte ptr [EAX + 0x14],DL
10011ecd JMP 0x10011ee2
10011ecf MOV CL,byte ptr [EAX + 0x14]
10011ed2 TEST CL,CL
10011ed4 JNZ 0x10011ee2
10011ed6 MOV CL,byte ptr [ESP + 0x13]
10011eda TEST CL,CL
10011edc SETZ CL
10011edf MOV byte ptr [EAX + 0x14],CL
10011ee2 MOV EDX,dword ptr [EBP + 0x7a38]
10011ee8 INC EDI
10011ee9 MOV ECX,dword ptr [EBX + EDX*0x1 + 0x4]
10011eed LEA EAX,[EBX + EDX*0x1]
10011ef0 CMP EDI,ECX
10011ef2 JL 0x10011e02
10011ef8 MOV EAX,dword ptr [ESP + 0x30]
10011efc DEC EAX
10011efd MOV dword ptr [ESP + 0x30],EAX
10011f01 JNZ 0x10011a89
10011f07 MOV EAX,dword ptr [EBP + 0x7ae0]
10011f0d MOV EBX,dword ptr [ESP + 0x710]
10011f14 PUSH EAX
10011f15 MOV ECX,dword ptr [EAX]
10011f17 CALL dword ptr [ECX + 0x48]
10011f1a MOV EAX,dword ptr [ESP + 0x40]
10011f1e TEST EAX,EAX
10011f20 JNZ 0x10011f9b
10011f22 MOV EAX,[0x100708c0]
10011f27 MOV ESI,dword ptr [EAX + 0x8]
10011f2a CMP ESI,EAX
10011f2c JZ 0x10011f9b
10011f2e MOV EDI,dword ptr [ESP + 0x70c]
10011f35 MOV AL,byte ptr [ESI + 0x14]
10011f38 MOV ECX,dword ptr [ESP + 0x714]
10011f3f TEST AL,AL
10011f41 MOV EAX,dword ptr [ESI + 0x10]
10011f44 PUSH ECX
10011f45 MOV EDX,dword ptr [EAX]
10011f47 JZ 0x10011f4c
10011f49 PUSH EBX
10011f4a JMP 0x10011f52
10011f4c MOV ECX,EBX
10011f4e OR ECX,0x1
10011f51 PUSH ECX
10011f52 PUSH EDI
10011f53 PUSH EAX
10011f54 CALL dword ptr [EDX + 0x2c]
10011f57 MOV ECX,dword ptr [ESI + 0xc]
10011f5a MOV EAX,ESI
10011f5c TEST ECX,ECX
10011f5e JZ 0x10011f74
10011f60 MOV EAX,ECX
10011f62 MOV ECX,dword ptr [EAX + 0x8]
10011f65 TEST ECX,ECX
10011f67 JZ 0x10011f8d
10011f69 MOV EAX,ECX
10011f6b MOV ECX,dword ptr [EAX + 0x8]
10011f6e TEST ECX,ECX
10011f70 JNZ 0x10011f69
10011f72 JMP 0x10011f8d
10011f74 MOV ECX,dword ptr [ESI + 0x4]
10011f77 CMP ESI,dword ptr [ECX + 0xc]
10011f7a JNZ 0x10011f86
10011f7c MOV EAX,ECX
10011f7e MOV ECX,dword ptr [ECX + 0x4]
10011f81 CMP EAX,dword ptr [ECX + 0xc]
10011f84 JZ 0x10011f7c
10011f86 CMP dword ptr [EAX + 0xc],ECX
10011f89 JZ 0x10011f8d
10011f8b MOV EAX,ECX
10011f8d MOV ECX,dword ptr [0x100708c0]
10011f93 MOV ESI,EAX
10011f95 CMP EAX,ECX
10011f97 JNZ 0x10011f35
10011f99 JMP 0x10011fa2
10011f9b MOV EDI,dword ptr [ESP + 0x70c]
10011fa2 MOV ECX,dword ptr [EBP + 0x7ae0]
10011fa8 XOR EDX,EDX
10011faa MOV EAX,dword ptr [ECX]
10011fac CALL dword ptr [EAX + 0x20]
10011faf MOV EBP,dword ptr [EBP + 0x7ab0]
10011fb5 PUSH 0x1
10011fb7 PUSH -0x2
10011fb9 PUSH EDI
10011fba MOV ECX,dword ptr [EBP]
10011fbd PUSH EBP
10011fbe CALL dword ptr [ECX + 0xc]
10011fc1 POP EDI
10011fc2 POP ESI
10011fc3 POP EBP
10011fc4 POP EBX
10011fc5 ADD ESP,0x6f4
10011fcb RET 0x10