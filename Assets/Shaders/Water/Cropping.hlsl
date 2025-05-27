//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

float ObjCropping_float(float3 Pos, UnityTexture2D Objs, float Num, out float Out)
{
    float top, left, right;
    for (int i = 0; i < Num; i++)
    {
        
        // Decode the Objs array from the UnityTexture2D
        // R = top bound
        top = Objs.Load(int3(i, 0, 0))[0] * 255;
        if (Objs.Load(int3(i, 1, 0))[0] < 1) top *= -1;
        // G = left bound
        left = Objs.Load(int3(i, 0, 0))[1] * 255;
        if (Objs.Load(int3(i, 1, 0))[1] < 1) left *= -1;
        // B = right bound
        right = Objs.Load(int3(i, 0, 0))[2] * 255;
        if (Objs.Load(int3(i, 1, 0))[2] < 1) right *= -1;

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

#endif //MYHLSLINCLUDE_INCLUDED