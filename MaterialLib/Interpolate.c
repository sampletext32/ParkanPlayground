
void __cdecl
Interpolate(MaterialExternal *src1_ptr,MaterialExternal *src2_ptr,float progress,
           MaterialExternal *dst_ptr,uint targetFlags)

{
  if ((targetFlags & 2) == 0) {
    (dst_ptr->stage).diffuse.R = (src1_ptr->stage).diffuse.R;
    (dst_ptr->stage).diffuse.G = (src1_ptr->stage).diffuse.G;
    (dst_ptr->stage).diffuse.B = (src1_ptr->stage).diffuse.B;
  }
  else {
    (dst_ptr->stage).diffuse.R =
         ((src2_ptr->stage).diffuse.R - (src1_ptr->stage).diffuse.R) * progress +
         (src1_ptr->stage).diffuse.R;
    (dst_ptr->stage).diffuse.G =
         ((src2_ptr->stage).diffuse.G - (src1_ptr->stage).diffuse.G) * progress +
         (src1_ptr->stage).diffuse.G;
    (dst_ptr->stage).diffuse.B =
         ((src2_ptr->stage).diffuse.B - (src1_ptr->stage).diffuse.B) * progress +
         (src1_ptr->stage).diffuse.B;
  }
  if ((targetFlags & 1) == 0) {
    (dst_ptr->stage).ambient.R = (src1_ptr->stage).ambient.R;
    (dst_ptr->stage).ambient.G = (src1_ptr->stage).ambient.G;
    (dst_ptr->stage).ambient.B = (src1_ptr->stage).ambient.B;
  }
  else {
    (dst_ptr->stage).ambient.R =
         ((src2_ptr->stage).ambient.R - (src1_ptr->stage).ambient.R) * progress +
         (src1_ptr->stage).ambient.R;
    (dst_ptr->stage).ambient.G =
         ((src2_ptr->stage).ambient.G - (src1_ptr->stage).ambient.G) * progress +
         (src1_ptr->stage).ambient.G;
    (dst_ptr->stage).ambient.B =
         ((src2_ptr->stage).ambient.B - (src1_ptr->stage).ambient.B) * progress +
         (src1_ptr->stage).ambient.B;
  }
  if ((targetFlags & 4) == 0) {
    (dst_ptr->stage).specular.R = (src1_ptr->stage).specular.R;
    (dst_ptr->stage).specular.G = (src1_ptr->stage).specular.G;
    (dst_ptr->stage).specular.B = (src1_ptr->stage).specular.B;
  }
  else {
    (dst_ptr->stage).specular.R =
         ((src2_ptr->stage).specular.R - (src1_ptr->stage).specular.R) * progress +
         (src1_ptr->stage).specular.R;
    (dst_ptr->stage).specular.G =
         ((src2_ptr->stage).specular.G - (src1_ptr->stage).specular.G) * progress +
         (src1_ptr->stage).specular.G;
    (dst_ptr->stage).specular.B =
         ((src2_ptr->stage).specular.B - (src1_ptr->stage).specular.B) * progress +
         (src1_ptr->stage).specular.B;
  }
  if ((targetFlags & 8) == 0) {
    (dst_ptr->stage).emissive.R = (src1_ptr->stage).emissive.R;
    (dst_ptr->stage).emissive.G = (src1_ptr->stage).emissive.G;
    (dst_ptr->stage).emissive.B = (src1_ptr->stage).emissive.B;
  }
  else {
    (dst_ptr->stage).emissive.R =
         ((src2_ptr->stage).emissive.R - (src1_ptr->stage).emissive.R) * progress +
         (src1_ptr->stage).emissive.R;
    (dst_ptr->stage).emissive.G =
         ((src2_ptr->stage).emissive.G - (src1_ptr->stage).emissive.G) * progress +
         (src1_ptr->stage).emissive.G;
    (dst_ptr->stage).emissive.B =
         ((src2_ptr->stage).emissive.B - (src1_ptr->stage).emissive.B) * progress +
         (src1_ptr->stage).emissive.B;
  }
  if ((targetFlags & 0x10) != 0) {
    (dst_ptr->stage).ambient.A =
         ((src2_ptr->stage).ambient.A - (src1_ptr->stage).ambient.A) * progress +
         (src1_ptr->stage).ambient.A;
    (dst_ptr->stage).Power = (src1_ptr->stage).Power;
    return;
  }
  (dst_ptr->stage).ambient.A = (src1_ptr->stage).ambient.A;
  (dst_ptr->stage).Power = (src1_ptr->stage).Power;
  return;
}

