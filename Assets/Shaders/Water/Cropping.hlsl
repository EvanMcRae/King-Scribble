//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

float ObjCropping_float(float3 Pos, Texture2D Objs, float Num, out float Out)
{
    float top, left, right;
    for (int i = 0; i < Num; i++)
    {
        // Cache info/sign colors
        float4 info = Objs.Load(int3(i, 0, 0));
        float4 sign = Objs.Load(int3(i, 1, 0));

        // Decode the Objs array from the Texture2D
        // R = top bound
        top = info[0] * 255;
        if (sign[0] < 1) top *= -1;
        // G = left bound
        left = info[1] * 255;
        if (sign[1] < 1) left *= -1;
        // B = right bound
        right = info[2] * 255;
        if (sign[2] < 1) right *= -1;

        // If under the top bound and between the left and right bounds, crop
        if ((Pos[1] < top) && (Pos[0] > left) && (Pos[0] < right))
        {
            Out = 0;
            return Out;
        }
    }
    Out = 1;
    return Out;
}

float ObjCroppingOld_float(float3 Pos, float Top, float Left, float Right, out float Out)
{
    if ((Pos[1] < Top) && (Pos[0] > Left) && (Pos[0] < Right))
    {
        Out = 0;
        return Out;
    }
    Out = 1;
    return Out;
}

#endif //MYHLSLINCLUDE_INCLUDED