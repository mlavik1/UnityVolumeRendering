float3 ObjSpaceViewDir(float3 v)
{
    return TransformWorldToObject(GetCameraPositionWS()) - v;
}

float4 UnityObjectToClipPos(float3 pos)
{
    return TransformObjectToHClip(pos);
}
